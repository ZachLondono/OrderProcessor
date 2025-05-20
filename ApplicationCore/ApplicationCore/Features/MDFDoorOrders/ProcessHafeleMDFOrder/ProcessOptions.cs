namespace ApplicationCore.Features.MDFDoorOrders.ProcessHafeleMDFOrder;

public class ProcessOptions {

    public string DataFile { get; private set; } = string.Empty;

    private bool _generateInvoice = false;
    public bool GenerateInvoice {
        get => _generateInvoice;
        set {
            _generateInvoice = value;
            if (value) {
                SendInvoiceEmail = false;
            }
        }
    }
    public string InvoicePDFOutputDirectory { get; set; } = string.Empty;

    public bool SendInvoiceEmail { get; set; } = false;
    public bool PreviewInvoiceEmail { get; set; } = false;
    public List<Email> InvoiceEmailRecipients { get; set; } = [];

    public bool PostToGoogleSheets { get; set; } = false;

    public bool FillOrderSheet { get; set; } = false;
    public string OrderSheetTemplatePath { get; set; } = @"R:\Forms\_Door Order 2024, 2.114.xlsm";
    public string OrderSheetOutputDirectory { get; set; } = string.Empty;

    public class Email(string address) {
        public string Address { get; set; } = address;
    }

    public void SetDataFile(string file) {
        
        if (string.IsNullOrWhiteSpace(file)) {
            DataFile = string.Empty;
            return;
        }

        if (!File.Exists(file)) {
            DataFile = string.Empty;
            return;
        }

        DataFile = file;

        var dirs = FindDirectories(DataFile);

        OrderSheetOutputDirectory = dirs.Orders;
        InvoicePDFOutputDirectory = dirs.Invoices;

    }

    private static Directories FindDirectories(string dataFile) {

        var directory = Path.GetDirectoryName(dataFile);

        if (directory is null) {
            return new("", "");
        }

        const string rootDir = @"R:\Door Orders\Hafele\Orders";

        if (!directory.StartsWith(rootDir)) {
            return new(directory, directory);
        }

        string orders = string.Empty;
        string invoices = string.Empty;

        while (true) {

            var dirInfo = new DirectoryInfo(directory);

            if (dirInfo.FullName == rootDir) {

                break;

            } else {

                var subDirs = dirInfo.GetDirectories();

                if (orders == string.Empty) orders = subDirs.FirstOrDefault(file => file.Name.Contains("orders", StringComparison.InvariantCultureIgnoreCase))?.FullName ?? "";
                if (invoices == string.Empty) invoices = subDirs.FirstOrDefault(file => file.Name.Contains("bill", StringComparison.InvariantCultureIgnoreCase))?.FullName ?? "";

            }

            if (dirInfo.Parent is null) {
                break;
            }

            if (dirInfo.Parent.FullName == rootDir) {
                if (orders == string.Empty) orders = directory; 
                if (invoices == string.Empty) invoices = directory;
                break;
            }

            directory = dirInfo.Parent.FullName;

        }

        return new(orders, invoices);

    }

    private record Directories(string Orders, string Invoices);

}