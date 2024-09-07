using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using Dapper;
using System.Data;
using Domain.Infrastructure.Bus;
using Companies.Infrastructure;
using Domain.Infrastructure.Data;

namespace Companies.Vendors.Commands;

public class UpdateVendor {

    public record Command(Vendor Vendor) : ICommand;

    public class Handler : CommandHandler<Command> {

        private readonly ICompaniesDbConnectionFactory _factory;

        public Handler(ICompaniesDbConnectionFactory factory) {
            _factory = factory;
        }

        public override async Task<Response> Handle(Command command) {

            using var connection = await _factory.CreateConnection();

            connection.Open();
            var trx = connection.BeginTransaction();

            var vendor = command.Vendor;
            await UpdateVendor(connection, trx, vendor);
            await UpdateAddress(vendor.Address, vendor.Id, connection, trx);

            trx.Commit();
            connection.Close();

            return Response.Success();

        }

        private static async Task UpdateVendor(ISynchronousDbConnection connection, ISynchronousDbTransaction trx, Vendor vendor) {

            connection.Execute(
                """
                UPDATE vendors
                SET
                    name = @Name,
                    phone = @Phone,
                    logo = @Logo,

                    export_db_order = @ExportDBOrder,
                    export_mdf_door_order = @ExportMDFDoorOrder,
                    export_ext_file = @ExportExtFile,

                    release_invoice = @ReleaseInvoice,
                    release_invoice_send_email = @ReleaseSendInvoiceEmail,
                    release_invoice_email_recipients = @ReleaseInvoiceEmailRecipients,

                    release_include_invoice = @ReleaseIncludeInvoice,
                    release_packing_list = @ReleasePackingList,
                    release_job_summary = @ReleaseJobSummary,
                    release_send_email = @ReleaseSendEmail,
                    release_email_recipients = @ReleaseEmailRecipients

                WHERE id = @Id;
                """, new {
                    vendor.Id,
                    vendor.Name,
                    vendor.Phone,
                    vendor.Logo,

                    vendor.ExportProfile.ExportDBOrder,
                    vendor.ExportProfile.ExportMDFDoorOrder,
                    vendor.ExportProfile.ExportExtFile,

                    ReleaseInvoice = vendor.ReleaseProfile.GenerateInvoice,
                    ReleaseSendInvoiceEmail = vendor.ReleaseProfile.SendInvoiceEmail,
                    ReleaseInvoiceEmailRecipients = vendor.ReleaseProfile.InvoiceEmailRecipients,

                    ReleaseIncludeInvoice = vendor.ReleaseProfile.IncludeInvoice,
                    ReleasePackingList = vendor.ReleaseProfile.GeneratePackingList,
                    ReleaseJobSummary = vendor.ReleaseProfile.GenerateJobSummary,
                    ReleaseSendEmail = vendor.ReleaseProfile.SendReleaseEmail,
                    vendor.ReleaseProfile.ReleaseEmailRecipients,

                }, trx);

        }

        public static async Task UpdateAddress(Address address, Guid vendorId, ISynchronousDbConnection connection, ISynchronousDbTransaction trx) {
            
            connection.Execute(
                $"""
                UPDATE addresses
                SET
                    line1 = @Line1,
                    line2 = @Line2,
                    line3 = @Line3,
                    city = @City,
                    state = @State,
                    zip = @Zip,
                    country = @Country
                WHERE id = (SELECT address_id FROM vendors WHERE id = @VendorId);
                """, new {
                    VendorId = vendorId,
                    address.Line1,
                    address.Line2,
                    address.Line3,
                    address.City,
                    address.State,
                    address.Zip,
                    address.Country
                }, trx);

        }

    }

}
