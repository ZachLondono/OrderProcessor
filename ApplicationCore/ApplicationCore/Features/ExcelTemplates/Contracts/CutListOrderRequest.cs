using ApplicationCore.Features.ExcelTemplates.Domain;
using ApplicationCore.Infrastructure;
using MediatR;

namespace ApplicationCore.Features.ExcelTemplates.Contracts;

public record FillTemplateRequest(object Model, string OutputDirectory, string FileName, bool Print, ClosedXMLTemplateConfiguration Configuration) : IQuery<FillTemplateResponse>;
