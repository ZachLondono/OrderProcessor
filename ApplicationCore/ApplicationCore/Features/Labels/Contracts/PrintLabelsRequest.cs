using ApplicationCore.Features.Labels.Domain;
using ApplicationCore.Infrastructure;
using MediatR;

namespace ApplicationCore.Features.Labels.Contracts;

public record PrintLabelsRequest(IReadOnlyList<Label> Labels, LabelPrinterConfiguration Configuration) : ICommand;
