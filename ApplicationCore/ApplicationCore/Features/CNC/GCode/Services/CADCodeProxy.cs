using ApplicationCore.Features.CNC.GCode.Configuration;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Machining;
using ApplicationCore.Features.CNC.GCode.Contracts.Results;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Features.CNC.GCode.Domain.CADCode;
using ApplicationCore.Shared.Domain;
using CADCode;
using MoreLinq;

namespace ApplicationCore.Features.CNC.GCode.Services;

internal class CADCodeProxy : ICADCodeProxy {

	/*
	*  Defers the actual interaction to CADCode once it has already been set up and its data has been validated
	*/

	private Batch? _batch = null;
	private List<Label> _labels = new();
	private List<InventorySheetStock> _inventory = new();
	private List<Domain.Tool> _tools = new();
	private string _toolFilePath = string.Empty;

	private Action<CADCodeProgress>? _progressAction = null;
	private Action<CADCodeError>? _errorAction = null;

	private CADCodeBootObject? _cadCode = null;

	private readonly IToolFileReader _toolFileReader;
	private readonly CADCodeConfiguration _configuration;

	public CADCodeProxy(IToolFileReader toolFileReader, CADCodeConfiguration configuration) {
		_toolFileReader = toolFileReader;
		_configuration = configuration;
	}

	public void SetBatch(Batch batch) => _batch = batch;

	public void AddLabels(IEnumerable<Label> labels) => _labels.AddRange(labels);

	public void AddInventory(IEnumerable<InventorySheetStock> inventory) => _inventory.AddRange(inventory);

	public async Task AddToolsAsync(string toolFilePath) {
		var tools = await _toolFileReader.GetAvailableToolsAsync(toolFilePath);
		_tools.AddRange(tools);
		_toolFilePath = toolFilePath;
	}

	public OptimizationResult OptimizeNestedParts(bool generateLabels, string outputDirectory) {

		if (_batch is null) throw new InvalidOperationException("No batch set");

		if (_cadCode is null) InitilizeCADCode();

		var codeClass = CreateCode(outputDirectory);
		if (codeClass is null) throw new CADCodeFailedToInitilizeException();
		codeClass.BatchName = _batch.Name;

		var optimizer = CreateOptimizer(outputDirectory);
		if (optimizer is null) throw new CADCodeFailedToInitilizeException();

		var inventory = _inventory.SelectMany(AsCCInventory);
		foreach (var item in inventory) {
			optimizer.AddSheetStockByRef(item, UnitTypes.CC_U_METRIC);
		}

		codeClass.Border(1, 1, 1f, UnitTypes.CC_U_METRIC, OriginType.CC_LL, _batch.Name, AxisTypes.CC_AUTO_AXIS);

		List<CADCode.Part> ccParts = new();
		_batch.Parts.ForEach(part => {

			AddNestedPart(codeClass, part);

			ccParts.AddRange(AsCCParts(part, _batch.Material.Thickness, _batch.Material.SheetStock));

		});

		optimizer.Optimize(typeOptimizeMethod.CC_OPT_ANYKIND, codeClass, _batch.Name, 0, 0);

		return new OptimizationResult() {
			PlacedParts = Enumerable.Empty<PlacedPart>(),
			Programs = Array.Empty<string>(),
			UnplacedParts = Enumerable.Empty<UnplacedPart>(),
			UsedInventory = Array.Empty<UsedInventory>()
		};

	}

	public SinglePartGenerationResult SiglePartPrograms(string outputDirectory) {

		if (_batch is null) throw new InvalidOperationException("No batch set");

		if (_cadCode is null) InitilizeCADCode();

		List<string> generatedFiles = new();

		var codeClass = CreateCode(outputDirectory);
		if (codeClass is null) throw new CADCodeFailedToInitilizeException();

		codeClass.BatchName = _batch.Name;

		_batch.Parts.ForEach(part => AddSingleProgram(codeClass, part, _batch.Material.Thickness));

		var outputResult = codeClass.DoOutput(UnitTypes.CC_U_METRIC, 0, 0);
		if (outputResult != 0) throw new InvalidOperationException($"CAD Code returned non-zero result while generating output '{outputResult}'");

		var files = codeClass.GetProcessedFileNames();
		if (files is not null) {
			foreach (var file in files) {
				if (file is not string filename) continue;
				generatedFiles.Add(filename.Split(',')[0]);
			}
		}

		return new SinglePartGenerationResult() {
			GeneratedFiles = generatedFiles
		};

	}

