using ApplicationCore.Features.ExcelTemplates.Contracts;
using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Infrastructure;
using ApplicationCore.Shared;

namespace ApplicationCore.Features.ExcelTemplates.Handlers;

public class FillTemplateHandler : CommandHandler<FillTemplateRequest, FillTemplateResponse> {

    private readonly IFileReader _fileReader;
    private readonly IExcelTemplateFactory _factory;
    private readonly IExcelPrinter _printer;

    public FillTemplateHandler(IFileReader fileReader, IExcelTemplateFactory factory, IExcelPrinter printer) {
        _fileReader = fileReader;
        _factory = factory;
        _printer = printer;
    }

    public override async Task<Response<FillTemplateResponse>> Handle(FillTemplateRequest request) {

        var filler = new ClosedXMLTemplateFiller(request.Configuration, _fileReader, _factory, _printer);
        var result = await filler.FillTemplate(request.Model, request.OutputDirectory, request.FileName, request.Print);

        var response = new FillTemplateResponse(result);
        return new(response);

    }

}
