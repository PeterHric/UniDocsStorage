namespace DocumentStorage.InMemory;

public class InMemoryDocumentStorage : IDocumentStorage
{
    private readonly IDictionary<string, DocumentEntity> _documents = new Dictionary<string, DocumentEntity>();

    public Task<DocumentEntity> GetAsync(string id)
    {
        return Task.FromResult(_documents.TryGetValue(id, out var document) ? document : null);
    }

    public Task StoreAsync(DocumentEntity document)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DocumentEntity document)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }
}