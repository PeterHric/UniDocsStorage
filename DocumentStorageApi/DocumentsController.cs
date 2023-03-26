using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

using DocumentStorageApi.Routing;
using DocumentStorage;
using Serializers;
using Newtonsoft.Json.Linq;

namespace DocumentStorageApi.Controllers;


[ApiController]
[Route(ApiRoutes.DocumentsControllerURL)]
public class DocumentsController : ControllerBase
{
    public DocumentsController(ILogger<DocumentsController> logger, IDocumentStorage documentStorage)
    {
        _logger = logger;
        _documentStorage = documentStorage;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] DocumentEntity document)
    {
        if (document == null)
        {
            return BadRequest("Received null document");
        }
        // However, field Data is attributed as [Required]
        else if (document.Data == null || !(JObject.FromObject(document.Data) is JObject dataObject) || dataObject.Count == 0)
        {
            return BadRequest("Document contains no 'Data'");
        }
        // ToDo: Check existence of entity

        try
        {
            await _documentStorage.StoreAsync(document);
            return Ok();
        }
        catch (Exception ex)
        {
            var errMessage = $"Failed to store the document: {ex.Message}";
            _logger.LogError(errMessage);
            return StatusCode(500, errMessage);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(string id, [FromBody] DocumentEntity document)
    {
        if (document == null)
        {
            return BadRequest("Received null document");
        }
        // However, field Data is attributed as [Required]
        else if (document.Data == null || !(JObject.FromObject(document.Data) is JObject dataObject) || dataObject.Count == 0)
        {
            return BadRequest("Document contains no 'Data'");
        }

        try
        {
            await _documentStorage.UpdateAsync(document);
            return Ok();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error updating document. Reason:{ex.Message}";
            _logger.LogError(errorMessage);
            return StatusCode(500, errorMessage);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(string id)
    {
        try
        {
            var document = await _documentStorage.GetAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var acceptHeader = Request.Headers[HeaderNames.Accept].FirstOrDefault();
            if (acceptHeader == null)
            {
                return BadRequest($"Missing, or wrong 'Accept' request header type/format.");
            }

            var serializer = ASerializer.GetSerializer(acceptHeader);
            if (serializer == null)
            {
                return BadRequest($"Unsupported media type: {acceptHeader}");
            }

            return Content(serializer.Serialize(document), serializer.ContentType);
            // Or use async variant:
            //return Content(await serializer.SerializeAsync(document), serializer.ContentType);
        }
        catch (Exception ex)
        {
            var errMessage = $"Error retrieving document. Reason: {ex.Message}";
            _logger.LogError(errMessage);
            return Problem($"Error retrieving document. Reason: {ex.Message}");
        }
    }

    private readonly ILogger<DocumentsController> _logger;

    private readonly IDocumentStorage _documentStorage;
}
