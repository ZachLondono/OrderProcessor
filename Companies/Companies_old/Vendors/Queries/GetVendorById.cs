using Domain.Companies.Entities;
using Domain.Companies.ValueObjects;
using ApplicationCore.Shared.Data.Companies;
using ApplicationCore.Infrastructure.Bus;
using Dapper;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Companies.Vendors.Queries;

internal class GetVendorById {

    public record Query(Guid Id) : IQuery<Vendor?>;

    public class Handler : QueryHandler<Query, Vendor?> {

        private readonly ICompaniesDbConnectionFactory _factory;
        private readonly ILogger<Handler> _logger;

        public Handler(ICompaniesDbConnectionFactory factory, ILogger<Handler> logger) {
            _factory = factory;
            _logger = logger;
        }

        public override async Task<Response<Vendor?>> Handle(Query query) {

            using var connection = await _factory.CreateConnection();

            try {

                var data = await connection.QuerySingleOrDefaultAsync<VendorDataModel>(
                                                                                                                                                                """
                SELECT
                    
                    vendors.id,
                    vendors.name,
                    vendors.phone,
                    vendors.logo,

                    vendors.export_db_order AS ExportDBOrder,
                    vendors.export_mdf_door_order AS ExportMDFDoorOrder,
                    vendors.export_ext_file AS ExportExtFile,

                    vendors.release_invoice AS ReleaseInvoice,
                    vendors.release_invoice_send_email AS SendInvoiceEmail,
                    vendors.release_invoice_email_recipients AS InvoiceEmailRecipients,

                    vendors.release_packing_list AS ReleasePackingList,
                    vendors.release_job_summary AS ReleaseJobSummary,
                    vendors.release_include_invoice AS ReleaseIncludeInvoice,
                    vendors.release_send_email AS ReleaseSendEmail,
                    vendors.release_email_recipients AS ReleaseEmailRecipients,

                    addresses.line1 AS AddrLine1,
                    addresses.line2 AS AddrLine2,
                    addresses.line3 AS AddrLine3,
                    addresses.city AS AddrCity,
                    addresses.state AS AddrState,
                    addresses.zip AS AddrZip,
                    addresses.country AS AddrContry

                FROM vendors
                    
                    LEFT JOIN addresses ON vendors.address_id = addresses.id

                WHERE vendors.id = @Id;
                """, query);

                if (data is null) {
                    return (Vendor?)null;
                }

                var vendor = data.AsVendor();

                return vendor;

            } catch (Exception ex) {

                _logger.LogError(ex, "Exception thrown while trying to load vendor");

                return new Error() {
                    Title = "Could not Load Vendor Info From Database",
                    Details = ex.Message
                };

            }

        }

        public class VendorDataModel {

            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public byte[] Logo { get; set; } = Array.Empty<byte>();

            public string AddrLine1 { get; set; } = string.Empty;
            public string AddrLine2 { get; set; } = string.Empty;
            public string AddrLine3 { get; set; } = string.Empty;
            public string AddrCity { get; set; } = string.Empty;
            public string AddrState { get; set; } = string.Empty;
            public string AddrZip { get; set; } = string.Empty;
            public string AddrCountry { get; set; } = string.Empty;

            public bool ExportDBOrder { get; set; }
            public bool ExportMDFDoorOrder { get; set; }
            public bool ExportExtFile { get; set; }
            public string ExportOutputDirectory { get; set; } = string.Empty;

            public bool ReleaseInvoice { get; set; }
            public string InvoiceOutputDirectory { get; set; } = string.Empty;
            public bool SendInvoiceEmail { get; set; }
            public string InvoiceEmailRecipients { get; set; } = string.Empty;

            public bool ReleasePackingList { get; set; }
            public bool ReleaseJobSummary { get; set; }
            public bool ReleaseIncludeInvoice { get; set; }
            public bool ReleaseSendEmail { get; set; }
            public string ReleaseEmailRecipients { get; set; } = string.Empty;
            public string ReleaseOutputDirectory { get; set; } = string.Empty;

            public Vendor AsVendor() {

                var address = new Address() {
                    Line1 = AddrLine1,
                    Line2 = AddrLine2,
                    Line3 = AddrLine3,
                    City = AddrCity,
                    State = AddrState,
                    Zip = AddrZip,
                    Country = AddrCountry
                };

                var exportProfile = new ExportProfile() {
                    ExportDBOrder = ExportDBOrder,
                    ExportMDFDoorOrder = ExportMDFDoorOrder,
                    ExportExtFile = ExportExtFile
                };

                var releaseProfile = new ReleaseProfile() {
                    IncludeInvoice = ReleaseIncludeInvoice,
                    GeneratePackingList = ReleasePackingList,
                    GenerateJobSummary = ReleaseJobSummary,
                    SendReleaseEmail = ReleaseSendEmail,
                    ReleaseEmailRecipients = ReleaseEmailRecipients,

                    GenerateInvoice = ReleaseInvoice,
                    SendInvoiceEmail = SendInvoiceEmail,
                    InvoiceEmailRecipients = InvoiceEmailRecipients
                };

                return new Vendor(Id, Name, address, Phone, Logo, exportProfile, releaseProfile);

            }

        }

    }

}
