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

public interface IDocumentStorage
{
    Task<DocumentEntity> GetAsync(string id);

    Task StoreAsync(DocumentEntity document);

    Task UpdateAsync(DocumentEntity document);
}

