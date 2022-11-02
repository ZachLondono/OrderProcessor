using ApplicationCore.Features.Labels.Contracts;
using ApplicationCore.Features.Labels.Domain;

namespace ApplicationCore.Features.Labels.Services;

public interface ILabelPrinterService {

    Task<bool> PrintLabelAsync(Label label, LabelPrinterConfiguration configuration);

}
