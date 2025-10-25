using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using WebAPI.Core;
using Xunit;

namespace Tests
{
    /// <summary>
    /// Unit tests for ValidationBehavior.
    /// Validates request validation logic in the MediatR pipeline.
    /// </summary>
    public class ValidationBehaviorTest
    {
        /// <summary>
        /// Tests that behavior calls next handler when no validator is provided.
        /// </summary>
        [Fact]
        public async Task Handle_NoValidator_CallsNextHandler()
        {
            // Arrange
            var behavior = new ValidationBehavior<TestRequest, TestResponse>();
            var request = new TestRequest { Value = "test" };
            var expectedResponse = new TestResponse { Result = "success" };
            var nextCalled = false;

            Task<TestResponse> Next(CancellationToken ct)
            {
                nextCalled = true;
                return Task.FromResult(expectedResponse);
            }

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal(expectedResponse, result);
        }

        /// <summary>
        /// Tests that behavior calls next handler when validation passes.
        /// </summary>
        [Fact]
        public async Task Handle_ValidRequest_CallsNextHandler()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var behavior = new ValidationBehavior<TestRequest, TestResponse>(mockValidator.Object);
            var request = new TestRequest { Value = "valid" };
            var expectedResponse = new TestResponse { Result = "success" };
            var nextCalled = false;

            Task<TestResponse> Next(CancellationToken ct)
            {
                nextCalled = true;
                return Task.FromResult(expectedResponse);
            }

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal(expectedResponse, result);
            mockValidator.Verify(v => v.ValidateAsync(request, CancellationToken.None), Times.Once);
        }

        /// <summary>
        /// Tests that behavior throws ValidationException when validation fails.
        /// </summary>
        [Fact]
        public async Task Handle_InvalidRequest_ThrowsValidationException()
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Value", "Value is required.")
            };
            var validationResult = new ValidationResult(validationFailures);

            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var behavior = new ValidationBehavior<TestRequest, TestResponse>(mockValidator.Object);
            var request = new TestRequest { Value = null };

            Task<TestResponse> Next(CancellationToken ct) => Task.FromResult(new TestResponse());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(request, Next, CancellationToken.None));

            Assert.Single(exception.Errors);
            Assert.Equal("Value", exception.Errors.First().PropertyName);
            Assert.Equal("Value is required.", exception.Errors.First().ErrorMessage);
        }

        /// <summary>
        /// Tests that behavior does not call next handler when validation fails.
        /// </summary>
        [Fact]
        public async Task Handle_InvalidRequest_DoesNotCallNextHandler()
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Value", "Invalid value.")
            };
            var validationResult = new ValidationResult(validationFailures);

            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var behavior = new ValidationBehavior<TestRequest, TestResponse>(mockValidator.Object);
            var request = new TestRequest { Value = "invalid" };
            var nextCalled = false;

            Task<TestResponse> Next(CancellationToken ct)
            {
                nextCalled = true;
                return Task.FromResult(new TestResponse());
            }

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(request, Next, CancellationToken.None));

            Assert.False(nextCalled);
        }

        /// <summary>
        /// Tests that behavior throws ValidationException with multiple errors.
        /// </summary>
        [Fact]
        public async Task Handle_MultipleValidationErrors_ThrowsValidationExceptionWithAllErrors()
        {
            // Arrange
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Value", "Value is required."),
                new ValidationFailure("Value", "Value must be at least 3 characters."),
                new ValidationFailure("Name", "Name is required.")
            };
            var validationResult = new ValidationResult(validationFailures);

            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var behavior = new ValidationBehavior<TestRequest, TestResponse>(mockValidator.Object);
            var request = new TestRequest { Value = "" };

            Task<TestResponse> Next(CancellationToken ct) => Task.FromResult(new TestResponse());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(request, Next, CancellationToken.None));

            Assert.Equal(3, exception.Errors.Count());
        }

        /// <summary>
        /// Tests that behavior respects cancellation token.
        /// </summary>
        /*[Fact]
        public async Task Handle_CancellationRequested_PassesCancellationToken()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestRequest>>();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), cancellationToken))
                .ReturnsAsync(new ValidationResult());

            var behavior = new ValidationBehavior<TestRequest, TestResponse>(mockValidator.Object);
            var request = new TestRequest { Value = "test" };

            Task<TestResponse> Next(CancellationToken ct) => Task.FromResult(new TestResponse());

            // Act
            await behavior.Handle(request, Next, cancellationToken);

            // Assert
            mockValidator.Verify(v => v.ValidateAsync(request, cancellationToken), Times.Once);
        }*/

        /// <summary>
        /// Tests that behavior calls validator exactly once.
        /// </summary>
        [Fact]
        public async Task Handle_ValidRequest_CallsValidatorOnce()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var behavior = new ValidationBehavior<TestRequest, TestResponse>(mockValidator.Object);
            var request = new TestRequest { Value = "test" };

            Task<TestResponse> Next(CancellationToken ct) => Task.FromResult(new TestResponse());

            // Act
            await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            mockValidator.Verify(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that behavior returns response from next handler.
        /// </summary>
        [Fact]
        public async Task Handle_ValidRequest_ReturnsResponseFromNextHandler()
        {
            // Arrange
            var mockValidator = new Mock<IValidator<TestRequest>>();
            mockValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var behavior = new ValidationBehavior<TestRequest, TestResponse>(mockValidator.Object);
            var request = new TestRequest { Value = "test" };
            var expectedResponse = new TestResponse { Result = "expected result" };

            Task<TestResponse> Next(CancellationToken ct) => Task.FromResult(expectedResponse);

            // Act
            var result = await behavior.Handle(request, Next, CancellationToken.None);

            // Assert
            Assert.Equal("expected result", result.Result);
        }
    }

    // Test request and response classes - PUBLIC for Moq compatibility
    public class TestRequest
    {
        public string? Value { get; set; }
    }

    public class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}