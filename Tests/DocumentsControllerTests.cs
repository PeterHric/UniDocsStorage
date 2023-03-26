using System.Linq;
using NUnit.Framework;
using DocumentStorage;
using Microsoft.Extensions.Logging;
using DocumentStorageApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;

[TestFixture]
public class DocumentsControllerTests
{
    private Mock<IDocumentStorage>? _documentStorageMock;
    private ILogger<DocumentsController>? _logger;
    private DocumentsController? _controller;

    [SetUp]
    public void Setup()
    {
        _documentStorageMock = new Mock<IDocumentStorage>();
        _logger = Mock.Of<ILogger<DocumentsController>>();

        _controller = new DocumentsController(_logger, _documentStorageMock.Object);
    }

    [Test]
    public async Task PostAsync_WithValidDocument_ReturnsOkResult()
    {
        // Arrange
        var document = new DocumentEntity();
        document.Data["name"] = "Ken";
        document.Data["surname"] = "Mattel";

        // Act
        var result = await _controller!.PostAsync(document);

        // Assert
        Assert.IsInstanceOf<OkResult>(result);
        _documentStorageMock!.Verify(x => x.StoreAsync(document), Times.Once);
    }

    [Test]
    public async Task PostAsync_WithNullDocument_ReturnsBadRequestResult()
    {
        // Arrange
        DocumentEntity? document = null;

        // Act
        var result = await _controller!.PostAsync(document);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual("Received null document", badRequestResult.Value);
        _documentStorageMock!.Verify(x => x.StoreAsync(document), Times.Never);
    }

    [Test]
    public async Task PostAsync_WithDocumentWithoutData_ReturnsBadRequestResult()
    {
        // Arrange
        var document = new DocumentEntity();

        // Act
        var result = await _controller!.PostAsync(document);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual("Document contains no 'Data'", badRequestResult.Value);
        _documentStorageMock!.Verify(x => x.StoreAsync(document), Times.Never);
    }

    [Test]
    public async Task PutAsync_WithValidDocument_ReturnsOkResult()
    {
        // Arrange
        var id = "123";
        var document = new DocumentEntity();
        document.Data["name"] = "Barbie";
        document.Data["surname"] = "Mattel";

        // Act
        var result = await _controller!.PutAsync(id, document);

        // Assert
        Assert.IsInstanceOf<OkResult>(result);
        _documentStorageMock!.Verify(x => x.UpdateAsync(document), Times.Once);
    }

    [Test]
    public async Task PutAsync_WithNullDocument_ReturnsBadRequestResult()
    {
        // Arrange
        var id = "123";
        DocumentEntity? document = null;

        // Act
        var result = await _controller!.PutAsync(id, document);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual("Received null document", badRequestResult.Value);
        _documentStorageMock!.Verify(x => x.UpdateAsync(document), Times.Never);
    }

    [Test]
    public async Task PutAsync_WithDocumentWithoutData_ReturnsBadRequestResult()
    {
        // Arrange
        var id = "123";
        var document = new DocumentEntity(); // { Data = new Dictionary<string, string>() };

        // Act
        var result = await _controller!.PutAsync(id, document);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.AreEqual("Document contains no 'Data'", badRequestResult.Value);
        _documentStorageMock!.Verify(x => x.UpdateAsync(document), Times.Never);
    }

    [Test]
    public async Task GetAsync_ReturnsDocument_WithSupportedFormat()
    {
        // Arrange
        var expectedDocument = new DocumentEntity { Id = "1" };
        expectedDocument.Tags.Add("tag1");
        expectedDocument.Data["element1"] = "Test Document";

        var mockStorage = new Mock<IDocumentStorage>();
        mockStorage.Setup(x => x.GetAsync("1")).ReturnsAsync(expectedDocument);
        var controller = new DocumentsController(Mock.Of<ILogger<DocumentsController>>(), mockStorage.Object);

        var acceptHeader = "application/json"; // Currently supported format

        // Act
        var result = await controller.GetAsync("1");
        var contentResult = result as ContentResult;

        // Assert
        Assert.IsNotNull(contentResult);
        Assert.AreEqual(200, contentResult!.StatusCode);
        Assert.AreEqual(acceptHeader, contentResult.ContentType);

        var deserializedDocument = JsonConvert.DeserializeObject<DocumentEntity>(contentResult.Content);
        Assert.AreEqual(expectedDocument.Id, deserializedDocument.Id);
        Assert.AreEqual(expectedDocument.Tags.First(), deserializedDocument.Tags.First());
        Assert.AreEqual(expectedDocument.Data["element1"], deserializedDocument.Data["element1"]);
    }
}