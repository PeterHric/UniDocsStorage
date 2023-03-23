namespace DocumentStorage.InMemory;

public class InMemoryDocumentStorage : IDocumentStorage
{
    private readonly IDictionary<string, DocumentEntity> _documents = new Dictionary<string, DocumentEntity>();

    public async Task<DocumentEntity> GetAsync(string id) =>
            await Task.Run(() => _documents.TryGetValue(id, out var document) ? document : throw new Exception(""));

    /// <summary>
    /// Mimics asynchronicity
    /// </summary>
    public Task<DocumentEntity> GetMimicAsync(string id) =>
        Task.FromResult(_documents.TryGetValue(id, out var document) ? document : throw new Exception(""));

    /// <summary>
    /// Stores document, but synchronous way
    /// Only mimics asynchronicity, in order to implement IDocumentStorage
    /// </summary>
    /// <param name="document">The document to store</param>
    /// <returns>Dummy Task.CompletedTask</returns>
    public Task StoreAsync(DocumentEntity document)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates a document, but synchronous way
    /// Only mimics asynchronicity, in order to implement IDocumentStorage
    /// </summary>
    /// <param name="document">The document to update</param>
    /// <returns>Dummy Task.CompletedTask</returns>
    public Task UpdateAsync(DocumentEntity document)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }
}