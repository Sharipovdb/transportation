using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Transportation.IntegrationTests.Common;
using Transportation.Application.Crew.Commands;
using Transportation.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Transportation.API.Models;
using Transportation.Infrastructure.Persistence;

namespace Transportation.IntegrationTests.API;

public class CrewControllerTests : IntegrationTestBase
{
    protected readonly CustomWebApplicationFactory Factory; 

    public CrewControllerTests(CustomWebApplicationFactory factory) : base(factory) => Factory = factory;
    
    [Fact]
    public async Task GetAllCrews_ShouldReturnOk_WhenRequestIsValid()
    {
        var result = await HttpClient.GetAsync(
            $"/api/Crew/GetAll?PaginationInfo.Index={1L}&PaginationInfo.Size={10L}");
        
        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }
        
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByIdCrews_ShouldReturnOk_WhenRequestIsValid()
    {
        long generatedId;

        using (var scope = Factory.Services.CreateScope()) 
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>(); 
            
            var crewToInsert = new Crew 
            { 
                Name = "Old Group Name", 
                CrewLeadId = 2L,  
                DriverLeadId = 1L, 
                RouteId = 1L, 
                SeatCapacity = 2 
            };

            db.Crews.Add(crewToInsert);
            await db.SaveChangesAsync();
          
            generatedId = crewToInsert.Id; 
        }
        
        var result = await HttpClient.GetAsync($"/api/Crew/GetById/{generatedId}");
        
        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }
        
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    
    [Fact]
    public async Task CreateCrew_ShouldBeSaveToDatabase_WhenDataIsValid()
    {
        var crewRequest = new CreateCrewCommand("Экипаж А", 1, 1, 2, 4);
        
        var result = await HttpClient.PostAsJsonAsync("/api/Crew/Create", crewRequest);
       
        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task UpdateCrew_ShouldBeSaveToDatabase_WhenDataIsValid()
    { 
        long generatedId;

        using (var scope = Factory.Services.CreateScope()) 
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>(); 
        
            var crewToInsert = new Crew 
            { 
                Name = "Old Group Name", 
                CrewLeadId = 2L,  
                DriverLeadId = 1L, 
                RouteId = 1L, 
                SeatCapacity = 2 
            };

            db.Crews.Add(crewToInsert);
            await db.SaveChangesAsync();
        
            generatedId = crewToInsert.Id;
        }

        var crewRequest = new UpdateCrewRequest{
            Name =  "New Group Name", 
            LeadId = 8L, 
            DriverId = null, 
            RouteId = 12L, 
            SeatCapacity = 3
        };
        
        var result = await HttpClient.PutAsJsonAsync($"/api/Crew/Update/{generatedId}", crewRequest);        
        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }
        
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact]
    public async Task DeleteCrew_ShouldReturnNoContent_WhenCrewExists()
    {
        long generatedId;

        using (var scope = Factory.Services.CreateScope()) 
        {
            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>(); 
    
            var crewToInsert = new Crew 
            { 
                Name = "Экипаж для удаления", 
                CrewLeadId = 2L,  
                DriverLeadId = 1L, 
                RouteId = 1L, 
                SeatCapacity = 4 
            };

            db.Crews.Add(crewToInsert);
            await db.SaveChangesAsync();
        
            generatedId = crewToInsert.Id; 
        }

        var result = await HttpClient.DeleteAsync($"/api/Crew/Delete/{generatedId}");
    
        if (result.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorDetails = await result.Content.ReadAsStringAsync();
            throw new Exception($"API вернул BadRequest (400). Ошибка от сервера: {errorDetails}");
        }
        
        result.StatusCode.Should().Be(HttpStatusCode.NoContent); 
    }
}