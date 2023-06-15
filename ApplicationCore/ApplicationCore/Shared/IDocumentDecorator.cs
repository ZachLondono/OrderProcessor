using QuestPDF.Infrastructure;

namespace ApplicationCore.Shared;

public interface IDocumentDecorator {

    public void Decorate(IDocumentContainer container);

}