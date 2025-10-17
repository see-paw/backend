using Xunit;
using API.Services;

namespace Tests;

public class MathServiceTests
{
    private readonly MathService _mathService;

    public MathServiceTests()
    {
        _mathService = new MathService();
    }

    [Fact]
    public void Add_ShouldReturnCorrectSum()
    {
        // Arrange
        int a = 2, b = 3;

        // Act
        int result = _mathService.Add(a, b);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void Multiply_ShouldReturnCorrectProduct()
    {
        // Arrange
        int a = 4, b = 5;

        // Act
        int result = _mathService.Multiply(a, b);

        // Assert
        Assert.Equal(20, result);
    }
}
