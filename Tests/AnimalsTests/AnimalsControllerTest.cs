using Application.Animals.Commands;
using Application.Core;
using Application.Images.Commands;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;
using WebAPI.DTOs.Animals;
using WebAPI.DTOs.Images;

namespace Tests.AnimalsTests;

/// <summary>
/// Unit tests for AnimalsController - AddImagesToAnimal endpoint only.
/// </summary>
public class AnimalsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUserAccessor> _mockUserAccessor;
    private readonly AnimalsController _controller;

    public AnimalsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockMapper = new Mock<IMapper>();
        _mockUserAccessor = new Mock<IUserAccessor>();
        _controller = new AnimalsController(_mockMapper.Object, _mockUserAccessor.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mockMediator.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock.Object
            }
        };
    }
}