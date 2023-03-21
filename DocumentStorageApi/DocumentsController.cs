using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//namespace DocumentStorageService.Controllers;
namespace DocumentStorageApi.Controllers;

public class Document
{
    [JsonProperty("id")]
    []
    public string Id { get; set; }

    [JsonProperty("tags")]
    public IList<string> Tags { get; set; }

    [JsonProperty("data")]
    public JObject Data { get; set; }
}

public interface IDocumentStorage
{
    Task StoreDocumentAsync(Document document);

    Task<Document> GetDocumentAsync(string id);

    Task UpdateDocumentAsync(Document document);
}

public class InMemoryDocumentStorage : IDocumentStorage
{
    private readonly IDictionary<string, Document> _documents = new Dictionary<string, Document>();

    public Task<Document> GetDocumentAsync(string id)
    {
        return Task.FromResult(_documents.TryGetValue(id, out var document) ? document : null);
    }

    public Task StoreDocumentAsync(Document document)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task UpdateDocumentAsync(Document document)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }
}

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ILogger<DocumentsController> _logger;
    private readonly IDocumentStorageService _documentStorageService;

    public DocumentsController(ILogger<DocumentsController> logger, IDocumentStorageService documentStorageService)
    {
        _logger = logger;
        _documentStorageService = documentStorageService;
    }

    [HttpPost]
    public IActionResult Post([FromBody] JObject document)
    {
        if (document == null || !document.ContainsKey("id") || !document.ContainsKey("tags") || !document.ContainsKey("data"))
        {
            return BadRequest("Invalid document format");
        }

        try
        {
            _documentStorageService.StoreDocument(document);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing document");
            return StatusCode(500, "Error storing document");
        }
    }

    [HttpPut("{id}")]
    public IActionResult Put(string id, [FromBody] JObject document)
    {
        if (document == null || !document.ContainsKey("tags") || !document.ContainsKey("data"))
        {
            return BadRequest("Invalid document format");
        }

        try
        {
            _documentStorageService.UpdateDocument(id, document);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document");
            return StatusCode(500, "Error updating document");
        }
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        try
        {
            var document = _documentStorageService.GetDocument(id);
            if (document == null)
            {
                return NotFound();
            }

            var acceptHeader = Request.Headers[HeaderNames.Accept].FirstOrDefault();
            var serializer = GetSerializer(acceptHeader);

            if (serializer == null)
            {
                return BadRequest($"Unsupported media type: {acceptHeader}");
            }

            var stream = new MemoryStream();
            serializer.Serialize(stream, document);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, serializer.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document");
            return StatusCode(500, "Error retrieving document");
        }
    }

    private ISerializer GetSerializer(string mediaType)
    {
    }

}
