using CouchDBPages.Server.Models.Data;
using CouchDBPages.Server.Models.Responses;
using CouchDBPages.Server.Models.Services;
using CouchDBPages.Server.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CouchDBPages.UnitTests.Services;

public class FileServiceTests
{
    [Test]
    public async Task GetFile()
    {
        const string HOSTNAME = "localhost";
        const string PATH = "index.html";

        const string GITHASH = "gfrihfogdfgih";

        const string FILEHASH = "gigfdhgofhdgf";
        // Necessary Services
        // Mock Service
        var fileDataManifestService = new Mock<IFileDataManifestService>();
        var fileDataService = new Mock<IFileDataService>();
        var ILogger = new Mock<ILogger<FileService>>();

        var manifest = new PagesFileManifest(GITHASH, HOSTNAME,
            new Dictionary<string, string> { { PATH, FILEHASH } }, false);
        // Mock Response
        fileDataManifestService.Setup(service => service.GetMetadata(HOSTNAME, It.IsAny<CancellationToken>())).ReturnsAsync(manifest);


        var byteArray = new byte[] { 9, 4, 3, 2 };
        var Stream = new MemoryStream(byteArray);
        var response = new FileDataResponse(Stream, "text/html", "900",
            "\"p5krX2MyR6i3cIPjDZ6rRg==\"", "0");

        fileDataService.Setup(service => service.GetFile(FILEHASH, It.IsAny<CancellationToken>())).ReturnsAsync(response);


        var fileService = new FileService(fileDataManifestService.Object, fileDataService.Object, ILogger.Object);

        // act
        var result = await fileService.GetFile(HOSTNAME, PATH, It.IsAny<CancellationToken>());
        // assert
        result.Should().NotBeNull("We should get back a valid response");

        // assert
        result.Should().NotBeNull("We should get back a valid response");
        result.ContentType.Should().Be(response.ContentType, "Content Type should be forwarded");
        result.ContentLength.Should().Be(response.ContentLength, "Content Length should be forwarded");
        result.ETag.Should().Be(response.ETag, "ETag should be forwarded");
        result.X_CouchDB_Body_Time.Should().Be(response.X_CouchDB_Body_Time, "CouchDB time should be forwarded");


        var resultBuffer = new byte[result.FileStream.Length];
        var resultLength = await result.FileStream.ReadAsync(resultBuffer);

        byteArray.Length.Should().Be(resultLength, "Stream length should be the same");
        resultBuffer.Should().BeEquivalentTo(byteArray, "The stream data should be different, passed through");
    }
}