using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers;

namespace Tests.Core
{
    /// <summary>
    /// Unit tests for BaseApiController.HandleResult method.
    /// Validates correct HTTP status code and response handling.
    /// </summary>
    public class BaseApiControllerTest
    {
        private readonly TestableController _controller;
        private readonly Mock<IMediator> _mockMediator;

        public BaseApiControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new TestableController();

            // Mock HttpContext to provide IMediator
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

        /// <summary>
        /// Tests that HandleResult returns 500 when result is null.
        /// </summary>
        [Fact]
        public void HandleResult_NullResult_Returns500()
        {
            // Act
            var result = _controller.PublicHandleResult<string>(null!);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Unexpected null result.", statusCodeResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns appropriate error responses for different failure codes.
        /// </summary>
        [Theory]
        [InlineData(400, "BadRequest")]
        [InlineData(401, "Unauthorized")]
        [InlineData(403, "Forbidden")]
        [InlineData(404, "NotFound")]
        [InlineData(409, "Conflict")]
        [InlineData(500, "InternalServerError")]
        public void HandleResult_FailureWithErrorCode_ReturnsCorrectStatusCode(int errorCode, string errorMessage)
        {
            // Arrange
            var failureResult = Result<string>.Failure(errorMessage, errorCode);

            // Act
            var result = _controller.PublicHandleResult(failureResult);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.NotNull(statusCodeResult);
            Assert.Equal(errorCode, statusCodeResult.StatusCode);
            Assert.Equal(errorMessage, statusCodeResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns BadRequest for 400 error code.
        /// </summary>
        [Fact]
        public void HandleResult_Failure400_ReturnsBadRequest()
        {
            // Arrange
            var failureResult = Result<string>.Failure("Invalid input", 400);

            // Act
            var result = _controller.PublicHandleResult(failureResult);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid input", badRequestResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns Unauthorized for 401 error code.
        /// </summary>
        [Fact]
        public void HandleResult_Failure401_ReturnsUnauthorized()
        {
            // Arrange
            var failureResult = Result<string>.Failure("Unauthorized access", 401);

            // Act
            var result = _controller.PublicHandleResult(failureResult);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Unauthorized access", unauthorizedResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns NotFound for 404 error code.
        /// </summary>
        [Fact]
        public void HandleResult_Failure404_ReturnsNotFound()
        {
            // Arrange
            var failureResult = Result<string>.Failure("Resource not found", 404);

            // Act
            var result = _controller.PublicHandleResult(failureResult);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Resource not found", notFoundResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns Conflict for 409 error code.
        /// </summary>
        [Fact]
        public void HandleResult_Failure409_ReturnsConflict()
        {
            // Arrange
            var failureResult = Result<string>.Failure("Conflict detected", 409);

            // Act
            var result = _controller.PublicHandleResult(failureResult);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Conflict detected", conflictResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns 500 with error message for internal server error.
        /// </summary>
        [Fact]
        public void HandleResult_Failure500_ReturnsInternalServerError()
        {
            // Arrange
            var failureResult = Result<string>.Failure("Internal error", 500);

            // Act
            var result = _controller.PublicHandleResult(failureResult);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal error", statusCodeResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns default error message for 500 when error is null.
        /// </summary>
        [Fact]
        public void HandleResult_Failure500WithNullError_ReturnsDefaultMessage()
        {
            // Arrange
            var failureResult = Result<string>.Failure(null, 500);

            // Act
            var result = _controller.PublicHandleResult(failureResult);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal Server Error", statusCodeResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns correct status code for unexpected error codes.
        /// </summary>
        [Theory]
        [InlineData(418, "I'm a teapot")]
        [InlineData(503, "Service unavailable")]
        public void HandleResult_UnexpectedErrorCode_ReturnsCorrectStatusCode(int errorCode, string errorMessage)
        {
            // Arrange
            var failureResult = Result<string>.Failure(errorMessage, errorCode);

            // Act
            var result = _controller.PublicHandleResult(failureResult);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(errorCode, statusCodeResult.StatusCode);
            Assert.Equal(errorMessage, statusCodeResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns Ok for 200 success code.
        /// </summary>
        [Fact]
        public void HandleResult_Success200_ReturnsOk()
        {
            // Arrange
            var successResult = Result<string>.Success("Success value", 200);

            // Act
            var result = _controller.PublicHandleResult(successResult);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success value", okResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns CreatedAtAction for 201 with action name.
        /// </summary>
        [Fact]
        public void HandleResult_Success201WithActionName_ReturnsCreatedAtAction()
        {
            // Arrange
            var successResult = Result<string>.Success("Created value", 201);

            // Act
            var result = _controller.PublicHandleResult(successResult, "GetById", new { id = 1 });

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdResult.ActionName);
            Assert.Equal("Created value", createdResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns 201 status code without CreatedAtAction when action name is null.
        /// </summary>
        [Fact]
        public void HandleResult_Success201WithoutActionName_Returns201StatusCode()
        {
            // Arrange
            var successResult = Result<string>.Success("Created value", 201);

            // Act
            var result = _controller.PublicHandleResult(successResult);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
            Assert.Equal("Created value", statusCodeResult.Value);
        }

        /// <summary>
        /// Tests that HandleResult returns NoContent for 204 success code.
        /// </summary>
        [Fact]
        public void HandleResult_Success204_ReturnsNoContent()
        {
            // Arrange
            var successResult = Result<string>.Success(null, 204);

            // Act
            var result = _controller.PublicHandleResult(successResult);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Tests that HandleResult returns correct status code for custom success codes.
        /// </summary>
        [Theory]
        [InlineData(202, "Accepted")]
        [InlineData(206, "Partial content")]
        public void HandleResult_CustomSuccessCode_ReturnsCorrectStatusCode(int statusCode, string value)
        {
            // Arrange
            var successResult = Result<string>.Success(value, statusCode);

            // Act
            var result = _controller.PublicHandleResult(successResult);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(statusCode, statusCodeResult.StatusCode);
            Assert.Equal(value, statusCodeResult.Value);
        }

        // Helper class that extends BaseApiController to expose HandleResult
        private class TestableController : BaseApiController
        {
            public ActionResult PublicHandleResult<T>(Result<T> result, string? actionName = null, object? routeValues = null)
            {
                return HandleResult(result, actionName, routeValues);
            }
        }
    }
}