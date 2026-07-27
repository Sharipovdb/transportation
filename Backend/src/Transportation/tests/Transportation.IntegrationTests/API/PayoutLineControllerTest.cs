//using Transportation.IntegrationTests.Common;
//using Transportation.Domain.Entities;
//using Transportation.Infrastructure.Persistence;
//using System.Net.Http.Json;
//using FluentAssertions;
//using Transportation.Application.PayoutLine.Models;
//using Transportation.Application.PayoutLine.Commands;
//using System.Net;
//using Microsoft.Extensions.DependencyInjection;
//
//namespace Transportation.IntegrationTests.API;
//
//public class PayoutLineControllerTest:IntegrationTestBase
//{
//    public PayoutLineControllerTest(CustomWebApplicationFactory factory) : base(factory)
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
//    [Fact]
//    public async Task GetByIdPayoutLine()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//        long payoutLineId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//            var (employeeId, sheetId) = await CreateDependenciesAsync(db);
//
//            var payoutLine = new PayoutLine
//            {
//                MonthlyTransportSheetId = sheetId,
//                EmployeeId = employeeId,
//                DriverPayment = 2000,
//                ExtraKmPayment = 150,
//                TaxiCompensation = 100
//            };
//
//            db.PayoutLines.Add(payoutLine);
//            await db.SaveChangesAsync();
//            payoutLineId = payoutLine.Id;
//        }
//
//        // Act
//        var response = await _httpClient.GetAsync($"/api/PayoutLine/GetById/{payoutLineId}");
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var result = await response.Content.ReadFromJsonAsync<PayoutLineDto>();
//        result.Should().NotBeNull();
//        result!.Id.Should().Be(payoutLineId);
//        result.DriverPayment.Should().Be(2000);
//        result.TotalAmount.Should().Be(2250); 
//    }
//    
//    [Fact]
//    public async Task GetAllPayouytLine()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//        long sheetId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//            var (employeeId, dependencySheetId) = await CreateDependenciesAsync(db);
//            sheetId = dependencySheetId;
//
//            var payoutLine = new PayoutLine
//            {
//                MonthlyTransportSheetId = sheetId,
//                EmployeeId = employeeId,
//                DriverPayment = 500,
//                ExtraKmPayment = 0,
//                TaxiCompensation = 0
//            };
//
//            db.PayoutLines.Add(payoutLine);
//            await db.SaveChangesAsync();
//        }
//
//        // Act
//        var response = await _httpClient.GetAsync(
//            $"/api/PayoutLine/GetAll?MonthlyTransportSheetId={sheetId}&PageIndex=0&PageSize=10");
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var result = await response.Content.ReadFromJsonAsync<Mediator.Helper.Common.Models.PaginatedResult<PayoutLineDto>>();
//        result.Should().NotBeNull();
//        result!.Items.Should().NotBeEmpty();
//        result.Items.First().MonthlyTransportSheetId.Should().Be(sheetId);
//    }
//    
//    [Fact]
//    public async Task UpdatePayoutLine()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//        long payoutLineId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//            var (employeeId, sheetId) = await CreateDependenciesAsync(db);
//
//            var payoutLine = new PayoutLine
//            {
//                MonthlyTransportSheetId = sheetId,
//                EmployeeId = employeeId,
//                DriverPayment = 1000,
//                ExtraKmPayment = 50,
//                TaxiCompensation = 50
//            };
//
//            db.PayoutLines.Add(payoutLine);
//            await db.SaveChangesAsync();
//            payoutLineId = payoutLine.Id;
//        }
//
//        var updateRequest = new UpdatePayoutLineRequest
//        {
//            DriverPayment = 1500,
//            ExtraKmPayment = 100,
//            TaxiCompensation = 200
//        };
//        
//        // Act
//        var response = await _httpClient.PutAsJsonAsync($"/api/PayoutLine/Update/{payoutLineId}", updateRequest);
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var result = await response.Content.ReadFromJsonAsync<PayoutLineDto>();
//        result.Should().NotBeNull();
//        result!.Id.Should().Be(payoutLineId);
//        result.DriverPayment.Should().Be(1500);
//        result.ExtraKmPayment.Should().Be(100);
//        result.TaxiCompensation.Should().Be(200);
//        result.TotalAmount.Should().Be(1800); 
//    }
//    
//    [Fact]
//    public async Task DeletePayoutLine()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//        long payoutLineId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//            var (employeeId, sheetId) = await CreateDependenciesAsync(db);
//
//            var payoutLine = new PayoutLine
//            {
//                MonthlyTransportSheetId = sheetId,
//                EmployeeId = employeeId,
//                DriverPayment = 1000
//            };
//
//            db.PayoutLines.Add(payoutLine);
//            await db.SaveChangesAsync();
//            payoutLineId = payoutLine.Id;
//        }
//
//        // Act
//        var response = await _httpClient.DeleteAsync($"/api/PayoutLine/Delete/{payoutLineId}");
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var isDeleted = await response.Content.ReadFromJsonAsync<bool>();
//        isDeleted.Should().BeTrue();
//    }
//
//
//}