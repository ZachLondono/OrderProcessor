using ApplicationCore.Features.CNC.CSV.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Machining;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.CNC;

public class CSVTokensParser {

	private readonly ILogger<CSVTokensParser> _logger;

	public CSVTokensParser(ILogger<CSVTokensParser> logger) {
		_logger = logger;
	}

	public IEnumerable<CNCBatch> ParseTokens(CSVReadResult csv) {

		Stack<CSVPart> parts = new();

		foreach (var token in csv.Tokens) {

			if (token.MachiningToken.ToLower() == "border") {

				parts.Push(new CSVPart() {
					BatchName = token.JobName,
					Border = new Border() {
						FileName = token.Filename,
						Qty = token.Quantity,
						Width = token.StartY,
						Length = token.StartX,
						Thickness = token.StartZ,
						Material = token.Material
					}
				});

			} else {

				parts.Peek()
					.Tokens
					.Add(token);

			}

		}

		_logger.LogInformation($"Found {parts.Count} parts in {csv.Tokens.Count()} tokens");

		return parts.GroupBy(p => p.BatchName)
					.Select(group => new CNCBatch() {
						Name = group.Key,
						Parts = group.Select(MapCSVPartToCNCPart).ToList()
					});

	}

	private CNCPart MapCSVPartToCNCPart(CSVPart csvpart) => new() {
		
		// TODO: parse shape and fillet tokens

		Description = csvpart.Border.Description,
		FileName = csvpart.Border.FileName,
		Length = csvpart.Border.Length,
		Width = csvpart.Border.Width,
		Qty = csvpart.Border.Qty,
		ContainsShape = csvpart.Tokens.Any(t => t.MachiningToken.ToLower() == "shape"),
		Tokens = csvpart.Tokens
						.Select(MapCSVTokenToCNCToken)
						.Where(t => t != null)
						.Cast<Token>()
						.ToList(),
		Material = new() {
			Name = csvpart.Border.Material,
			Thickness = csvpart.Border.Thickness
		}
	};

	private Token? MapCSVTokenToCNCToken(CSVToken token)
		=> token.MachiningToken.ToLower() switch {
			"shape" => null, // throw new NotImplementedException(),
			"fillet" => null, // throw new NotImplementedException(),
			"rectangle" => CSVTokenToRectangle(token),
			"route" => CSVTokenToRouteLine(token),
			"ccwarc" => CSVTokenToRouteArc(token, false),
			"cwarc" => CSVTokenToRouteArc(token, true),
			"pocket" => CSVTokenToPocket(token),
			"freepocket" => CSVTokenToPocketSegment(token),
			_ => UnrecognizedTokenFound(token)
		};

	private static Rectangle CSVTokenToRectangle(CSVToken token) => new() {
		PositionA = new(token.StartX, token.StartY),
		PositionB = new(token.PocketX, token.PocketY),
		PositionC = new(token.EndX, token.EndY),
		PositionD = new(token.CenterX, token.CenterY),
		StartDepth = token.StartZ,
		EndDepth = token.EndZ,
		PassCount = token.Passes,
		Sequence = token.SequenceNumber,
		Tool = new(token.ToolNumber, token.ToolDiameter),
		Radius = token.Radius,
		Offset = ParseRouteOffset(token),
		RType = ""
	};

	private static RouteLine CSVTokenToRouteLine(CSVToken token) => new() {
		StartPosition = new(token.StartX, token.StartY),
		StartDepth = token.StartZ,
		EndPosition = new(token.EndX, token.EndY),
		EndDepth = token.EndZ,
		PassCount = token.Passes,
		Sequence = token.SequenceNumber,
		Tool = new(token.ToolNumber, token.ToolDiameter),
		Offset = ParseRouteOffset(token),
		RType = ""
	};


	private static RouteArc CSVTokenToRouteArc(CSVToken token, bool clockwise) => new() {
		StartPosition = new(token.StartX, token.StartY),
		StartDepth = token.StartZ,
		EndPosition = new(token.EndX, token.EndY),
		EndDepth = token.EndZ,
		Radius = token.Radius,
		Direction = clockwise ? ArcDirection.Clockwise : ArcDirection.CounterClockwise,
		PassCount = token.Passes,
		Sequence = token.SequenceNumber,
		Tool = new(token.ToolNumber, token.ToolDiameter),
		Offset = ParseRouteOffset(token),
		RType = ""
	};

	private static Pocket CSVTokenToPocket(CSVToken token) => new() {
		PositionA = new(token.StartX, token.StartY),
		PositionB = new(token.PocketX, token.PocketY),
		PositionC = new(token.EndX, token.EndY),
		PositionD = new(token.CenterX, token.CenterY),
		StartDepth = token.StartZ,
		EndDepth = token.EndZ,
		PassCount = token.Passes,
		Sequence = token.SequenceNumber,
		Tool = new(token.ToolNumber, token.ToolDiameter),
		Climb = 0,
		RType = ""
	};

	private static PocketSegment CSVTokenToPocketSegment(CSVToken token) => new() {
		StartPosition = new(token.StartX, token.StartY),
		StartDepth = token.StartZ,
		EndPosition = new(token.EndX, token.EndY),
		EndDepth = token.EndZ,
		PassCount = token.Passes,
		Sequence = token.SequenceNumber,
		Tool = new(token.ToolNumber, token.ToolDiameter),
		Offset = ParseRouteOffset(token),
		RType = ""
	};

	private Token? UnrecognizedTokenFound(CSVToken token) {

		_logger.LogWarning("Unrecognized token found '{Token}'", token);

		return null;

	}

	private static RouteOffset ParseRouteOffset(CSVToken token) {
		return token.OffsetSide.ToLower() switch {
			"i" => new(OffsetType.Inside, 0),
			"o" => new(OffsetType.Outside, 0),
			"l" => new(OffsetType.Left, 0),
			"r" => new(OffsetType.Right, 0),
			"c" => new(OffsetType.Center, 0),
			_ => new(OffsetType.None, 0)
		};
	}

	private class CSVPart {

		public string BatchName { get; set; } = string.Empty;

		public Border Border { get; set; } = new();

		public List<CSVToken> Tokens { get; set; } = new List<CSVToken>();

	}

	private class Border {

		public string FileName { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public int Qty { get; set; }

		public double Width { get; set; }

		public double Length { get; set; }

		public double Thickness { get; set; }

		public string Material { get; set; } = string.Empty;

	}

}
