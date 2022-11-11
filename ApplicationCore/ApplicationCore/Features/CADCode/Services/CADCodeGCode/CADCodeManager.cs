using CADCode;
using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Services.Domain;
using ApplicationCore.Features.CADCode.Services.Domain.CADCode;
using ApplicationCore.Features.CADCode.Services.Domain.CADCode.Configuration;
using ApplicationCore.Features.CADCode.Services.Domain.Inventory;
using Part = CADCode.Part;

namespace ApplicationCore.Features.CADCode.Services.Services.CADCodeGCode;

public class CADCodeManager {

    private readonly Dictionary<PartMaterial, List<CNCPart>> _partsByMaterial = new();
    private readonly UnitTypes _units = UnitTypes.CC_U_METRIC;
    private readonly IEnumerable<InventoryItem> _availableInventory;
    private readonly string _jobName;
    private CADCodeClasses? _cadCode = null;
    private bool _hasKey = false;

    public CADCodeManager(string jobName, IEnumerable<InventoryItem> availableInventory) {
        _jobName = jobName;
        _availableInventory = availableInventory;
    }

    public void AddPart(CNCPart part) {
        if (_partsByMaterial.ContainsKey(part.Material)) {
            _partsByMaterial[part.Material].Add(part);
        } else {
            _partsByMaterial.Add(part.Material, new() { part });
        }
    }

    /// <summary>
    /// Generates the nested gcode for a single machine
    /// </summary>
    /// <returns>The results of the nest optimization</returns>
    public IEnumerable<OptimizationResult> GenerateNestedCode(CADCodeConfiguration configuration, CNCMachineConfiguration machineConfig, CADCodeMachineConfiguration ccmachineConfig) {

        var optimizerSettings = configuration.OptimizerSettings;

        string date = DateTime.Today.ToShortDateString();

        int lastNumber = 0;
        var results = new List<OptimizationResult>();
        foreach (var (material, parts) in _partsByMaterial) {

            string optimzationBatchName = new Random().Next(0, 100000).ToString("D6");

            // TODO: there is probably a better way to reset for next optimization, seperate CADCode into another abstraction (CADCodeWrapper)
            _cadCode = InitilizeCADCode(configuration, ccmachineConfig, GenerationType.Nested, optimzationBatchName);
            _cadCode.Code.StartingProgramNumber = lastNumber++;
            //_cadCode.Code = CreateCodeClass(_cadCode.BootObject, _cadCode.Files, _cadCode.Tools, _jobName);
            //_cadCode.Optimizer = CreateOptimizer(_cadCode.BootObject, _cadCode.Files, optimizerSettings);

            // TODO: have to somehow check the orientation of inventory depending on the machine
            var inventory = _availableInventory.Where(item => item.Name == material.Name && Math.Abs(item.Thickness - material.Thickness) < 0.05)
                                                .SelectMany(item => item.AsCutListInventory(machineConfig.Orientation))
                                                .ToList();
            
            if (!inventory.Any()) {
                // TODO: warn that there is no available inventory for selected material
                continue;
            }

            inventory.ForEach(item => {
                _cadCode!.Optimizer.AddSheetStockByRef(item, _units);
            });

            parts.ForEach(p => _cadCode.Code.AddNestedPartMachining(p, machineConfig.ToolMap, machineConfig.Orientation));

            int line = 0;
            parts.ForEach(part => {
                _cadCode.Labels.NewLabel();
                _cadCode.Labels.AddField("WidthInches", (part.Width / 25.4).ToString("0.00"));
                _cadCode.Labels.AddField("LengthInches", (part.Length / 25.4).ToString("0.00"));
                _cadCode.Labels.AddField("Width", (part.Width).ToString("0"));
                _cadCode.Labels.AddField("Length", (part.Length).ToString("0"));
                _cadCode.Labels.AddField("Job Name", _jobName);
                _cadCode.Labels.AddField("Description", part.Description);
                _cadCode.Labels.AddField("Current Date", date);
                _cadCode.Labels.AddField("ProductName", part.Description);// TODO: add product name to part
                _cadCode.Labels.AddField("Filename", part.FileName);// TODO: add product name to part
                _cadCode.Labels.AddField("Quantity Required", part.Qty.ToString());
                _cadCode.Labels.AddField("Cabinet Number", (++line).ToString());// TODO: add line # and cabinet # to parts
                _cadCode.Labels.EndLabel();
            });

            // TODO: investigate if there is another way of figuring out what parts are on what sheet, because the current way creates labels that are not accurate ( i don't think this is necessary, i think CADCode does it the same way as implemented here)
            var optimizedParts = parts.SelectMany(part => part.GetAsCADCodeParts(_units, machineConfig.Orientation)).ToList();
            
            optimizedParts.ForEach(part => {
                _cadCode.Optimizer.AddPartByRef(ref part);
            });

            int showPattern = optimizerSettings.ShowPattern ? 1 : 0;
            int showResult = optimizerSettings.ShowResult ? 1 : 0;

            /*
             * If a label class is passed to the optimize function, then there must be at least 1 label in the labels class.
             */
            _cadCode.Optimizer.CutlistTitle = _jobName;
            _cadCode.Optimizer.Optimize(typeOptimizeMethod.CC_OPT_ANYKIND, _cadCode.Code, optimzationBatchName, showPattern, showResult, _cadCode.Labels);

            // TODO: combine results
            var result = GetResult(inventory, optimizedParts, _cadCode.Optimizer, _cadCode.Code);
            PrintResult(result);
            results.Add(result);

            lastNumber += _cadCode.Code.ProgramCount;

            PrintMissedEvents(_cadCode.BootObject);

        }


        return results;

    }

