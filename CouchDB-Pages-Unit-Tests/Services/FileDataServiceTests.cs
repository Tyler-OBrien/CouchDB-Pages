using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using CouchDBPages.Server.Brokers;
using CouchDBPages.Server.Models.Data;
using CouchDBPages.Server.Models.Responses;
using CouchDBPages.Server.Services;
using CouchDBPages.Shared.API;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace CouchDBPages.UnitTests.Services;

public class FileDataServiceTests
{
    [Test]
    public async Task GetFile()
    {
        const string HOSTNAME = "localhost";
        const string CONTENT_TYPE = "text/html";
        const long CONTENT_LENGTH = 7;
        const string ETAG = "\"4949g9h0g\"";
        const string CouchDB_Body_Time = "0";
        // Necessary Services
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();

        // Mock Response
        var byteArray = new byte[] { 7, 8, 9 };
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(byteArray) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(CONTENT_TYPE);
        response.Content.Headers.ContentLength = CONTENT_LENGTH;
        response.Headers.ETag = new EntityTagHeaderValue(ETAG);
        response.Headers.Add("X-CouchDB-Body-Time", CouchDB_Body_Time);

        apiBrokerMock.Setup(apiBroker => apiBroker.GetFileAttachment(HOSTNAME, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);


        var fileDataService = new FileDataService(apiBrokerMock.Object);

        // act
        var result = await fileDataService.GetFile(HOSTNAME, It.IsAny<CancellationToken>());
        // assert
        result.Should().NotBeNull("We should get back a valid response");
        result.ContentType.Should().Be(CONTENT_TYPE, "Content Type should be forwarded");
        result.ContentLength.Should().Be(CONTENT_LENGTH.ToString(), "Content Length should be forwarded");
        result.ETag.Should().Be(ETAG, "ETag should be forwarded");
        result.X_CouchDB_Body_Time.Should().Be(CouchDB_Body_Time, "CouchDB time should be forwarded");


        var resultBuffer = new byte[result.FileStream.Length];
        var resultLength = await result.FileStream.ReadAsync(resultBuffer);

        byteArray.Length.Should().Be(resultLength, "Stream length should be the same");
        resultBuffer.Should().BeEquivalentTo(byteArray, "The stream data should be passed through");
    }

    [Test]
    public async Task RegressionTest()
    {
        const string HOSTNAME = "localhost";
        const string CONTENT_TYPE = "text/html";
        const long CONTENT_LENGTH = 7;
        const string ETAG = "\"4949g9h0g\"";
        const string CouchDB_Body_Time = "0";
        // Necessary Services
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();

        // Mock Response
        var byteArray = new byte[] { 7, 8, 9 };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new ByteArrayContent(new byte[] { 7, 8, 10 }) };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/svg");
        response.Content.Headers.ContentLength = 10;
        response.Headers.ETag = new EntityTagHeaderValue(ETAG);
        response.Headers.Add("X-CouchDB-Body-Time", "1" + CouchDB_Body_Time);

        apiBrokerMock.Setup(apiBroker => apiBroker.GetFileAttachment(HOSTNAME, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);


        var fileDataService = new FileDataService(apiBrokerMock.Object);

        // act
        var result = await fileDataService.GetFile(HOSTNAME, It.IsAny<CancellationToken>());
        // assert
        result.Should().NotBeNull("We should get back a valid response");
        result.ContentType.Should().NotBe(CONTENT_TYPE, "Content Type should be forwarded");
        result.ContentLength.Should().NotBe(CONTENT_LENGTH.ToString(), "Content Length should be forwarded");
        result.ETag.Should().Be(ETAG, "ETag should be forwarded");
        result.X_CouchDB_Body_Time.Should().NotBe(CouchDB_Body_Time, "CouchDB time should be forwarded");


        var resultBuffer = new byte[result.FileStream.Length];
        var resultLength = await result.FileStream.ReadAsync(resultBuffer);

        byteArray.Length.Should().Be(resultLength, "Stream length should be the same");
        resultBuffer.Should().NotBeEquivalentTo(byteArray, "The stream data should be different, passed through");
    }


    [Test]
    public async Task PutFile()
    {
        // Necessary Services
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();

        // Mock Response
        var byteArray = new byte[] { 7, 8, 9 };

        using var sha256 = SHA256.Create();

        var base64EncodedString = Convert.ToBase64String(byteArray);

        var fileHash = BitConverter.ToString(await sha256.ComputeHashAsync(new MemoryStream(byteArray)))
            .Replace("-", "")
            .ToLowerInvariant();


        var UploadFile = new UploadFileData(fileHash, "index.html", base64EncodedString);

        var couchResponse = new CouchResponse { Id = "ghoigohfgdigdf", Ok = true, Rev = "1-495u490tjgg" };

        apiBrokerMock.Setup(apiBroker => apiBroker.PutFileAsync(It.IsAny<PagesFile>())).ReturnsAsync(
            new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(JsonSerializer.Serialize(couchResponse)) });

        apiBrokerMock
            .Setup(apiBroker => apiBroker.PutFileAttachmentAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        var fileDataService = new FileDataService(apiBrokerMock.Object);

        // act
        var result = await fileDataService.PutFile(UploadFile);
        // assert
        result.Should().NotBeNull("We should get back a valid response");
        result.StatusCode.Should().Be(HttpStatusCode.Accepted, "The file should be accepted");
    }

    [Test]
    public async Task PutFileConflict()
    {
        // Necessary Services
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();

        // Mock Response
        var byteArray = new byte[] { 7, 8, 9 };

        using var sha256 = SHA256.Create();

        var base64EncodedString = Convert.ToBase64String(byteArray);

        var fileHash = BitConverter.ToString(await sha256.ComputeHashAsync(new MemoryStream(byteArray)))
            .Replace("-", "")
            .ToLowerInvariant();


        var UploadFile = new UploadFileData(fileHash, "index.html", base64EncodedString);


        apiBrokerMock.Setup(apiBroker => apiBroker.PutFileAsync(It.IsAny<PagesFile>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict));


        var fileDataService = new FileDataService(apiBrokerMock.Object);

        // act
        var result = await fileDataService.PutFile(UploadFile);
        // assert
        result.Should().NotBeNull("We should get back a valid response");
        result.StatusCode.Should()
            .Be(HttpStatusCode.OK, "We should get back OK, indictating the file was already there..");
    }
}