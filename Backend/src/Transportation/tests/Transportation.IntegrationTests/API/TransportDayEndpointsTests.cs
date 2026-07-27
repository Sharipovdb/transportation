using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Transportation.API.Models;
using Transportation.Application.TransportDay.Models;
using Transportation.Domain.Entities;
using Transportation.IntegrationTests.Common;

namespace Transportation.IntegrationTests.API;

public class TransportDayEndpointsTests : IntegrationTestBase
{
    public TransportDayEndpointsTests(CustomWebApplicationFactory factory) : base(factory) {}

    [Fact]
    public async Task CreateTransportDay()
    {
        // Arrange
        var transportDay = new TransportDay
        {
            CrewId = 1,
            Date = DateTime.Now,
            MorningMode = TransportMode.Driven,
            AfternoonMode = TransportMode.Taxi,
            ExtraCommuteKm = 1026,
            ExtraBusinessKm = 0,
            Notes = "Test",
            LoggedBy = 1,
            LoggedAt = DateTime.Now,
            Confirmed = true
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/TransportDays/Create", transportDay);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllTransportDays()
    {
        //arrange
        var transportDay = new TransportDay
        {
            CrewId = 1,
            Date = DateTime.Now,
            MorningMode = TransportMode.Driven,
            AfternoonMode = TransportMode.Taxi,
            ExtraCommuteKm = 1026,
            ExtraBusinessKm = 0,
            Notes = "Test",
            LoggedBy = 1,
            LoggedAt = DateTime.Now,
            Confirmed = true

        };
        var createResponse = await HttpClient.PostAsJsonAsync("/api/TransportDays/Create", transportDay);
        var createBody = await createResponse.Content.ReadAsStringAsync();
 
        createResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        //act
       var response = await HttpClient.GetAsync(
           "/api/TransportDays/GetAll?PaginationInfo.PageNumber=1&PaginationInfo.PageSize=50");

       var content = await response.Content.ReadAsStringAsync();
        //assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        //content.Should().Contain("Test");
    }

    [Fact]
    public async Task GetTransportDayById()
    {
        var response = await HttpClient.GetAsync(
            "/api/TransportDays/GetById/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        //когда создайте каждый  endpoints
        /*
        // Arrange
        var request = new CreateTransportDayCommand
        {
            CrewId = 1,
            Date = DateTime.Now,
            MorningMode = TransportMode.Driven,
            AfternoonMode = TransportMode.Taxi,
            DriverId = 1,
            CommuteKm = 1026,
            ExtraBusinesskm = 0,
            Notes = "Test"
        };

        var createResponse = await _httpClient.PostAsJsonAsync(
            "/api/TransportDaysCreate",
            request);

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var response = await _httpClient.GetAsync(
            "/api/TransportDaysGetAll?PaginationInfo.PageNumber=1&PaginationInfo.PageSize=50");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<TransportDayDto>>();

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().Contain(x => x.Notes == "Test");*/
    }
    [Fact]
    public async Task UpdateTransportDay()
    {
        // Arrange
        var createRequest = new TransportDay()
        {
            CrewId = 1,
            Date = DateTime.Now,
            MorningMode = TransportMode.Driven,
            AfternoonMode = TransportMode.Taxi,
            ExtraCommuteKm = 100,
            ExtraBusinessKm = 0,
            Notes = "Old Note"
        };

        var createResponse = await HttpClient.PostAsJsonAsync(
            "/api/TransportDaysCreate",
            createRequest);

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await createResponse.Content
            .ReadFromJsonAsync<TransportDayDto>();

        // Update
        var updateRequest = new UpdateTransportDayRequest
        {
            MorningMode = TransportMode.Taxi,
            AfternoonMode = TransportMode.Driven,
            DriverId = 2,
            CommuteKm = 150,
            ExtraBusinessKm = 20,
            Notes = "Updated Note"
        };

        // Act
        var updateResponse = await HttpClient.PutAsJsonAsync(
            $"/api/TransportDaysUpdate/{created!.Id}",
            updateRequest);

        // Assert Update
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert GetById
        var getResponse = await HttpClient.GetAsync(
            $"/api/TransportDaysGetById/{created.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await getResponse.Content
            .ReadFromJsonAsync<TransportDayDto>();

        updated.Should().NotBeNull();
        updated!.Notes.Should().Be("Updated Note");
        updated.ExtraBusinessKm.Should().Be(20);
        updated.DriverId.Should().Be(2);
        updated.MorningMode.Should().Be(TransportMode.Taxi.ToString());
        updated.AfternoonMode.Should().Be(TransportMode.Driven.ToString());
    }
    
}