    public IReadOnlyList<string> GenerateSinglePrograms(CADCodeConfiguration configuration, CNCMachineConfiguration machineConfig, CADCodeMachineConfiguration ccmachineConfig) {

        // I don't think it is necessary to generate a new batch name (random numbers) for single programs of different material/machine
        _cadCode = InitilizeCADCode(configuration, ccmachineConfig, GenerationType.Single, "");

        var processedFiles = new List<string>();

        foreach (var parts in _partsByMaterial.Values) {
            foreach (var part in parts) {
                _cadCode.Code.AddSingleProgram(part, _units, machineConfig.ToolMap, machineConfig.Orientation);
            }
        }

        int ret = _cadCode.Code.DoOutput(_units, 0, 0);
        if (ret != 0) Console.WriteLine("Error while releasing single programs");

        var fileNames = _cadCode.Code.GetProcessedFileNames();
        if (fileNames is null) return processedFiles;
        foreach (var file in fileNames) {
            if (file is not string filename) continue;
            processedFiles.Add(filename.Split(',')[0]);
        }

        PrintMissedEvents(_cadCode.BootObject);

        return processedFiles;
    }

    public void Reset() {
        _cadCode = null;
    }

    private static void PrintMissedEvents(CADCodeBootObject boot) {

        var events = boot.GetEvents();
        foreach (var e in events) {
            Console.WriteLine($"[MissedEvent] {e as string}");
        }

    }

    private CADCodeClasses InitilizeCADCode(CADCodeConfiguration configuration, CADCodeMachineConfiguration ccmachineConfig, GenerationType generationType, string optimizationBatchName) {

        var bootObj = CreateCADCodeObj();

        // TODO: do something when no licence for CADCode is found. Make sure it does not block all other processes in the release 
        if (!_hasKey) throw new InvalidProgramException("Cannot access CADCode");

        var files = CreateFileClass(configuration, ccmachineConfig, generationType);
        var tools = CreateToolFile(bootObj, files, ccmachineConfig.ToolFilePath);
        var labels = GetLabelClass(bootObj, files, _jobName, optimizationBatchName);
        var code = CreateCodeClass(bootObj, files, tools, _jobName);
        var optimizer = CreateOptimizer(bootObj, files, configuration.OptimizerSettings);

        // TODO: ensure that the proper jobName subdirectories are created (1. Label file, 2. gcode output)

        return new(bootObj,files, tools, labels, code, optimizer);

    }

