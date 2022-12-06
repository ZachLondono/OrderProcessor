using ApplicationCore.Features.CNC.CSV.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Machining;
using ApplicationCore.Features.CNC.Shared;
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

	private CNCPart MapCSVPartToCNCPart(CSVPart csvpart) {

		bool containsShape = csvpart.Tokens.Any(t => t.MachiningToken.ToLower() == "shape");

		List<Token> tokens = new();

		if (containsShape) {

			int idx = csvpart.Tokens.FindLastIndex(t => t.MachiningToken.ToLower() == "shape");

			var outline = csvpart.Tokens.Take(idx + 1);

			tokens.AddRange(OutlineTokensFromCSV(outline));

			tokens.AddRange(
					csvpart.Tokens
							.Skip(idx + 1)
							.Select(MapCSVTokenToCNCToken)
							.Where(t => t != null)
							.Cast<Token>()
				);

		} else {

			tokens = csvpart.Tokens
						.Select(MapCSVTokenToCNCToken)
						.Where(t => t != null)
						.Cast<Token>()
						.ToList();

		}

		return new() {
			Description = csvpart.Border.Description,
			FileName = csvpart.Border.FileName,
			Length = csvpart.Border.Length,
			Width = csvpart.Border.Width,
			Qty = csvpart.Border.Qty,
			ContainsShape = containsShape,
			Tokens = tokens,
			Material = new() {
				Name = csvpart.Border.Material,
				Thickness = csvpart.Border.Thickness
			}
		};
	}

	private IEnumerable<Token> OutlineTokensFromCSV(IEnumerable<CSVToken> tokens) {

		// TODO: move shape class to CNC.Shared namespace
		Shape shape = new();

		foreach (var segment in tokens) {

			switch (segment.MachiningToken.ToLower()) {

				case "shape":
					var start = new Point(
							segment.StartX,
							segment.StartY);

					var end = new Point(
							segment.EndX,
							segment.EndY);

					shape.AddLine(start, end);
					break;

				case "fillet":
					shape.AddFillet(segment.Radius);
					break;

			}

		}

		var segments = shape.GetSegments();

		List<Token> outline = new();
		foreach (var segment in segments) {

			if (segment is ArcSegment arc) {

				outline.Add(new OutlineArc() {
					Start = arc.Start,
					End = arc.End,
					Center = arc.Center,
					Direction = arc.Direction,
					Radius = arc.Radius,
					RType = "",

					// TODO: get these values from csv token
					Sequence = 0,
					Offset = new RouteOffset(OffsetType.Outside, 0),
					PassCount = 1,
					Tool = new Tool("", 0)
				});

			} else if (segment is LineSegment line) {

				outline.Add(new OutlineSegment() {
					Start = line.Start,
					End = line.End,
					RType = "",

					// TODO: get these values from csv token
					Sequence = 0,
					Offset = new RouteOffset(OffsetType.Outside, 0),
					PassCount = 1,
					Tool = new Tool("", 0)
				});

			}

		}

		return outline;

	}

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
			"bore" => CSVTokenToBore(token),
			"multibore" => CSVTokenToMuliBore(token),
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

	private static Token CSVTokenToPocket(CSVToken token) {

		if (token.StartX == 0 && token.StartY == 0 && token.EndX == 0 && token.EndY == 0 && token.PocketX == 0 && token.PocketY == 0 && token.Radius != 0) {

			return new PocketCircle() {
				CenterPosition = new(token.CenterX, token.CenterY),
				Depth = token.StartZ,
				PassCount = token.Passes,
				Radius = token.Radius,
				RType = "",
				Sequence = token.SequenceNumber,
				Tool = new(token.ToolNumber, token.ToolDiameter)
			};

		}

		return new Pocket() {
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

	}

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

	private static Bore CSVTokenToBore(CSVToken token) => new() {
		Position = new(token.StartX, token.StartY),
		Depth = token.StartZ,
		PassCount = token.Passes,
		Sequence = token.SequenceNumber,
		Tool = new(token.ToolNumber, token.ToolDiameter),
		RType = ""
	};

	private static MultiBore CSVTokenToMuliBore(CSVToken token) => new() {
		StartPosition = new(token.StartX, token.StartY),
		EndPosition = new(token.EndX, token.EndY),
		NumberOfHoles = 0, // token.Holes
		Pitch = token.Pitch,
		Depth = token.StartZ,
		PassCount = token.Passes,
		Sequence = token.SequenceNumber,
		Tool = new(token.ToolNumber, token.ToolDiameter),
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
