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
        // However, field Data is [Required]
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
        // However, field Data is [Required]
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

            var bytes = await serializer.SerializeAsync(document);
            if (serializer.ContentType == "application/x-msgpack")
            {
                return File(bytes, serializer.ContentType, $"document_{document.Id}.msgpack");
            }

            using var stream = new MemoryStream(bytes);
            //return new FileStreamResult(stream, serializer.ContentType);
            return Content(serializer.Serialize(document), serializer.ContentType);

            // Can use in response
            /*
            using var stream1 = new MemoryStream();
            serializer.Serialize(document);
            stream1.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream1, serializer.ContentType);
            */
        }
        catch (Exception ex)
        {
            var errMessage = $"Error retrieving document. Reason: {ex.Message}";
            _logger.LogError(errMessage);
            return Problem($"Error retrieving document. Reason: {ex.Message}");
        }
    }

    /*
        static public DocumentEntity? FromJsonObject(JObject jObjDocument)
        {
            // Check that the "id" and "tags" properties exist in the JSON object
            if (!jObjDocument.TryGetValue("id", StringComparison.OrdinalIgnoreCase, out JToken idToken) ||
                !jObjDocument.TryGetValue("tags", StringComparison.OrdinalIgnoreCase, out JToken tagsToken))
            {
                return null;
            }

            // Parse the "id" property into a Guid
            if (!Guid.TryParse(idToken.ToString(), out Guid id))
            {
                return null;
            }

            // Parse the "tags" property into a list of strings
            if (!tagsToken.HasValues)
            {
                return null;
            }

            List<string> tags = new List<string>();
            foreach (JToken tagToken in tagsToken)
            {
                if (tagToken.Type == JTokenType.String)
                {
                    tags.Add(tagToken.ToString());
                }
            }

            // Create the DocumentEntity object
            DocumentEntity document = new DocumentEntity()
            {
                Id = id,
                Tags = tags,
                Data = jObjDocument.ContainsKey("data") ? (JObject)jObjDocument["data"] : null
            };

            return document;
        }

        public DocumentEntity? FromJsonObject(JObject jObjDocument)
        {
            //var jsonParsedMsg = JObject.Parse(encodedMessage);
            //case ScadaMessageType.SERVICE_UNIT_STATISTIC:
            //parsedObject = jsonParsedMsg.GetValue("Payload").ToObject<ServiceUnitStatistic>();
            DocumentEntity retVal = null;
            try
            {
                if (!jObjDocument.TryGetValue("Id", StringComparison.OrdinalIgnoreCase, out JToken result))
                    return null;

                var retVal = new DocumentEntity()
                {
                    Id = result.SelectToken("Id").ToObject<Guid>(),
                    //Tags = result.SelectToken("Tags").ToObject<IList<string>>(),
                    Tags = result.SelectToken("Tags").ToObject<List<string>>(),
                    Data = result.SelectToken("Data").ToObject<JObject>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create DocumentEntity from JsonObject. Reason: {ex.Message}");
            }

            return retVal;
        }
    */

    private readonly ILogger<DocumentsController> _logger;

    private readonly IDocumentStorage _documentStorage;
}
