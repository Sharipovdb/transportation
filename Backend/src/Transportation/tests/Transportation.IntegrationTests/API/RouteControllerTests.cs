using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Transportation.Application.Route.Commands;
using Transportation.Application.Route.Models;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence;
using Transportation.IntegrationTests.Common;
using Transportation.Mediator.Helper.Common.Models;
using Xunit.Abstractions;

namespace Transportation.IntegrationTests.API;

public class RouteControllerTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomWebApplicationFactory _factory;
    public RouteControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task CreateRoute_ShouldSaveToDatabase_WhenDataIsValid()
    {
        // Arrange 

        var request = new CreateRouteCommand(
            "Chashma", 
            20);

        // Act 

        var response = await HttpClient.PostAsJsonAsync("/api/Route/Add", request);

        // Assert

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task GetById_ShouldReturnRoute_WhenRouteExists()
    {
        // Arrange
        await ResetDatabaseAsync();

        var route = new Route
        {
            Name = "Vali",
            DistanceKm = 40
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();

            db.Routes.Add(route);
            await db.SaveChangesAsync();
        }

        _testOutputHelper.WriteLine(route.Id.ToString());
        route.Id.Should().BeGreaterThan(0);
        
        // Act
        var response = await HttpClient.GetAsync($"/api/Route/GetById/{route.Id}");

        // Assert
        
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<RouteDto>();

        result.Should().NotBeNull();
        result.Name.Should().Be(route.Name);
        result.DistanceKm.Should().Be(route.DistanceKm);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllRoutes_WhenRoutesExist()
    {
        // Arrange
        await ResetDatabaseAsync();

        var route = new Route
        {
            Name = "Vali",
            DistanceKm = 40
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();

            db.Routes.Add(route);
            await db.SaveChangesAsync();
        }

        _testOutputHelper.WriteLine(route.Id.ToString());
        route.Id.Should().BeGreaterThan(0);
        
        // Act
        
        var response = await HttpClient.GetAsync(
            "/api/Route/GetAll?PaginationInfo.Index=0&PaginationInfo.Size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<RouteDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ShouldUpdateRoute_WhenRouteExist()
    {
        // Arrange
        
        await ResetDatabaseAsync();
        
        var route = new Route
        {
            Name = "Vali",
            DistanceKm = 40
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
            db.Routes.Add(route);
            await db.SaveChangesAsync();
        }
        
        var request = new UpdateRouteCommand(
            1, 
            "Valijon",
            90);
        
        // Act
        
        var response = await HttpClient.PutAsJsonAsync("/api/Route/Update", request);
        
        // Assert
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldDeleteRoute_WhenRouteExists()
    {
        // Arrange
        await ResetDatabaseAsync();

        var route = new Route
        {
            Name = "Vali",
            DistanceKm = 40
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
            db.Routes.Add(route);
            await db.SaveChangesAsync();
        }

        // Act 
        
        var response = await HttpClient.DeleteAsync($"/api/Route/Delete/1");
        
        // Assert
        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
    }
}