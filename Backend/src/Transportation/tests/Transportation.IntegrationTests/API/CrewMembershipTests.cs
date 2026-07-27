using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Transportation.API.Models;
using Transportation.Application.CrewMembership.Commands;
using Transportation.IntegrationTests.Common;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence;

namespace Transportation.IntegrationTests.API;

public class CrewMembershipTests : IntegrationTestBase
{
    protected readonly CustomWebApplicationFactory Factory;

    public CrewMembershipTests(CustomWebApplicationFactory factory) : base(factory) => Factory = factory;

    [Fact]
    public async Task GetAllCrewMemberships_ShouldReturnOk_WhenRequestIsValid()
    {
        var url = $"/api/CrewMembership/GetAll" +
                  $"?PaginationInfo.Index={0L}" +
                  $"&PaginationInfo.Size={1L}";

        var result = await HttpClient.GetAsync(url);

        if (result.StatusCode != HttpStatusCode.OK)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();

            result.StatusCode.Should().Be(HttpStatusCode.OK, $"сервер вернул ошибку: {errorDetails}");
        }

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByIdCrewMemberships_ShouldReturnOk_WhenRequestIsValid()
    {
        long generatedId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();

            var crewmembershipinsert = new CrewMembership
            {
                CrewId = 5L,
                UserId = 8L,
                ActiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            db.CrewMemberships.Add(crewmembershipinsert);
            await db.SaveChangesAsync();

            generatedId = crewmembershipinsert.Id;
        }

        var result = await HttpClient.GetAsync($"/api/CrewMembership/GetById/{generatedId}");

        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    //  [Fact]
    //  public async Task CreateCrewMembership_ShouldBeSavedToDatabase_WhenDataIsValid()
    //  {
    //      long generatedCrewId;
    //
    //      using (var scope = Factory.Services.CreateScope())
    //      {
    //          var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
    //
    //          var testCrew = new Crew 
    //          { 
    //              Name = "Test Crew Alpha", 
    //              SeatCapacity = 5 
    //          }; 
    //      
    //          db.Crews.Add(testCrew);
    //          await db.SaveChangesAsync();
    //      
    //          generatedCrewId = testCrew.Id;
    //      }
    //
    //      var createCrewMembershipCommand = new CreateCrewMembershipCommand(generatedCrewId, 2L);
    //      
    //      var result = await HttpClient.PostAsJsonAsync("/api/CrewMembership/Create", createCrewMembershipCommand);
    //  
    //      if (result.StatusCode != HttpStatusCode.OK)
    //      {
    //          var errorDetails = await result.Content.ReadAsStringAsync();
    //          throw new Exception($"Сервер вернул ошибку: {result.StatusCode}. Подробности: {errorDetails}");
    //      }
    //
    //      result.StatusCode.Should().Be(HttpStatusCode.OK);
    //  }

    [Fact]
    public async Task UpdateCrewMembership_ShouldBeSaveToDatabase_WhenDataIsValid()
    {
        long generatedId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();

            var crewmembershipinsert = new CrewMembership
            {
                CrewId = 4L,
                UserId = 2L,
                ActiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            db.Add(crewmembershipinsert);
            await db.SaveChangesAsync();

            generatedId = crewmembershipinsert.Id;
        }

        var crewmembershipRequest = new UpdateCrewMembershipRequest
        (
            ActiveFrom: DateTime.UtcNow,
            ActiveTo: null
        );

        var result =
            await HttpClient.PutAsJsonAsync($"/api/CrewMembership/Update/{generatedId}", crewmembershipRequest);

        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCrewMembership_ShouldReturnNoContent_WhenCrewExists()
    {
        long generatedId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();

            var membershipInsert = new CrewMembership
            {
                CrewId = 5L,
                UserId = 8L,
                ActiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            db.CrewMemberships.Add(membershipInsert);
            await db.SaveChangesAsync();

            generatedId = membershipInsert.Id;
        }

        var result = await HttpClient.DeleteAsync($"/api/CrewMembership/Delete/{generatedId}");

        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task TransferCrewMembership_ShouldReturnOk_WhenDataIsValid()
    {
        long oldCrewId;
        long newCrewId;
        long userId;

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();

            var oldcrewinsert = new Crew
            {
                Name = "Old Crew Alpha",
                SeatCapacity = 4
            };

            var newcrewinsert = new Crew
            {
                Name = "New Crew Alpha",
                SeatCapacity = 3
            };

            var empolyeeinsert = new User
            {
                UserName = "Azam",
                PhoneNumber = "11-777-02-01",
                TelegramId = "@Alex",
                FirstName = "Azam",
                LastName = "Bayzaev"
            };

            db.Crews.Add(oldcrewinsert);
            db.Crews.Add(newcrewinsert);
            db.Users.Add(empolyeeinsert);
            await db.SaveChangesAsync();

            oldCrewId = oldcrewinsert.Id;
            newCrewId = newcrewinsert.Id;
            userId = empolyeeinsert.Id;

            var oldMembership = new CrewMembership
            {
                CrewId = oldCrewId,
                UserId = userId,
                ActiveFrom = DateTime.UtcNow.AddDays(-1),
                IsActive = true
            };

            db.CrewMemberships.Add(oldMembership);
            await db.SaveChangesAsync();
        }

        var crewmembershiprequest = new TransferCrewMembershipCommand(userId, oldCrewId, newCrewId);

        var result = await HttpClient.PutAsJsonAsync("/api/CrewMembership/Transfer", crewmembershiprequest);

        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}