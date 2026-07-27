using System.Net;

namespace Transportation.IntegrationTests.API;

public class MonthlyTransportSheetsControllerTests
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7248")
    };
    
    [Fact]
    public async Task GetById_WhenSheetDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        const long nonExistingId = 2;

        // Act
        var response = await _httpClient.GetAsync($"api/MonthlyTransportSheetsGetById/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}