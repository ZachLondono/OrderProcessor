using OneOf;
using OrderExporting.CNC.Programs.Job;
using OrderExporting.CNC.ReleasePDF;
using OrderExporting.DovetailDBPackingList;
using OrderExporting.HardwareList;
using OrderExporting.Invoice;
using OrderExporting.JobSummary;
using OrderExporting.PackingList;
using OrderExporting.Shared;
using QuestPDF.Fluent;
using UglyToad.PdfPig.Writer;

namespace ApplicationCore.Shared.Services;

public class ReleasePDFBuilder(CNCReleaseDecoratorFactory cncReleaseDecoratorFactory) {

    private readonly List<OneOf<ReleasedJob, ExistingPDF, JobSummary, Invoice, PackingList, DovetailDrawerBoxPackingList, Hardware>> _segments = [];
    private readonly CNCReleaseDecoratorFactory _cncReleaseDecoratorFactory = cncReleaseDecoratorFactory;

    public void AddReleasedJob(ReleasedJob job) => AddReleasedJob(job, _segments.Count);
    public void AddReleasedJob(ReleasedJob job, int position) => _segments.Insert(position, job);

    public void AddExistingPDF(string filePath) => AddExistingPDF(filePath, _segments.Count);
    public void AddExistingPDF(string filePath, int position) => _segments.Insert(position, new ExistingPDF(filePath));

    public void AddJobSummary(JobSummary summary) => AddJobSummary(summary, _segments.Count);
    public void AddJobSummary(JobSummary summary, int position) => _segments.Insert(position, summary);

    public void AddInvoice(Invoice invoice) => AddInvoice(invoice, _segments.Count);
    public void AddInvoice(Invoice invoice, int position) => _segments.Insert(position, invoice);

    public void AddPackingList(PackingList packingList) => AddPackingList(packingList, _segments.Count);
    public void AddPackingList(PackingList packingList, int position) => _segments.Insert(position, packingList);

    public void AddDovetailDBPackingList(DovetailDrawerBoxPackingList packingList) => AddDovetailDBPackingList(packingList, _segments.Count);
    public void AddDovetailDBPackingList(DovetailDrawerBoxPackingList packingList, int position) => _segments.Insert(position, packingList);

    public void AddHardwareList(Hardware hardwareList) => AddHardwareList(hardwareList, _segments.Count);
    public void AddHardwareList(Hardware hardwareList, int position) => _segments.Insert(position, hardwareList);

    public void Clear() => _segments.Clear();

    public async Task<byte[]> BuildAsync() {

        var accumulator = new PDFAccumulator();
        foreach (var segment in _segments) {

            await segment.Match(
                job => {
                    var decorator = _cncReleaseDecoratorFactory.Create(job);
                    accumulator.AddDecorator(decorator);
                    return Task.CompletedTask;
                },
                async existingFile => {
                    var bytes = await File.ReadAllBytesAsync(existingFile.FilePath);
                    await accumulator.AddData(bytes);
                },
                summary => {
                    var decorator = new JobSummaryDecorator(summary);
                    accumulator.AddDecorator(decorator);
                    return Task.CompletedTask;
                },
                invoice => {
                    var decorator = new InvoiceDecorator(invoice);
                    accumulator.AddDecorator(decorator);
                    return Task.CompletedTask;
                },
                packingList => {
                    var decorator = new PackingListDecorator(packingList);
                    accumulator.AddDecorator(decorator);
                    return Task.CompletedTask;
                },
                packingList => {
                    var decorator = new DovetailDBPackingListDecorator(packingList);
                    accumulator.AddDecorator(decorator);
                    return Task.CompletedTask;
                },
                hardwareList => {
                    var decorator = new HardwareListDecorator(hardwareList);
                    accumulator.AddDecorator(decorator);
                    return Task.CompletedTask;
                }
            );

        }

        await accumulator.EndFile();

        return accumulator.FileData;

    }

    private record struct ExistingPDF(string FilePath);

    public class PDFAccumulator {

        public byte[] FileData { get; private set; } = [];
        private readonly List<IDocumentDecorator> _decorators = [];

        public void AddDecorator(IDocumentDecorator decorator) => _decorators.Add(decorator);

        public async Task AddData(byte[] data) {

            await Task.Run(() => {

                List<byte[]> components = [];

                if (FileData.Length > 0) {
                    components.Add(FileData);
                }

                if (_decorators.Count != 0) {
                    var bytes = AccumulateDecorators();
                    components.Add(bytes);
                }

                components.Add(data);

                FileData = PdfMerger.Merge(components);

            });

        }

        private byte[] AccumulateDecorators() {
            var bytes = Document.Create(doc => _decorators.ForEach(dec => dec.Decorate(doc))).GeneratePdf();
            _decorators.Clear();
            return bytes;
        }

        public async Task EndFile() {

            await Task.Run(() => {

                if (_decorators.Count == 0) {
                    return;
                }

                List<byte[]> components = [];

                if (FileData.Length > 0) {
                    components.Add(FileData);
                }

                var bytes = AccumulateDecorators();
                components.Add(bytes);

                FileData = PdfMerger.Merge(components);

            });

        }

    }

}
