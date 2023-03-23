namespace DocumentStorage.Mongo;

public class MongoDocumentStorage : IDocumentStorage
{
    private readonly IMongoCollection<DocumentEntity> _collection;

    public MongoDocumentStorage(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("mydb");
        _collection = database.GetCollection<DocumentEntity>("documents");
    }

    public DocumentEntity Create(DocumentEntity document)
    {
        _collection.InsertOne(document);
        return document;
    }

    public DocumentEntity Update(DocumentEntity document)
    {
        _collection.ReplaceOne(d => d.Id == document.Id, document);
        return document;
    }

    public DocumentEntity GetById(string id)
    {
        return _collection.Find(d => d.Id == id).FirstOrDefault();
    }

    public List<DocumentEntity> GetAll()
    {
        return _collection.Find(_ => true).ToList();
    }
}