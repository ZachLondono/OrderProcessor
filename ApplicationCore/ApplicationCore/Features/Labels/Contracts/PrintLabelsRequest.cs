using ApplicationCore.Features.Labels.Domain;
using ApplicationCore.Infrastructure;

namespace ApplicationCore.Features.Labels.Contracts;

public record PrintLabelsRequest(IReadOnlyList<Label> Labels, LabelPrinterConfiguration Configuration) : ICommand;
