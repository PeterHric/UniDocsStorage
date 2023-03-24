using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocumentStorage.MsSql;

public class MssqlDocumentStorage : IDocumentStorage
{
    private readonly string _connectionString;

    public MssqlDocumentStorage(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultMsSqlConnection") ??
            throw new Exception("Missing connection string in configuration !");
    }

    /// <summary>
    /// Gets Document by ID
    /// </summary>
    /// <param name="id">Id of the document to be retireved</param>
    /// <returns>The retrieved document, if successul. Empty document otherwise.</returns>
    public async Task<DocumentEntity> GetAsync(string id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand("SELECT * FROM Documents WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DocumentEntity
            {
                Id = reader.GetString(reader.GetOrdinal("Id")),
                Tags = reader.GetString(reader.GetOrdinal("Tags")).Split(',').ToList(),
                Data = JsonConvert.DeserializeObject<JObject>(reader.GetString(reader.GetOrdinal("Data")))
            };
        }

        throw new Exception($"Failed to retrieve document with id:{id}");
    }

    public async Task StoreAsync(DocumentEntity document)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand("INSERT INTO Documents (Id, Tags, Data) VALUES (@Id, @Tags, @Data)", connection);
        command.Parameters.AddWithValue("@Id", document.Id);
        command.Parameters.AddWithValue("@Tags", string.Join(",", document.Tags));
        command.Parameters.AddWithValue("@Data", JsonConvert.SerializeObject(document.Data));
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(DocumentEntity document)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand("UPDATE Documents SET Tags = @Tags, Data = @Data WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", document.Id);
        command.Parameters.AddWithValue("@Tags", string.Join(",", document.Tags));
        command.Parameters.AddWithValue("@Data", JsonConvert.SerializeObject(document.Data));
        await command.ExecuteNonQueryAsync();
    }
}