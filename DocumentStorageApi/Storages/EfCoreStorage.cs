using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DocumentStorage.EFCore;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(string id);

    Task<IReadOnlyList<T>> GetAllAsync();

    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);
}

/// <summary>
/// Document repos should extend IDocumentStorage for more comfortable data manipulation
/// and also necessary SaveChanges*() methods to save EF DB context changes.
/// </summary>
public interface IDocumentRepository : IDocumentStorage
{
    Task RemoveAsync(DocumentEntity entity);

    Task<bool> ExistsAsync(string id);

    Task<int> SaveChangesAsync();

    int SaveChanges();
}

public class EfDocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _dbContext;

    public EfDocumentRepository(DocumentDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task<DocumentEntity> GetAsync(string id) =>
        await _dbContext.Documents.FindAsync(id) ?? throw new Exception($"Could not retrieve document with ID: {id}");

    public async Task StoreAsync(DocumentEntity entity) =>
        await _dbContext.Documents.AddAsync(entity);

    public Task UpdateAsync(DocumentEntity entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }
    public Task RemoveAsync(DocumentEntity entity)
    {
        _dbContext.Documents.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string id) =>
        await _dbContext.Documents.AnyAsync(x => x.Id == id);


    public async Task<int> SaveChangesAsync() =>
        await _dbContext.SaveChangesAsync();

    public int SaveChanges() =>
         _dbContext.SaveChanges();
}

public interface IUnitOfWork : IDisposable
{
    int SaveChanges();

    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly DocumentDbContext _dbContext;
    private bool _disposed;

    public UnitOfWork(DocumentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IDocumentRepository DocumentRepository => new EfDocumentRepository(_dbContext);

    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _dbContext.Dispose();
            _disposed = true;
        }
    }
}

public class DocumentDbContext : DbContext
{

    public DbSet<DocumentEntity> Documents { get; set; }

    public DocumentDbContext(DbContextOptions options) : base(options) =>
        Documents = Set<DocumentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<DocumentEntity>().HasKey(x => x.Id);

    /// <summary>
    /// Called by EF to configure the DB
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var isConfigured = optionsBuilder.IsConfigured;
        if (!isConfigured)
        {
            // Choose underlying persistency engine:
            //optionsBuilder.UseMySql(_mySqlConnectionString, ServerVersion.AutoDetect(_mySqlConnectionString));
            optionsBuilder.UseMongoDb(_mongoDbConnectionString);
            // ToDo: Add support for other types of DB
            //optionsBuilder.UseInMemoryDatabase("DalInMemoryDB");
            //optionsBuilder.UseSqlite(CreateInMemoryDatabase());
            //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Test");
        }
    }

    protected string _mySqlConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ?? // Docker
                                              "server=127.0.0.1;uid=root;pwd=root;database=DocumentsDB";       // native debug

    /// <summary>
    /// MongoDB connection string shape:  "mongodb://<username>:<password>@<host>:<port>/<database>"
    /// </summary>
    protected string _mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? // Docker
                                                "mongodb://root:root:localhost:27017/DocumentsDB";                 // native debug
}