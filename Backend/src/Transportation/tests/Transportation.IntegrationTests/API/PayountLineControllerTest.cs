//using Transportation.IntegrationTests.Common;
//using Transportation.Domain.Entities;
//using Transportation.Infrastructure.Persistence;
//using System.Net.Http.Json;
//using FluentAssertions;
//using Transportation.Application.PayoutLine.Commands;
//using System.Net;
//
//
//using Microsoft.Extensions.DependencyInjection;
//using Transportation.Application.PayoutLine.Models;
//
//
//namespace Transportation.IntegrationTests.API;
//
//public class PayountLineControllerTest : IntegrationTestBase
//{
//    public PayountLineControllerTest(CustomWebApplicationFactory factory) : base(factory)
//    {
//        
//    }
//
//    private async Task<(long EmployeeId, long SheetId)> CreateDependenciesAsync(TransportationDbContext db)
//    {
//        var employee = new Employee
//        {
//            Name = "Shohina Boboeva",
//            PhoneNumber = "+99292333333",
//            TelegramId = "@shohina",
//            Role = Role.Worker
//        };
//        db.Employees.Add(employee);
//        await db.SaveChangesAsync(); 
//
//        var route = new Route
//        {
//            Name = " Khistevarz-Dekhmoy",
//            DistanceKm = 45
//        };
//        db.Routes.Add(route);
//        await db.SaveChangesAsync();
//
//        var crew = new Crew
//        {
//            Name = "Ekipaj №1",
//            RouteId = route.Id,
//            CrewLeadId = employee.Id,
//            DriverLeadId = employee.Id,
//            SeatCapacity = 5
//        };
//        db.Crews.Add(crew);
//        await db.SaveChangesAsync(); 
//
//        var sheet = new MonthlyTransportSheet
//        {
//            CrewId = crew.Id,
//            Year = 2026,
//            Month = 7,
//            IsConfirmed = false
//        };
//        db.MonthlyTransportSheets.Add(sheet);
//        await db.SaveChangesAsync(); 
//
//        return (employee.Id, sheet.Id);
//    }
//
//    [Fact]
//    public async Task Create_ShouldSaveToDatabase_WhenDataIsValid()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//
//        long employeeId;
//        long sheetId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//            (employeeId, sheetId) = await CreateDependenciesAsync(db);
//        }
//
//        var command = new CreatePayoutLineCommand(
//            MonthlyTransportSheetId: sheetId,
//            UserId: employeeId,
//            DriverPayment: 1000,
//            ExtraKmPayment: 50,
//            TaxiCompensation: 100
//        );
//
//        // Act 
//        var response = await _httpClient.PostAsJsonAsync("/api/PayoutLine/Create", command);
//
//        // Assert 
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var result = await response.Content.ReadFromJsonAsync<PayoutLineDto>();
//        result.Should().NotBeNull();
//        result!.EmployeeId.Should().Be(employeeId);
//        result.MonthlyTransportSheetId.Should().Be(sheetId);
//        result.DriverPayment.Should().Be(1000);
//        result.ExtraKmPayment.Should().Be(50);
//        result.TaxiCompensation.Should().Be(100);
//        result.TotalAmount.Should().Be(1150); 
//    }
//
//
//
//
//   
//}
//