using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace DocumentStorage.Hdd;

public class HddDocumentStorage : IDocumentStorage
{
    private readonly string _folderPath;

    public HddDocumentStorage(IConfiguration configuration)
    {
        _folderPath = configuration.GetValue<string>("HddStoragePath");
    }

    public async Task<DocumentEntity> GetAsync(string id)
    {
        var filePath = Path.Combine(_folderPath, id + ".json");
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<DocumentEntity>(json);
        }

        throw new Exception($"Failed to retrieve document with id:{id} from file path: {filePath}");
    }

    public async Task StoreAsync(DocumentEntity document)
    {
        var filePath = Path.Combine(_folderPath, document.Id + ".json");
        await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(document));
    }

    public async Task UpdateAsync(DocumentEntity document)
    {
        var filePath = Path.Combine(_folderPath, document.Id + ".json");
        await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(document));
    }

    public List<DocumentEntity> GetAll()
    {
        var result = new List<DocumentEntity>();
        foreach (var filePath in Directory.EnumerateFiles(_folderPath, "*.json"))
        {
            var json = File.ReadAllText(filePath);
            result.Add(JsonConvert.DeserializeObject<DocumentEntity>(json));
        }
        return result;
    }
}