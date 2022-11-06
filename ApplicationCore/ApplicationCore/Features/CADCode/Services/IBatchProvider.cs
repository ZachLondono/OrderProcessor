using ApplicationCore.Features.CADCode.Contracts;
using ApplicationCore.Features.CADCode.Contracts.Machining;

namespace ApplicationCore.Features.CADCode.Services;

internal interface IBatchProvider {

    public CNCBatch GetBatch();

}

// Mock of Script Part Provider
internal class MockBatchProvider : IBatchProvider {

    public CNCBatch GetBatch() {

        var tool = new Tool("Pocket9", 9);

        var bore = new Bore() {
            Position = new(30, 20),
            Depth = 10,
            Tool = tool
        };

        var route = new Route() {
            StartPosition = new(10, 10),
            EndPosition = new(200, 200),
            StartDepth = 10,
            EndDepth = 10,
            Tool = tool,
            PassCount = 1,
        };

        var pocket = new Pocket() {
            PositionA = new(100, 100),
            PositionB = new(400, 100),
            PositionC = new(400, 1105),
            PositionD =  new(100, 1105),
            StartDepth = 10,
            EndDepth = 10,
            Tool = tool,
            PassCount = 1,
            Climb = 0
        };

        var partA = new CNCPart() {
            FileName = "PartA",
            Description = "A large part",
            Width = 100,
            Length = 200,
            Qty = 1,
            Material = new() { Name = "MDF", Thickness = 19},
            Tokens = new List<Token>() {  bore }
        };

        var partB = new CNCPart() {
            FileName = "PartB",
            Description = "A large part",
            Width = 200,
            Length = 500,
            Qty = 10,
            Material = new() { Name = "MDF", Thickness = 19 },
            Tokens = new List<Token>() { bore, route }
        };

        var partC = new CNCPart() {
            FileName = "PartC",
            Description = "A large part",
            Width = 250,
            Length = 300,
            Qty = 10,
            Material = new() { Name = "White Melamine", Thickness = 19 },
            Tokens = new List<Token>() { bore, route }
        };

        var batch = new CNCBatch() {
            Name = "JobName",
            Parts = new List<CNCPart> {
                partA,/*
                partB,
                partC,*/
            }
        };

        return batch;

    }

}