    private static void PrintResult(OptimizationResult nestResult) {

        //Console.WriteLine("************************************");
        //Console.WriteLine("Single Parts:");
        //foreach (var program in singleprograms) {
        //    Console.WriteLine($"    {program}");
        //}
        Console.WriteLine("************************************");
        Console.WriteLine("Nested Parts:");
        var unplaced = nestResult.UnplacedParts;
        var placed = nestResult.PlacedParts;
        var inventory = nestResult.UsedInventory;
        var programs = nestResult.Programs;
        Console.WriteLine(" Unplaced parts:");
        foreach (var part in unplaced) {
            Console.WriteLine($"        {part}");
        }

        Console.WriteLine(" Used Inventory:");
        for (int i = 0; i < inventory.Length; i++) {

            var item = inventory[i];
            Console.WriteLine($"        ({item.SheetsUsed}) {item.Name} - {item.Width}x{item.Length}x{item.Thickness} grained:{item.IsGrained}");

            IEnumerable<(int QtyOnSheet, string FileName)> partsOnMaterial = placed.Where(p => p.UsedInventoryIndex == i)
                                                                                    .GroupBy(p => p.FileName)
                                                                                    .Select(g => (g.Count(), g.Key));
            foreach (var (QtyOnSheet, FileName) in partsOnMaterial) {
                Console.WriteLine($"            ({QtyOnSheet}) {FileName}");
            }

        }

        Console.WriteLine(" Programs:");
        for (int i = 0; i < programs.Length; i++) {

            var prorgam = programs[i];
            Console.WriteLine($"        {prorgam}");

            IEnumerable<(int QtyOnSheet, string FileName)> partsInProgram = placed.Where(p => p.ProgramIndex == i)
                                                                                    .GroupBy(p => p.FileName)
                                                                                    .Select(g => (g.Count(), g.Key));
            foreach (var (QtyOnSheet, FileName) in partsInProgram) {
                Console.WriteLine($"            ({QtyOnSheet}) {FileName}");
            }

        }
        Console.WriteLine("************************************");

    }

    private static OptimizationResult GetResult(List<CutlistInventory> inventory, IEnumerable<Part> optimizedParts, CADCodePanelOptimizerClass optimizer, CADCodeCodeClass code) {
        var unplacedParts = optimizer.GetUnplacedPartNames()
                                    .GroupBy(p => p)
                                    .Select(g => {
                                        return new UnplacedPart() {
                                            FileName = g.Key,
                                            UnplacedQty = g.Count()
                                        };
                                    });

        var usedInventory = inventory.Select(i => {
            _ = int.TryParse(i.SheetsUsed, out int sheetsUsed);
            _ = double.TryParse(i.Width, out double width);
            _ = double.TryParse(i.Length, out double length);
            _ = double.TryParse(i.Thickness, out double thickness);
            return new UsedInventory() {
                Name = i.Description,
                Width = width,
                Length = length,
                Thickness = thickness,
                SheetsUsed = sheetsUsed,
                IsGrained = (i.Graining == "True") // TODO: check what this string is supposed to be
            };
        })
        .ToArray();

        var placedParts = optimizedParts.Select(p => {
            return new PlacedPart() {
                FileName = p.Face5Filename,
                UsedInventoryIndex = p.ParentInventoryItem - 1,
                Area = p.Area,
                ProgramIndex = p.PatternNumber - 1,
                IsRotated = p.Rotated,
                InsertionPoint = new() {
                    X = p.InsertionX,
                    Y = p.InsertionY
                }
            };
        }).ToList();

        var programNames = code.GetProgramFileNamesEnumerable().ToArray();

        var result = new OptimizationResult() {
            UsedInventory = usedInventory,
            PlacedParts = placedParts,
            UnplacedParts = unplacedParts,
            Programs = programNames
        };

        return result;
    }

    private static CADCodeLabelClass GetLabelClass(CADCodeBootObject bootObject, CADCodeFileClass files, string jobName, string optimizationBatchName) {
        var labels = bootObject.CreateLabels();
        labels.FileLocations = files;
        labels.JobName = $"{jobName}_{optimizationBatchName}";
        labels.SetupFileName = "Y:\\CADCode\\cfg\\Label Design Files\\DoorDrawerPatterns.mdb"; // Label design file

        labels.Progress += Labels_Progress;

        // TODO: need to make sure that the paths exist (do this when initilizing the cadcode object)
        // TODO: it seems like if the label file is not a valid path (the directory must exist) then the CADCode.Part.Rotated property is not set
        labels.LabelFileName = $"{jobName}.mdb";
        //Console.WriteLine(labels.IncludePictureInDatabase);
        //labels.IncludePictureInDatabase = true;
        //labels.NewLabel();
        //labels.EndLabel();
        //labels.PageFileName = "DoorDrawerPatterns.mdb";
        return labels;
    }

    private static void Labels_Progress(int L) {
        Console.WriteLine($"[LabelEvent] Progress {L}");
    }

    private CADCodeBootObject CreateCADCodeObj() {
        _hasKey = true;
        var bootObj = new CADCodeBootObject {
            //MessageMethod = MessageTypes.CC_USE_EVENTS,
            MessageMethod = MessageTypes.CC_USE_MESSAGEBOX,
            DebugMode = CC_DebugModes.CC_DEBUG_NONE
        };
        bootObj.CreateError += BootObj_CreateError;
        bootObj.Init();
        return bootObj;
    }

