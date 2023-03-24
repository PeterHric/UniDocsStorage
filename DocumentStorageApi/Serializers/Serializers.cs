using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;

using MessagePack;
using DocumentStorage;
using Newtonsoft.Json;
using System.Text;

namespace Serializers;

public interface ISerializer
{
    string Serialize(DocumentEntity document);

    void Serialize(DocumentEntity document, out Stream stream);

    Task<byte[]> SerializeAsync(DocumentEntity document);

    string ContentType { get; }
}

public abstract class ASerializer : ISerializer
{
    virtual public string ContentType { get; protected set; } = "abstract";

    public abstract string Serialize(DocumentEntity document);

    public abstract void Serialize(DocumentEntity document, out Stream stream);

    public abstract Task<byte[]> SerializeAsync(DocumentEntity document);

    /// <summary>
    /// Factory method returns appropriate serializer instance based on expected format type.
    /// </summary>
    /// <param name="formatType">Defines type of serializer: "application/json" | "application/xml" | "application/x-msgpack"</param>
    /// <returns>Instance of demanded serializer</returns>
    static public ISerializer GetSerializer(string formatType)
    {
        switch (formatType)
        {
            case "application/json":
                return new JsonSerializer();
            case "application/xml":
                return new XmlDocumentSerializer();
            case "application/x-msgpack":
                return new MsgPackSerializer();
            // ToDo: Add support for other type of serializers
            default:
                throw new ArgumentException("Unsupported format type !");
        }
    }
}

public class XmlDocumentSerializer : ASerializer
{
    override public string ContentType { get; protected set; } = "application/xml";

    override public string Serialize(DocumentEntity document)
    {
        using var writer = new StringWriter();
        _serializer.Serialize(writer, document);
        return writer.ToString();
    }

    override public void Serialize(DocumentEntity document, out Stream stream)
    {
        stream = new MemoryStream();
        _serializer.Serialize(stream, document);
        stream.Seek(0, SeekOrigin.Begin);
    }

    override public async Task<byte[]> SerializeAsync(DocumentEntity document)
    {
        using var stream = new MemoryStream();
        _serializer.Serialize(stream, document);
        stream.Seek(0, SeekOrigin.Begin);
        return await Task.FromResult(stream.ToArray());
    }

    XmlSerializer _serializer = new XmlSerializer(typeof(DocumentEntity));
}

public class MsgPackSerializer : ASerializer
{
    override public string ContentType { get; protected set; } = "application/x-msgpack";

    override public string Serialize(DocumentEntity document)
    {
        var options = MessagePackSerializerOptions.Standard;
        var bytes = MessagePackSerializer.Serialize(document, options);
        return Convert.ToBase64String(bytes);
    }

    override public void Serialize(DocumentEntity document, out Stream stream)
    {
        var options = MessagePackSerializerOptions.Standard;
        var bytes = MessagePackSerializer.Serialize(document, options);
        stream = new MemoryStream(bytes);
    }

    override public async Task<byte[]> SerializeAsync(DocumentEntity document)
    {
        var options = MessagePackSerializerOptions.Standard;
        using var stream = new MemoryStream();
        await MessagePackSerializer.SerializeAsync(stream, document, options);
        stream.Seek(0, SeekOrigin.Begin);
        return stream.ToArray();
    }
}

public class JsonSerializer : ASerializer
{
    override public string ContentType { get; protected set; } = "application/json";

    override public string Serialize(DocumentEntity document)
    {
        return JsonConvert.SerializeObject(document);
    }

    override public void Serialize(DocumentEntity document, out Stream stream)
    {
        var json = Serialize(document);
        var bytes = Encoding.UTF8.GetBytes(json);
        stream = new MemoryStream(bytes);
    }

    override public async Task<byte[]> SerializeAsync(DocumentEntity document)
    {
        var json = Serialize(document);
        var bytes = Encoding.UTF8.GetBytes(json);
        return await Task.FromResult(bytes);
    }
}