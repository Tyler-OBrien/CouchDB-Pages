using System.Net;
using CouchDBPages.Server.Models.Responses;
using CouchDBPages.Server.Models.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace CouchDBPages.UnitTests.Controllers;

public class CouchDBController
{
    public HttpContext CreateDefaultHttpContext()
    {
        return new DefaultHttpContext { Request = { Host = new HostString("localhost"), Path = new PathString("/") } };
    }

    // Don't regress to erroring if path was empty
    public HttpContext CreateEmptyHttpContext()
    {
        return new DefaultHttpContext { Request = { Host = new HostString("localhost"), Path = new PathString("") } };
    }

    [Test]
    public async Task IndexTest()
    {
        // arrange
        var fileServiceMock = new Mock<IFileService>();
        var Stream = new MemoryStream(new byte[] { 9, 4, 3, 2 });
        var response = new FileDataResponse(Stream, "text/html", "900",
            "\"p5krX2MyR6i3cIPjDZ6rRg==\"", "0");
        fileServiceMock.Setup(fileservice => fileservice.GetFile("localhost", "")).ReturnsAsync(response);
        var homeController = new Server.Controllers.CouchDBController(fileServiceMock.Object);
        homeController.ControllerContext = new ControllerContext();
        homeController.ControllerContext.HttpContext = CreateDefaultHttpContext();
        // act
        var result = await homeController.GetFromDatabase();
        // assert
        ((FileStreamResult)result).FileStream.Should().BeSameAs(Stream,
            "The file data response's stream should be sent back to the client.");
    }

    [Test]
    public async Task IndexTest404()
    {
        // arrange
        var fileServiceMock = new Mock<IFileService>();
        FileDataResponse response = null;
        fileServiceMock.Setup(fileservice => fileservice.GetFile("localhost", "")).ReturnsAsync(response);
        var homeController = new Server.Controllers.CouchDBController(fileServiceMock.Object);
        homeController.ControllerContext = new ControllerContext();
        homeController.ControllerContext.HttpContext = CreateDefaultHttpContext();
        // act
        var result = await homeController.GetFromDatabase();
        // assert
        ((NotFoundResult)result).StatusCode.Should().Be((int)HttpStatusCode.NotFound,
            "If we sent back no file, the controller should return null.");
    }

    [Test]
    public async Task EmptyPathRegressionTest()
    {
        // arrange
        var fileServiceMock = new Mock<IFileService>();
        FileDataResponse response = null;
        fileServiceMock.Setup(fileservice => fileservice.GetFile("localhost", "")).ReturnsAsync(response);
        var homeController = new Server.Controllers.CouchDBController(fileServiceMock.Object);
        homeController.ControllerContext = new ControllerContext();
        homeController.ControllerContext.HttpContext = CreateEmptyHttpContext();
        // act
        var result = await homeController.GetFromDatabase();
        // assert
        ((NotFoundResult)result).StatusCode.Should()
            .Be((int)HttpStatusCode.NotFound, "If no path is sent, nothing should error out.");
    }
}