	private static void AddNestedPart(CADCodeCodeClass code, Contracts.Part part) {

		AddNestedFace(code, part.Length, part.Width, part.PrimaryFace);

		if (part.SecondaryFace is not null) {
			AddNestedFace(code, part.Length, part.Width, part.SecondaryFace);
		}

	}

	private static void AddNestedFace(CADCodeCodeClass code, Dimension length, Dimension width, PartFace face) {
		code.NestedPart((float)length.AsMillimeters(), (float)width.AsMillimeters(), OriginType.CC_LL, face.FileName, AxisTypes.CC_AUTO_AXIS, 0);
		foreach (var operation in face.Operations) {
			AddMachining(code, operation);
		}
	}

	private static void AddSingleProgram(CADCodeCodeClass code, Contracts.Part part, Dimension thickness) {

		AddSingleFace(code, part.Length, part.Width, thickness, part.PrimaryFace);
		code.EndPanel();

		if (part.SecondaryFace is not null) {
			AddSingleFace(code, part.Length, part.Width, thickness, part.SecondaryFace);
			code.EndPanel();
		}

	}

	private static void AddSingleFace(CADCodeCodeClass code, Dimension length, Dimension width, Dimension thickness, PartFace face) {
		code.Border((float)length.AsMillimeters(), (float)width.AsMillimeters(), (float)thickness.AsMillimeters(), UnitTypes.CC_U_METRIC, OriginType.CC_LL, face.FileName, AxisTypes.CC_X_AXIS);
		foreach (var operation in face.Operations) {
			AddMachining(code, operation);
		}
	}

	public void Reset() {
		_labels.Clear();
		_inventory.Clear();
		_tools.Clear();
		_batch = null;
		_cadCode = null;
	}

	public void OnProgress(Action<CADCodeProgress> onprogress) => _progressAction = onprogress;

	public void OnError(Action<CADCodeError> onerror) => _errorAction = onerror;

	private void InitilizeCADCode() {

		_cadCode = new CADCodeBootObject();

		_cadCode.CreateError += (L, S) => {
			if (_errorAction is not null) _errorAction(new CADCodeError(S));
		};

		var initResult = _cadCode.Init();
		if (initResult != 0) throw new CADCodeFailedToInitilizeException();

	}

	private CADCodeCodeClass CreateCode(string outputDirectory) {

		if (_cadCode is null) throw new InvalidOperationException("CADCode not initilized");

		var code = _cadCode.CreateCode();

		code.ToolFile = CreateToolFile();
		code.FileStructures = CreateFileStructure(outputDirectory);
		code.PictureScaleCircleMinimum = 0.0f;
		code.GeneratePictures = true;
		code.PicturesFilledCircles = false;

		code.MachiningError += (L, S) => {
			if (_errorAction is not null) _errorAction(new CADCodeError(S));
		};
		code.Progress += (P) => {
			if (_progressAction is not null) _progressAction(new CADCodeProgress("Code generation progress", P));
		};

		return code;

	}

	private CADCodeToolFileClass CreateToolFile() {

		if (_cadCode is null) throw new InvalidOperationException("CADCode not initilized");
		if (_toolFilePath == string.Empty) throw new InvalidOperationException("Tool file not set");

		var toolFile = _cadCode.CreateToolFile();

		toolFile.ReadToolFile(_toolFilePath);

		toolFile.ToolFileError += (L, S) => {
			if (_errorAction is not null) _errorAction(new CADCodeError(S));
		};

		return toolFile;

	}

	private CADCodeFileClass CreateFileStructure(string outputDirectory) => new() {
		ToolFilePath = _toolFilePath,
		FileOutputDirectory = outputDirectory,
		LabelFilePath = _configuration.LabelDataOutputDirectory,
		LabelSetupPath = _configuration.LabelDesignFile,
		PictureFileLocation = _configuration.PictureFileOutputDirectory
	};

	private CADCodePanelOptimizerClass CreateOptimizer(string outputDirectory) {

		if (_cadCode is null) throw new InvalidOperationException("CADCode not initilized");
		
		var optimizer = _cadCode.CreatePanelOptimizer();

		optimizer.OutputUnits = UnitTypes.CC_U_METRIC;
		optimizer.FileLocations = CreateFileStructure(outputDirectory);

		optimizer.OptimizeError += (L, S) => {
			if (_errorAction is not null) _errorAction(new CADCodeError(S));
		};

		optimizer.Progress += (P) => {
			if (_progressAction is not null) _progressAction(new CADCodeProgress("Nested code generation progress", P));
		};

		return optimizer;

	}

