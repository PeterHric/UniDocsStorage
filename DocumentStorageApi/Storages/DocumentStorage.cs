using System.ComponentModel.DataAnnotations;

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
    public IList<string> Tags { get; set; } = new List<string>();

    [Required]
    [JsonProperty("data")]
    public JObject Data { get; set; } = new JObject();
}

/// <summary>
/// ToDo: With larger API, implement the Repository pattern with UoW, DbContext
/// with ORM support such as EFCore, or NHibernate 
/// </summary>
public interface IDocumentStorage
{
    Task<DocumentEntity> GetAsync(string id);

    Task StoreAsync(DocumentEntity document);

    Task UpdateAsync(DocumentEntity document);
}

