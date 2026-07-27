//using System.Net;
//using System.Net.Http.Json;
//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Transportation.Application.TaxiExpense.Commands;
//using Transportation.Domain.Entities;
//using Transportation.Infrastructure.Persistence;
//using Transportation.IntegrationTests.Common;
//
//namespace Transportation.IntegrationTests.API;
//
//public class TaxiExpenseControllerTests : IntegrationTestBase
//{
//    public TaxiExpenseControllerTests(CustomWebApplicationFactory factory) 
//        : base(factory) { }
//
//    [Fact]
//    public async Task Create_ShouldSaveDatabase_WhenDataIsCorrect()
//    {
//        // Arrange
//        var employee = new Employee
//        {
//            Name = "Muhammad",
//            PhoneNumber = "+992873012005",
//            TelegramId = Guid.NewGuid().ToString(),
//            Role = Role.DriverLead
//        };
//        
//        var route = new Route
//        {
//            Name = "Route A",
//            DistanceKm = 50
//        };
//        
//        var crew = new Crew   
//        {
//            Name = "BM-1026",
//            Route = route,
//            CrewLeadId = employee.Id,
//            //DriverLeadId = employee.Id,
//            SeatCapacity = 4
//        };
//        
//        var transportDay = new TransportDay
//        {
//            CrewId = crew.Id,
//            Date = DateTime.UtcNow,
//            MorningMode = TransportMode.Taxi,
//            AfternoonMode = TransportMode.Taxi,
//            CommuteKm = 10,
//            ExtraBusinessKm = 0,
//            LoggedBy = employee.Id,
//            Confirmed = false
//        };
//        
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//            
//            db.Employees.Add(employee);
//            db.Routes.Add(route);
//            db.Crews.Add(crew);
//            db.TransportDays.Add(transportDay);
//            
//            await db.SaveChangesAsync();
//        }
//
//        var taxiExpense = new CreateTaxiExpenseCommand
//        (
//            TransportDayId: transportDay.Id,
//            Leg: Leg.Afternoon,
//            Amount: 1026,
//            PaidById: employee.Id,
//            PaymentTiming: PaymentTiming.Immediate,
//            ReimbursementStatus: ReimbursementStatus.Pending
//        );
//
//        // Act
//        var response = await _httpClient.PostAsJsonAsync("/api/TaxiExpenseCreate", taxiExpense);
//        
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//    }
//}
//