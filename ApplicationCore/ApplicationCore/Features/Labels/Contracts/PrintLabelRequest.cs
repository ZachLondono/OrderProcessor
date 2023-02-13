using ApplicationCore.Features.Labels.Domain;
using ApplicationCore.Infrastructure.Bus;

namespace ApplicationCore.Features.Labels.Contracts;

public record PrintLabelRequest(Label Label, LabelPrinterConfiguration Configuration) : ICommand;
