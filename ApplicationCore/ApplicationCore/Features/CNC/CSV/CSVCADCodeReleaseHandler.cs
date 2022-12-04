using ApplicationCore.Features.CNC.Contracts.Machining;
using ApplicationCore.Features.CNC.GCode.Contracts;
using ApplicationCore.Features.CNC.GCode.Contracts.Machining;
using ApplicationCore.Features.CNC.GCode.Domain;
using ApplicationCore.Infrastructure;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace ApplicationCore.Features.CNC.CSV;

internal class CSVCADCodeReleaseHandler : CommandHandler<ReleaseCADCodeCSVBatchCommand>
{

    private readonly ILogger<CSVCADCodeReleaseHandler> _logger;
    private readonly ICSVParser _parser;
    private readonly IBus _bus;

    public CSVCADCodeReleaseHandler(ILogger<CSVCADCodeReleaseHandler> logger, ICSVParser parser, IBus bus)
    {
        _logger = logger;
        _parser = parser;
        _bus = bus;
    }

    public override async Task<Response> Handle(ReleaseCADCodeCSVBatchCommand command)
    {

        var parseResult = await _parser.ParsePartsAsync(command.FilePath);

        var partsByJob = parseResult.Parts.GroupBy(p => p.Border.JobName);

        foreach (var job in partsByJob)
        {

            try
            {

                var parts = job.Select(CSVPartToCNCPart).ToList();

                CNCBatch batch = new()
                {
                    Name = job.Key,
                    Parts = parts
                };

                var response = await _bus.Send(new CNCReleaseRequest(batch));
                response.Match(
                    async job =>
                    {
                        var pdfResponse = await _bus.Send(new GenerateCNCReleasePDFRequest(job, command.CNCReportOutputDirectory));
                        pdfResponse.OnError(error => _logger.LogError(error.Title));
                    },
                    error =>
                    {
                        _logger.LogError(error.Title);
                    }
                );

            }
            catch (Exception ex)
            {

                _logger.LogError("Error creating batch {EX}", ex);

            }

        }

        return new();

    }

