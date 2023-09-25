using ApplicationCore.Infrastructure.Bus;
using ApplicationCore.Shared.Data.Ordering;
using Dapper;

namespace ApplicationCore.Features.Orders.ProductDrawings;

public class SaveProductDrawing {

    public record Command(ProductDrawing Drawing) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IOrderingDbConnectionFactory _connectionFactory;

        public Handler(IOrderingDbConnectionFactory connectionFactory) {
            _connectionFactory = connectionFactory;
        }

        public override async Task<Response> Handle(Command command) {

            try {

                using var connection = await _connectionFactory.CreateConnection();

                int? exists = await connection.QueryFirstOrDefaultAsync<int>("SELECT 1 FROM product_drawings WHERE id = @Id;", command);

                if (exists is int n && n == 1) {

                    await connection.ExecuteAsync(
                        """
                        UPDATE product_drawings
                        SET name = @Name, dxf_data = @DXFData
                        WHERE id = @Id;
                        """,
                        command);

                } else {

                    await connection.ExecuteAsync(
                        """
                        INSERT INTO product_drawings
                            (id,
                            product_id,
                            dxf_data,
                            name)
                        VALUES (
                            Id,
                            ProductId,
                            DXFData,
                            Name
                        );
                        """,
                        command);

                }

            } catch (Exception ex) {

                return Response.Error(new() {
                    Title = "Failed to Save Product Drawing",
                    Details = ex.Message
                });

            }

            return Response.Success();

        }

    }

}
