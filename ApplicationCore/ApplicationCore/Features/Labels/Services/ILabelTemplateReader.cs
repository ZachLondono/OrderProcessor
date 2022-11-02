using ApplicationCore.Features.Labels.Domain;

namespace ApplicationCore.Features.Labels.Services;

internal interface ILabelTemplateReader {

    public LabelTemplate GetTemplateFromFile(string filepath);

}