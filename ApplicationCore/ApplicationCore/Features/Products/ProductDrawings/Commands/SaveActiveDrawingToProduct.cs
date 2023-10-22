using ApplicationCore.Features.Orders.ProductDrawings.Models;
using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using ApplicationCore.Shared.Services;
using BricscadApp;
using Dapper;

namespace ApplicationCore.Features.Orders.ProductDrawings.Commands;

public class SaveActiveDrawingToProduct {

    public record Command(Guid ProductId, Guid DrawingId, string DrawingName) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _connectionFactory;

        public Handler(IOrderingDbConnectionFactory connectionFactory) {
            _connectionFactory = connectionFactory;
        }

        public override async Task<Response> Handle(Command command) {

            try {

                var getDataResponse = await GetActiveDrawingData();

                var drawing = new ProductDrawing() {
                    Id = command.DrawingId,
                    ProductId = command.ProductId,
                    Name = command.DrawingName,
                    DXFData = Array.Empty<byte>()
                };
                
                Error? error = null;
                getDataResponse.Match(
                    data => drawing.DXFData = data,
                    e => error = e);

                if (error is not null) {
                    return error;
                }

                using var connection = await _connectionFactory.CreateConnection();

                int? exists = await connection.QueryFirstOrDefaultAsync<int>("SELECT 1 FROM product_drawings WHERE id = @DrawingId;", command);

                if (exists is int n && n == 1) {

                    await connection.ExecuteAsync(
                        """
                        UPDATE product_drawings
                          SET name = @Name, dxf_data = @DXFData
                        WHERE id = @Id;
                        """,
                        drawing);

                } else {

                    await connection.ExecuteAsync(
                        """
                        INSERT INTO product_drawings
                            (id,
                            product_id,
                            dxf_data,
                            name)
                        VALUES (
                            @Id,
                            @ProductId,
                            @DXFData,
                            @Name
                        );
                        """,
                        drawing);

                }

            } catch (Exception ex) {

                return Response.Error(new() {
                    Title = "Failed to Save Product Drawing",
                    Details = ex.Message
                });

            }

            return Response.Success();

        }

        private async Task<Response<byte[]>> GetActiveDrawingData() {

            var tmpPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            var exportResponse = ExportActiveDocument(tmpPath);

            tmpPath += ".dxf";
    
            Error? error = null;
            exportResponse.OnError(e => error = e);
    
            if (error is not null) {
                return error!;
            }
    
            var data = await File.ReadAllBytesAsync(tmpPath);
            File.Delete(tmpPath);
    
            return CompressionService.Compress(data);
    
        }

        private Response ExportActiveDocument(string exportFileName) {

            AcadApplication? app = null; 

            try {
                app ??= BricsCADApplicationRetriever.GetAcadApplication(false);
            } catch { }
        
            if (app is null) {
    
                return new Error() {
                    Title = "BricsCAD Not Found",
                    Details = "Failed to get instance of BricsCAD application."
                };
    
            }       
    
            AcadDocument? document = null;
    
            try {
                document = app.ActiveDocument;
            } catch { }
    
            if (document is null) {
                return new Error() {
                    Title = "No Document Found",
                    Details = "Failed to get active BricsCAD document."
                };
            }
    
            try {
    
                document.Export(exportFileName, "dxf", document.ActiveSelectionSet);
    
            } catch {
    
                return new Error() {
                    Title = "Could Not Export DXF",
                    Details = "Error occurred while trying to export dxf from BricsCAD"
                };
    
            }
    
            return Response.Success();

        }

    }

}
