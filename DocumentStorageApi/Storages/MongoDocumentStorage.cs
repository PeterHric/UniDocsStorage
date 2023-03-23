using MongoDB.Driver;

namespace DocumentStorage.Mongo;

public class MongoDocumentStorage : IDocumentStorage
{
    private readonly IMongoCollection<DocumentEntity> _collection;

    public MongoDocumentStorage(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("mymongodb");
        _collection = database.GetCollection<DocumentEntity>("documents");
    }

    public async Task StoreAsync(DocumentEntity document) =>
        await _collection.InsertOneAsync(document);


    public async Task UpdateAsync(DocumentEntity document) =>
        await _collection.ReplaceOneAsync(d => d.Id == document.Id, document);

    public async Task<DocumentEntity> GetAsync(string id) =>
         (await _collection.FindAsync(d => d.Id == id)).FirstOrDefault();

    public async Task<List<DocumentEntity>> GetAllAsync() =>
        await (await _collection.FindAsync(_ => true)).ToListAsync();

}