    private void BootObj_CreateError(int errorCode, string message) {
        Console.WriteLine($"[BootEvent] CreateError {errorCode} {message}");
        if (errorCode == 33012) _hasKey = false;
}

    private CADCodeCodeClass CreateCodeClass(CADCodeBootObject bootObject, CADCodeFileClass files, CADCodeToolFileClass tools, string jobName) {
        var code = bootObject.CreateCode();
        code.FileWritten += Code_FileWritten;
        code.MachiningError += Code_MachiningError;
        code.Progress += Code_Progress;

        code.FileStructures = files;
        code.ToolFile = tools;
        code.PictureScaleCircleMinimum = 0.0f;
        code.WriteWinStepFiles = true;
        code.GeneratePictures = true;
        code.BatchName = jobName;
        //code.SetPicturePath("C:\\Users\\Zachary Londono\\Desktop\\CC Output");
        code.PicturesFilledCircles = false;
        return code;
    }

    private void Code_Progress(int L) {
        //Console.WriteLine($"[CodeEvent] Progress - '{L}'");
    }

    private void Code_MachiningError(int L, string S) {
        Console.WriteLine($"[CodeEvent] Error - '{S}'");
    }

    private void Code_FileWritten(string Filename) {
        Console.WriteLine($"[CodeEvent] File written '{Filename}'");
    }

    private static CADCodeToolFileClass CreateToolFile(CADCodeBootObject bootObject, CADCodeFileClass files, string toolFilePath) {
        var tools = bootObject.CreateToolFile();
        tools.ToolFileError += Tools_ToolFileError;
        tools.FileLocations = files;
        tools.ReadToolFile(toolFilePath);
        return tools;
    }

    private static void Tools_ToolFileError(int L, string S) {
        Console.WriteLine($"[ToolEvent] Error - '{S}'");
    }

    private static CADCodePanelOptimizerClass CreateOptimizer(CADCodeBootObject bootObject, CADCodeFileClass files, OptimizerSettings  optimizerSettings) {
        var optimizer = bootObject.CreatePanelOptimizer();
        optimizer.FileWritten += Optimizer_FileWritten;
        optimizer.OptimizeError += Optimizer_OptimizeError;
        optimizer.Progress += Optimizer_Progress;

        optimizer.FileLocations = files;
        optimizer.Settings(optimizerSettings.Iterations, optimizerSettings.RunTime, optimizerSettings.Utilization, optimizerSettings.Kerf, optimizerSettings.Panels, optimizerSettings.Offset);
        return optimizer;
    }

    private static void Optimizer_Progress(int L) {
        //Console.WriteLine($"[OptimizeEvent] Progress - '{L}'");
    }

    private static void Optimizer_OptimizeError(int L, string S) {
        Console.WriteLine($"[OptimizeEvent] Error - '{S}'");
    }

    private static void Optimizer_FileWritten(string Filename) {
        Console.WriteLine($"[OptimizeEvent] File written '{Filename}'");
    }

    private static CADCodeFileClass CreateFileClass(CADCodeConfiguration ccConfig, CADCodeMachineConfiguration machineConfiguration, GenerationType generationType) => new() {
        ToolFilePath = machineConfiguration.ToolFilePath,
        FileOutputDirectory = generationType == GenerationType.Nested ? machineConfiguration.NestedGCodeOutputDirectory : machineConfiguration.SingleGCodeOutputDirectory,
        LabelFilePath = ccConfig.LabelDataOutputDirectory,
        LabelSetupPath = ccConfig.LabelDesignFile,
        PictureFileLocation = ccConfig.PictureFileOutputDirectory,
        StartingSubProgramNumber = ccConfig.StartingSubProgramNumber,
        StartingProgramNumber = ccConfig.StartingProgramNumber,
    };

    class CADCodeClasses {
        public CADCodeBootObject BootObject { get; set; }
        public CADCodeFileClass Files { get; set; }
        public CADCodeToolFileClass Tools { get; set; }
        public CADCodeLabelClass Labels { get; set; }
        public CADCodeCodeClass Code { get; set; }
        public CADCodePanelOptimizerClass Optimizer { get; set; }

        public CADCodeClasses(CADCodeBootObject bootObject, CADCodeFileClass files, CADCodeToolFileClass tools, CADCodeLabelClass labels, CADCodeCodeClass code, CADCodePanelOptimizerClass optimizer) {
            BootObject = bootObject;
            Files = files;
            Tools = tools;
            Labels = labels;
            Code = code;
            Optimizer = optimizer;
        }
    }

    enum GenerationType {
        Nested,
        Single
    }

}