	private static void AddMachining(CADCodeCodeClass code, MachiningOperation operation) {

		// TODO: need to get the tool specs

		if (operation is Bore bore) {

			code.Bore((float)bore.Position.X,
					  (float)bore.Position.Y,
					  (float)bore.Depth,
							 FaceTypes.CC_UPPER_FACE,
					  (float)0,
							 bore.ToolName,
							 "",
							 "",
					  (float)0,
					  (float)0,
							 "",
							 bore.Sequence,
							 bore.PassCount);

		} else if (operation is MultiBore multiBore) {

			code.MultiBore((float)multiBore.StartPosition.X,
							(float)multiBore.StartPosition.Y,
							(float)multiBore.Depth,
							(float)multiBore.EndPosition.X,
							(float)multiBore.EndPosition.Y,
								   FaceTypes.CC_UPPER_FACE,
							(float)0,
								   multiBore.ToolName,
							(float)multiBore.Pitch,
							(float)0,
							(float)0,
								   "",
								   multiBore.NumberOfHoles,
								   multiBore.Sequence,
								   multiBore.PassCount);

		} else if (operation is Pocket pocket) {

			code.Pocket((float)pocket.PositionA.X,
						(float)pocket.PositionA.Y,
						(float)pocket.PositionB.X,
						(float)pocket.PositionB.Y,
						(float)pocket.PositionC.X,
						(float)pocket.PositionC.Y,
						(float)pocket.PositionD.X,
						(float)pocket.PositionD.Y,
						(float)pocket.StartDepth,
						(float)pocket.EndDepth,
							   pocket.ToolName,
							   FaceTypes.CC_UPPER_FACE,
						(float)0,
						(float)0,
						(float)0,
						(float)0,
							   "",
							   pocket.Sequence,
							   pocket.Climb,
							   pocket.PassCount);

		} else if (operation is PocketArc parc) {

			code.DefinePocket((float)parc.StartPosition.X,
								(float)parc.StartPosition.Y,
								(float)parc.StartDepth,
								(float)parc.EndPosition.X,
								(float)parc.EndPosition.Y,
								(float)parc.EndDepth,
									   0,
									   0,
									   0,
								(float)parc.Radius,
									   AsCCArcType(parc.Direction),
									   AsCCOffsetType(parc.Offset.Type),
								(float)parc.Offset.Amount,
									   RotationTypes.CC_ROTATION_AUTO,
									   0,
									   parc.ToolName,
								(float)0,
								(float)0,
								(float)0,
									   0,
									   parc.Sequence,
									   Array.Empty<byte>(),
									   false,
									   parc.PassCount);


		} else if (operation is PocketSegment psegment) {

			code.DefinePocket((float)psegment.StartPosition.X,
								(float)psegment.StartPosition.Y,
								(float)psegment.StartDepth,
								(float)psegment.EndPosition.X,
								(float)psegment.EndPosition.Y,
								(float)psegment.EndDepth,
									   0,
									   0,
									   0,
									   0,
									   ArcTypes.CC_UNKNOWN_ARC,
									   AsCCOffsetType(psegment.Offset.Type),
								(float)psegment.Offset.Amount,
									   RotationTypes.CC_ROTATION_AUTO,
									   0,
									   psegment.ToolName,
								(float)0,
								(float)0,
								(float)0,
									   0,
									   psegment.Sequence,
									   Array.Empty<byte>(),
									   false,
									   psegment.PassCount);


		} else if (operation is RouteArc arc) {

			code.RouteArc((float)arc.StartPosition.X,
							(float)arc.StartPosition.Y,
							(float)arc.StartDepth,
							(float)arc.EndPosition.X,
							(float)arc.EndPosition.Y,
							(float)arc.EndDepth,
								   arc.ToolName,
							(float)0,
								   AsCCOffsetType(arc.Offset.Type),
							(float)arc.Offset.Amount,
								   RotationTypes.CC_ROTATION_AUTO,
								   FaceTypes.CC_UPPER_FACE,
							(float)0,
							(float)0,
							(float)0,
							(float)0,
								   "",
								   0,
								   0,
								   0,
								   0,
							(float)arc.Radius,
								   AsCCArcType(arc.Direction),
								   0,
								   arc.Sequence,
								   arc.PassCount);

		} else if (operation is RouteLine route) {

			code.RouteLine((float)route.StartPosition.X,
							(float)route.StartPosition.Y,
							(float)route.StartDepth,
							(float)route.EndPosition.X,
							(float)route.EndPosition.Y,
							(float)route.EndDepth,
								   route.ToolName,
							(float)0,
								   AsCCOffsetType(route.Offset.Type),
							(float)route.Offset.Amount,
								   RotationTypes.CC_ROTATION_AUTO,
								   FaceTypes.CC_UPPER_FACE,
							(float)0,
							(float)0,
							(float)0,
							(float)0,
								   "",
								   route.Sequence,
								   route.PassCount);

		} else if (operation is OutlineArc oarc) {

			code.DefineOutLine((float)oarc.Start.X,
								(float)oarc.Start.Y,
								(float)oarc.End.X,
								(float)oarc.End.Y,
									   0,
									   0,
								(float)oarc.Radius,
									   AsCCArcType(oarc.Direction),
									   AsCCOffsetType(oarc.Offset.Type),
									   oarc.ToolName,
								(float)0,
								(float)0,
									   oarc.Sequence,
									   oarc.PassCount,
									   0);

		} else if (operation is OutlineSegment osegment) {

			code.DefineOutLine((float)osegment.Start.X,
								(float)osegment.Start.Y,
								(float)osegment.End.X,
								(float)osegment.End.Y,
									   0,
									   0,
									   0,
									   ArcTypes.CC_UNKNOWN_ARC,
									   AsCCOffsetType(osegment.Offset.Type),
									   osegment.ToolName,
								(float)0,
								(float)0,
									   osegment.Sequence,
									   osegment.PassCount,
									   0);

		} else if (operation is Rectangle rectangle) {

			var components = rectangle.GetComponents();
			foreach (var component in components) {
				AddMachining(code, component);
			}

		}

	}

