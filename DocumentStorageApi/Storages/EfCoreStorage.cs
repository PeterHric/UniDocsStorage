using DocumentStorage;
using Microsoft.EntityFrameworkCore;

namespace DocumentStorageApi.Storages;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(string id);

    Task<IReadOnlyList<T>> GetAllAsync();

    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);
}

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
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
    {
    }

    public DbSet<DocumentEntity> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentEntity>().HasKey(x => x.Id);
    }
}