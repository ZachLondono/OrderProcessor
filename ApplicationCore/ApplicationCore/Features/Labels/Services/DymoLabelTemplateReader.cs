using ApplicationCore.Features.Labels.Domain;
using ApplicationCore.Features.Shared;
using System.Xml;

namespace ApplicationCore.Features.Labels.Services;

internal class DymoLabelTemplateReader : ILabelTemplateReader {

    private readonly IFileReader _fileReader;

    public DymoLabelTemplateReader(IFileReader fileReader) {
        _fileReader = fileReader;
    }

    public LabelTemplate GetTemplateFromFile(string filepath) {

        // TODO: maybe cache label template 

        using var stream = _fileReader.OpenReadFileStream(filepath);

        var doc = new XmlDocument();
        doc.Load(stream);
        var labelObjectNodes = doc.SelectNodes("/DieCutLabel/ObjectInfo");
        if (labelObjectNodes is null)
            throw new ArgumentException($"The provided file is not a valid label template\n{filepath}");

        var fields = new List<string>();

        foreach (XmlNode labelObjectInfo in labelObjectNodes) {
            XmlNodeList childObject = labelObjectInfo.ChildNodes;
            foreach (XmlNode labelObjectNode in childObject) {
                if (!labelObjectNode.Name.Equals("TextObject")) continue;
                var fieldname = labelObjectNode["Name"];
                if (fieldname is null) break;
                fields.Add(fieldname.InnerText);
                break;
            }
        }

        var name = Path.GetFileNameWithoutExtension(filepath);
        var template = new LabelTemplate(name, fields);

        return template;

    }

}