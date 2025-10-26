/*using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using WebAPI.Core;
using WebAPI.Middleware;
using Xunit;

namespace Tests
{
    /// <summary>
    /// Unit tests for ExceptionMiddleware.
    /// Validates exception handling for validation errors and general exceptions.
    /// </summary>
    public class ExceptionMiddlewareTest
    {
        private readonly Mock<ILogger<ExceptionMiddleware>> _mockLogger;
        private readonly Mock<IHostEnvironment> _mockEnv;
        private readonly ExceptionMiddleware _middleware;

        public ExceptionMiddlewareTest()
        {
            _mockLogger = new Mock<ILogger<ExceptionMiddleware>>();
            _mockEnv = new Mock<IHostEnvironment>();
            _middleware = new ExceptionMiddleware(_mockLogger.Object, _mockEnv.Object);
        }

        /// <summary>
        /// Tests that middleware calls next delegate when no exception occurs.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_NoException_CallsNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var nextCalled = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            Assert.True(nextCalled);
        }

        /// <summary>
        /// Tests that middleware handles ValidationException and returns 400.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_ValidationException_Returns400()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Id", "Id must be a valid GUID.")
            };
            var validationException = new ValidationException(validationFailures);

            RequestDelegate next = (HttpContext ctx) => throw validationException;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            Assert.Equal(400, context.Response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        }

        /// <summary>
        /// Tests that middleware returns validation errors in correct format.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_ValidationException_ReturnsValidationProblemDetails()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required."),
                new ValidationFailure("Email", "Email must be valid.")
            };
            var validationException = new ValidationException(validationFailures);

            RequestDelegate next = (HttpContext ctx) => throw validationException;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

            Assert.Contains("Name", responseBody);
            Assert.Contains("Name is required.", responseBody);
            Assert.Contains("Email", responseBody);
            Assert.Contains("Email must be valid.", responseBody);
            Assert.Contains("Validation error", responseBody);
        }

        /// <summary>
        /// Tests that middleware handles multiple errors for same property.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_ValidationExceptionMultipleErrorsSameProperty_ReturnsAllErrors()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Password", "Password is required."),
                new ValidationFailure("Password", "Password must be at least 8 characters.")
            };
            var validationException = new ValidationException(validationFailures);

            RequestDelegate next = (HttpContext ctx) => throw validationException;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

            Assert.Contains("Password is required.", responseBody);
            Assert.Contains("Password must be at least 8 characters.", responseBody);
        }

        /// <summary>
        /// Tests that middleware handles general exceptions and returns 500.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_GeneralException_Returns500()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

            var exception = new Exception("Unexpected error");
            RequestDelegate next = (HttpContext ctx) => throw exception;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);
        }

        /// <summary>
        /// Tests that middleware logs general exceptions.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_GeneralException_LogsError()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

            var exception = new Exception("Test error");
            RequestDelegate next = (HttpContext ctx) => throw exception;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// Tests that middleware includes stack trace in development environment.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_GeneralExceptionInDevelopment_IncludesStackTrace()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

            var exception = new Exception("Development error");
            RequestDelegate next = (HttpContext ctx) => throw exception;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<AppException>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(response);
            Assert.Equal(500, response.StatusCode);
            Assert.Equal("Development error", response.Message);
            Assert.NotNull(response.Details);
        }

        /// <summary>
        /// Tests that middleware excludes stack trace in production environment.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_GeneralExceptionInProduction_ExcludesStackTrace()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

            var exception = new Exception("Production error");
            RequestDelegate next = (HttpContext ctx) => throw exception;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<AppException>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(response);
            Assert.Equal(500, response.StatusCode);
            Assert.Equal("Production error", response.Message);
            Assert.Null(response.Details);
        }

        /// <summary>
        /// Tests that middleware returns correct JSON structure for general exceptions.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_GeneralException_ReturnsCorrectJsonStructure()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

            var exception = new Exception("Error message");
            RequestDelegate next = (HttpContext ctx) => throw exception;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

            Assert.Contains("statusCode", responseBody);
            Assert.Contains("message", responseBody);
            Assert.Contains("Error message", responseBody);
        }

        /// <summary>
        /// Tests that middleware handles ValidationException with no errors gracefully.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_ValidationExceptionWithNoErrors_Returns400()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var validationException = new ValidationException(new List<ValidationFailure>());
            RequestDelegate next = (HttpContext ctx) => throw validationException;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            Assert.Equal(400, context.Response.StatusCode);
        }

        /// <summary>
        /// Tests different exception types are handled as general exceptions.
        /// </summary>
        [Theory]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(NullReferenceException))]
        public async Task InvokeAsync_DifferentExceptionTypes_Returns500(Type exceptionType)
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Production");

            var exception = (Exception)Activator.CreateInstance(exceptionType, "Test error")!;
            RequestDelegate next = (HttpContext ctx) => throw exception;

            // Act
            await _middleware.InvokeAsync(context, next);

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
        }
    }
}*/