	public static IEnumerable<CADCode.Part> AsCCParts(Contracts.Part part, Dimension thickness, string material) {

		List<CADCode.Part> ccParts = new();

		for (int i = 0; i < part.Qty; i++) {

			ccParts.Add(new() {
				Face5Filename = part.PrimaryFace.FileName,
				Face6FileName = part.SecondaryFace?.FileName ?? "",
				Length = part.Length.AsMillimeters().ToString(),
				Width = part.Width.AsMillimeters().ToString(),
				Thickness = (float)thickness.AsMillimeters(),
				Material = material,
				Units = UnitTypes.CC_U_METRIC,
				RotationAllowed = 1,
				ContainsShape = part.ContainsShape,
				RouteShape = part.ContainsShape,
				PerimeterRoute = true
			});

		}

		return ccParts;

	}

	public static IEnumerable<CutlistInventory> AsCCInventory(InventorySheetStock sheetStock) {

		List<CutlistInventory> items = new();

		foreach (var size in sheetStock.Sizes) {

			items.Add(new() {
				Description = sheetStock.Name,
				Length = "",
				Width = "",
				Thickness = sheetStock.Thickness.AsMillimeters().ToString(),
				Priority = "",
				Graining = "",

				Supply = "999",
				Trim1 = "7",
				Trim2 = "7",
				Trim3 = "4",
				Trim4 = "4",
				TrimDrop = false
			});

		}

		return items;

	}

	public static OffsetTypes AsCCOffsetType(OffsetType offset) => offset switch {
		OffsetType.None => OffsetTypes.CC_OFFSET_NONE,
		OffsetType.Left => OffsetTypes.CC_OFFSET_LEFT,
		OffsetType.Right => OffsetTypes.CC_OFFSET_RIGHT,
		OffsetType.Inside => OffsetTypes.CC_OFFSET_INSIDE,
		OffsetType.Outside => OffsetTypes.CC_OFFSET_OUTSIDE,
		OffsetType.Center => OffsetTypes.CC_OFFSET_CENTERLINE,
		_ => OffsetTypes.CC_OFFSET_NONE,
	};

	public static ArcTypes AsCCArcType(ArcDirection arc) => arc switch {
		ArcDirection.Unknown => ArcTypes.CC_UNKNOWN_ARC,
		ArcDirection.Clockwise => ArcTypes.CC_CLOCKWISE_ARC,
		ArcDirection.CounterClockwise => ArcTypes.CC_COUNTER_CLOCKWISE_ARC,
		_ => ArcTypes.CC_UNKNOWN_ARC
	};

}