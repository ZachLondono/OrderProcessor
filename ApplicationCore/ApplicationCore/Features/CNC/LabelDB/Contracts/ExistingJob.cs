namespace ApplicationCore.Features.CNC.LabelDB.Contracts;

public record ExistingJob(string Name, string MachineName, IEnumerable<UsedInventory> Inventory, IEnumerable<Pattern> Patterns, IEnumerable<ManufacturedPart> Parts);
public record UsedInventory(int Qty, string Name, string Width, string Length, string Thickness, string Grained);
public record Pattern(string Name, string ImagePath, string MaterialName, float MaterialWidth, float MaterialLength, float MaterialThickness);
public record ManufacturedPart(string Name, string Description, int PatternNumber, float Width, float Length, float InsertX, float InsertY);