using ApplicationCore.Features.Orders.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Orders.Commands;

public class UpdateOrder {

    public record Command(Order Order) : ICommand;

    public class Handler : ICommandHandler<Command> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command request) {

            using var connection = _factory.CreateConnection();

            // TODO: update info fields and add additional items

            const string command = @"UPDATE orders SET
                                        status = @Status,
                                        number = @Number,
                                        name = @Name,
                                        customerid = @CustomerId,
                                        vendorid = @VendorId,
                                        productionnote = @ProductionNote,
                                        orderdate = @OrderDate,
                                        releasedate = @ReleaseDate,
                                        productiondate = @ProductionDate
                                    WHERE id = @Id;";

            await connection.ExecuteAsync(command, new {
                request.Order.Id,
                Status = request.Order.Status.ToString(),
                request.Order.Name,
                request.Order.Number,
                request.Order.CustomerId,
                request.Order.VendorId,
                request.Order.ProductionNote,
                request.Order.OrderDate,
                request.Order.ReleaseDate,
                request.Order.ProductionDate
            });

            return new Response();

        }

    }

}
