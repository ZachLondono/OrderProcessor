using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.ExcelTemplates.Contracts;

public record FillTemplateRequest(object Model, string OutputDirectory, string FileName, bool Print, ClosedXMLTemplateConfiguration Configuration) : ICommand<FillTemplateResponse>;
