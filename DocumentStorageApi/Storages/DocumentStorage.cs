using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocumentStorage;

public class DocumentEntity
{
    [Required]
    [JsonProperty("id")]
    public string Id { get; set; } = String.Empty;

    [Required]
    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = new List<string>();

    [Required]
    [JsonProperty("data")]
    [XmlElement("data")]
    [XmlElement(typeof(Dictionary<string, string>))]
    [MessagePack.Key("data")]
    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// For case of larger APIs, implement also with Repository pattern with UoW, DbContext
/// with ORM support such as EFCore, or NHibernate 
/// </summary>
public interface IDocumentStorage
{
    Task<DocumentEntity> GetAsync(string id);

    Task StoreAsync(DocumentEntity document);

    Task UpdateAsync(DocumentEntity document);
}

