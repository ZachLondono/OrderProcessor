using QuestPDF.Infrastructure;

namespace ApplicationCore.Features.Shared;

public interface IDocumentDecorator {

    public void Decorate(IDocumentContainer container);

}