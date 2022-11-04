using ApplicationCore.Features.Companies.Domain.ValueObjects;
using ApplicationCore.Infrastructure;
using ApplicationCore.Infrastructure.Data;
using Dapper;

namespace ApplicationCore.Features.Companies.Commands;

public class AssignVendorCompleteProfile {

    public record Command(Guid VendorId, CompleteProfile Profile) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly IDbConnectionFactory _factory;

        public Handler(IDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command request) {

            using var connection = _factory.CreateConnection();

            const string existQuery = "SELECT 1 FROM completeprofiles WHERE vendorid = @VendorId;";

            int exists = connection.QuerySingleOrDefault<int>(existQuery, request);

            string command = "";

            if (exists == 1) {

                command = @"UPDATE completeprofiles
                            SET
                                emailinvoice = @EmailInvoice,
                                invoicepdfdirectory = @InvoicePDFDirectory,
                                emailsenderemail = @EmailSenderEmail,
                                emailsendername = @EmailSenderName,
                                emailsenderpassword = @EmailSenderPassword
                            WHERE vendorid = @VendorId;";

            } else {

                command = @"INSERT INTO completeprofiles
                                (vendorid,
                                emailinvoice,
                                invoicepdfdirectory,
                                emailsendername,
                                emailsenderemail,
                                emailsenderpassword)
                            VALUES
                                (@VendorId,
                                @EmailInvoice,
                                @InvoiecPDFDirectory,
                                @EmailSenderName,
                                @EmailSenderEmail,
                                @EmailSenderPassword);";

            }

            await connection.ExecuteAsync(command, new {
                request.VendorId,
                request.Profile.EmailInvoice,
                request.Profile.InvoicePDFDirectory,
                request.Profile.EmailSenderName,
                request.Profile.EmailSenderEmail,
                request.Profile.EmailSenderPassword,
            });

            return new Response();

        }

    }

}