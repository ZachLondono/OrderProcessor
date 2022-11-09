using ApplicationCore.Features.Labels.Domain;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Labels.Contracts;

public record PrintLabelRequest(Label Label, LabelPrinterConfiguration Configuration) : ICommand;
