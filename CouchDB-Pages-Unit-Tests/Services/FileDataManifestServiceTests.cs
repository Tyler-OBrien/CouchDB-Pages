using System.Net;
using CouchDBPages.Server.Brokers;
using CouchDBPages.Server.Models.Data;
using CouchDBPages.Server.Services;
using CouchDBPages.Shared.API;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;

namespace CouchDBPages.UnitTests.Services;

public class FileDataManifestServiceTests
{
    public HttpContext CreateDefaultHttpContext()
    {
        return new DefaultHttpContext { Request = { Host = new HostString("localhost"), Path = new PathString("/") } };
    }

    [Test]
    public async Task GetMetadataTest()
    {
        const string HOSTNAME = "localhost";
        // Necessary Services
        var memCache = new MemoryCache(new MemoryCacheOptions());
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();

        // Mock Response
        var response = new PagesFileManifest("54389gh89gfdfdgfdg", HOSTNAME,
            new Dictionary<string, string> { { "index.html", "88gg78t8gfd897" } }, false);

        apiBrokerMock.Setup(apiBroker => apiBroker.FindManifestAsync(HOSTNAME, It.IsAny<CancellationToken>())).ReturnsAsync(response);


        var fileDataManifestService =
            new FileDataManifestService(memCache, apiBrokerMock.Object);

        // act
        var result = await fileDataManifestService.GetMetadata(HOSTNAME);
        // assert
        result.Should().BeSameAs(response, "The metadata returned by the API Broker should be passed through.");
    }

    [Test]
    public async Task MetadataDoesNotExist()
    {
        const string HOSTNAME = "localhost";
        // Necessary Services
        var memCache = new MemoryCache(new MemoryCacheOptions());
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();

        // Mock Response
        var response = new PagesFileManifest("54389gh89gfdfdgfdg", HOSTNAME,
            new Dictionary<string, string> { { "index.html", "88gg78t8gfd897" } }, false);

        apiBrokerMock.Setup(apiBroker => apiBroker.FindManifestAsync(HOSTNAME, It.IsAny<CancellationToken>())).ReturnsAsync(response);


        var fileDataManifestService =
            new FileDataManifestService(memCache, apiBrokerMock.Object);

        // act
        var result = await fileDataManifestService.GetMetadata("Something-Else");
        // assert
        result.Should().BeNull("If we send a request for metadata that does not exist, we should get a null response");
    }

    [Test]
    public async Task UploadMetadataConflict()
    {
        const string HOSTNAME = "localhost";
        // Necessary Services
        var memCache = new MemoryCache(new MemoryCacheOptions());
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();
        // Mock Response

        var uploadManifest = new UploadFileManifest("54389gh89gfdfdgfdg", HOSTNAME,
            new Dictionary<string, string> { { "index.html", "88gg78t8gfd897" } }, false);
        var newManifest = new PagesFileManifest(uploadManifest);
        newManifest.Hostname = $"{newManifest.GitHash}.{newManifest.Hostname}";
        newManifest.ID = $"{newManifest.GitHash}.{newManifest.Hostname}";

        apiBrokerMock.Setup(apiBroker => apiBroker.PutManifestAsync(It.IsAny<PagesFileManifest>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict));


        var fileDataManifestService =
            new FileDataManifestService(memCache, apiBrokerMock.Object);


        // act
        var result = await fileDataManifestService.PutMetadata(uploadManifest);
        // assert
        result.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "If the api broker responds back with conflict, the entire upload process should stop.");
    }

    [Test]
    // Ensure that previews are not uploaded to index (as apex hostname)
    public async Task UploadMetadatePreviewOnly()
    {
        const string HOSTNAME = "localhost";
        // Necessary Services
        var memCache = new MemoryCache(new MemoryCacheOptions());
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();

        // Mock Response

        var uploadManifest = new UploadFileManifest("54389gh89gfdfdgfdg", HOSTNAME,
            new Dictionary<string, string> { { "index.html", "88gg78t8gfd897" } }, true);


        apiBrokerMock.Setup(apiBroker => apiBroker.PutManifestAsync(It.IsAny<PagesFileManifest>()))
            .Callback<PagesFileManifest>(manifest =>
            {
                // First manifest is the git sha, second if preview is disabled, should be index
                Assert.AreNotEqual(manifest.ID, uploadManifest.Hostname);
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));


        var fileDataManifestService =
            new FileDataManifestService(memCache, apiBrokerMock.Object);


        // act
        var result = await fileDataManifestService.PutMetadata(uploadManifest);
        // assert
        result.StatusCode.Should()
            .Be(HttpStatusCode.Accepted, "If no conflicts occur, the response should be accepted.");
    }

    [Test]
    // This should upload first the git sha, then the index (apex hostname).
    // We are returning FindAsync as if index already exists, so we expect a revision valued returned
    public async Task UploadMetadata()
    {
        const string HOSTNAME = "localhost";
        // Necessary Services
        var memCache = new MemoryCache(new MemoryCacheOptions());
        // Mock Service
        var apiBrokerMock = new Mock<IAPIBroker>();
        // Mock Response

        var uploadManifest = new UploadFileManifest("54389gh89gfdfdgfdg", HOSTNAME,
            new Dictionary<string, string> { { "index.html", "88gg78t8gfd897" } }, false);
        var newManifest = new PagesFileManifest(uploadManifest)
            { Revision = "8-example", ID = "9g9gffg90hdfg90dfghgfd0" };

        var timesCalled = 0;

        // This should be cleaned up..
        apiBrokerMock.Setup(repo => repo.PutManifestAsync(It.IsAny<PagesFileManifest>()))
            .Callback<PagesFileManifest>(manifest =>
            {
                manifest.Hostname.Should().Match(hostname =>
                        hostname == HOSTNAME || hostname == $"{newManifest.GitHash}.{newManifest.Hostname}",
                    "The hostname should be the main deploy or the preview deploy, which we can get deterministically, as the deploy tool will also calculate it.");

                // We are returning FindAsync as if index already exists, so we expect a revision valued returned. This regressed a few times and caused issues with conflict responses from CouchDB.
                if (manifest.Hostname == HOSTNAME)
                    manifest.Revision.Should().NotBeNullOrWhiteSpace(
                        "If the main deploy already exists, it should contain the revision number to prevent conflicts");

                timesCalled++;
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));


        apiBrokerMock.Setup(repo => repo.FindManifestAsync(HOSTNAME, It.IsAny<CancellationToken>())).ReturnsAsync(newManifest);
        apiBrokerMock.Setup(apibroker => apibroker.PutFileAsync(It.IsAny<PagesFile>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));


        var fileDataManifestService =
            new FileDataManifestService(memCache, apiBrokerMock.Object);


        // act
        var result = await fileDataManifestService.PutMetadata(uploadManifest);
        // assert
        result.StatusCode.Should()
            .Be(HttpStatusCode.Accepted, "If no conflicts occur, the response should be accepted");

        timesCalled.Should().Be(2,
            "We should only have two calls, one for the preview deploy and one for the main deploy");
    }
}