    private CNCPart CSVPartToCNCPart(CSVPart part)
    {

        List<Token> tokens = new();

        bool hasShape = part.Tokens.Any(p => p.MachiningToken.ToLower().Equals("shape"));

        if (hasShape)
        {

            int lastShape = 0;
            int idx = 0;
            foreach (var token in part.Tokens)
            {
                if (token.MachiningToken.ToLower().Equals("shape")) lastShape = idx;
                idx++;
            }

            var shapes = part.Tokens.Take(lastShape + 1).ToList();

            var shape = new Shape();
            foreach (var segment in shapes)
            {

                switch (segment.MachiningToken.ToLower())
                {

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
            foreach (var segment in segments)
            {

                if (segment is ArcSegment arc)
                {

                    tokens.Add(new OutlineArc()
                    {
                        Start = arc.Start,
                        End = arc.End,
                        Direction = arc.Direction,
                        Radius = arc.Radius,
                        RType = "",

                        // TODO: get these values from csv token
                        Sequence = 0,
                        Offset = new RouteOffset(OffsetType.Inside, 0),
                        PassCount = 1,
                        Tool = new Tool("pocket3", 9)
                    });

                }
                else if (segment is LineSegment line)
                {

                    tokens.Add(new OutlineSegment()
                    {
                        Start = line.Start,
                        End = line.End,
                        RType = "",

                        // TODO: get these values from csv token
                        Sequence = 0,
                        Offset = new RouteOffset(OffsetType.Inside, 0),
                        PassCount = 1,
                        Tool = new Tool("pocket3", 9)
                    });

                }

            }


            part.Tokens = part.Tokens.Skip(lastShape + 1).ToList();

        }

        part.Tokens
            .Where(t => !string.IsNullOrWhiteSpace(t.MachiningToken))
            .ForEach(t => tokens.Add(MapCSVToken(t)));

        // TODO: add a HasShape property to parts
        return new CNCPart()
        {
            FileName = part.Border.Filename,
            Qty = part.Border.Quantity,
            Description = "",
            Length = part.Border.StartX,
            Width = part.Border.StartY,
            Tokens = tokens,
            ContainsShape = hasShape,
            Material = new()
            {
                Name = part.Border.Material,
                Thickness = part.Border.StartZ
            }
        };

    }

    private Token MapCSVToken(CSVToken token)
    {

        string tokenName = token.MachiningToken.ToLower();
        string comment = string.Empty;

        var commentStart = tokenName.IndexOf('*');
        if (commentStart != -1)
        {
            comment = tokenName[(commentStart + 1)..];
            tokenName = tokenName[0..commentStart];
        }

        // TODO: set token comment property
        return tokenName switch
        {
            "route" => new RouteLine()
            {
                StartPosition = new((float)token.StartX, (float)token.StartY),
                EndPosition = new((float)token.EndX, (float)token.EndY),
                StartDepth = (float)token.StartZ,
                EndDepth = (float)token.EndZ,
                Offset = new(token.OffsetSide, 0),
                PassCount = token.Passes,
                RType = "",
                Sequence = token.SequenceNumber,
                Tool = new(token.ToolNumber, (float)token.ToolDiameter)
            },

            "cwarc" => new RouteArc()
            {

            },

            "ccwarc" => new RouteArc()
            {

            },

            "rectangle" => new Rectangle()
            {
                PositionA = new((float)token.StartX, (float)token.StartY),
                PositionB = new((float)token.PocketX, (float)token.PocketY),
                PositionC = new((float)token.EndX, (float)token.EndY),
                PositionD = new((float)token.CenterX, (float)token.CenterY),
                StartDepth = (float)token.StartZ,
                EndDepth = (float)token.EndZ,
                Radius = (float)token.Radius,
                Offset = new(token.OffsetSide, 0),
                PassCount = token.Passes,
                RType = "",
                Sequence = token.SequenceNumber,
                Tool = new(token.ToolNumber, (float)token.ToolDiameter)
            },

            "pocket" => new Pocket()
            {
                PositionA = new((float)token.StartX, (float)token.StartY),
                PositionB = new((float)token.PocketX, (float)token.PocketY),
                PositionC = new((float)token.EndX, (float)token.EndY),
                PositionD = new((float)token.CenterX, (float)token.CenterY),
                StartDepth = (float)token.StartZ,
                EndDepth = (float)token.EndZ,
                PassCount = token.Passes,
                RType = "",
                Sequence = token.SequenceNumber,
                Tool = new(token.ToolNumber, (float)token.ToolDiameter),
                Climb = 0
            },

            "bore" => new Bore()
            {
                Position = new((float)token.StartX, (float)token.StartY),
                Depth = (float)token.StartZ,
                PassCount = token.Passes,
                RType = "",
                Sequence = token.SequenceNumber,
                Tool = new(token.ToolNumber, (float)token.ToolDiameter)
            },

            "multibore" => new MultiBore()
            {
                StartPosition = new((float)token.StartX, (float)token.StartY),
                EndPosition = new((float)token.EndX, (float)token.EndY),
                Depth = (float)token.StartZ,
                NumberOfHoles = 0,
                PassCount = token.Passes,
                //Pitch = token.Pitch, TODO: get pitch in token
                RType = "",
                Sequence = token.SequenceNumber,
                Tool = new(token.ToolNumber, (float)token.ToolDiameter)
            },

            "freepocket" => new PocketSegment()
            {
                StartPosition = new((float)token.StartX, (float)token.StartY),
                EndPosition = new((float)token.EndX, (float)token.EndY),
                StartDepth = (float)token.StartZ,
                EndDepth = (float)token.EndZ,
                Offset = new(token.OffsetSide, 0),
                PassCount = token.Passes,
                RType = "",
                Sequence = token.SequenceNumber,
                Tool = new(token.ToolNumber, (float)token.ToolDiameter)
            },

            _ => throw new NotImplementedException($"Unexpected token name {tokenName}"),
        };
    }

}