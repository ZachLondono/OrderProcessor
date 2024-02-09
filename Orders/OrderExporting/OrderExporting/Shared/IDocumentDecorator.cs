using QuestPDF.Infrastructure;

namespace OrderExporting.Shared;

public interface IDocumentDecorator {

    public void Decorate(IDocumentContainer container);

}