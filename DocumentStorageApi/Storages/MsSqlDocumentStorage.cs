using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DocumentStorage.MsSql;

public class MssqlDocumentStorage : IDocumentStorage
{
    private readonly string _connectionString;

    public MssqlDocumentStorage(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public DocumentEntity Create(DocumentEntity document)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand("INSERT INTO Documents (Id, Tags, Data) VALUES (@Id, @Tags, @Data)", connection);
        command.Parameters.AddWithValue("@Id", document.Id);
        command.Parameters.AddWithValue("@Tags", string.Join(",", document.Tags));
        command.Parameters.AddWithValue("@Data", JsonConvert.SerializeObject(document.Data));
        command.ExecuteNonQuery();
        return document;
    }

    public DocumentEntity Update(DocumentEntity document)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand("UPDATE Documents SET Tags = @Tags, Data = @Data WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", document.Id);
        command.Parameters.AddWithValue("@Tags", string.Join(",", document.Tags));
        command.Parameters.AddWithValue("@Data", JsonConvert.SerializeObject(document.Data));
        command.ExecuteNonQuery();
        return document;
    }

    public DocumentEntity GetById(string id)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand("SELECT * FROM Documents WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new DocumentEntity
            {
                Id = reader.GetString(reader.GetOrdinal("Id")),
                Tags = reader.GetString(reader.GetOrdinal("Tags")).Split(',').ToList(),
                Data = JsonConvert.DeserializeObject(reader.GetString(reader.GetOrdinal("Data")))
            };
        }
        else
        {
            return null;
        }
    }

    public List<DocumentEntity> GetAll()
    {
        var result = new List<DocumentEntity>();
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var command = new SqlCommand("SELECT * FROM Documents", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new DocumentEntity
            {
                Id = reader.GetString(reader.GetOrdinal("Id")),
                Tags = reader.GetString(reader.GetOrdinal("Tags")).Split(',').ToList(),
                Data = JsonConvert.DeserializeObject(reader.GetString(reader.GetOrdinal("Data")))
            });
        }
        return result;
    }
}