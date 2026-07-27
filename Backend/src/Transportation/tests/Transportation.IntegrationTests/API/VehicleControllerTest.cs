//using System.Net;
//using System.Net.Http.Json;
//using FluentAssertions;
//using Transportation.Application.Vehicle.Commands;
//using Transportation.IntegrationTests.Common;
//using Transportation.Application.Vehicle.Models;
//using Transportation.Infrastructure.Persistence;
//using Transportation.Domain.Entities;
//using Org.BouncyCastle.Asn1.Misc;
//using Microsoft.Extensions.DependencyInjection;
//using Transportation.Mediator.Helper.Common.Models;
//using Transportation.Application.Vehicle.Specifications;
//
//
//namespace Transportation.IntegrationTests.API; 
//
//public class CreateVehicleTests : IntegrationTestBase
//{
//
//    public CreateVehicleTests(CustomWebApplicationFactory factory) : base(factory)
//    {
//    }
//
//    [Fact]
//    public async Task CreateVehicle_ShouldSaveToDatabase_WhenDataIsValid()
//    {
//        await ResetDatabaseAsync();
//
//        long actualDriverId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//
//            var driver = new Employee
//            {
//                Name = "Alisher",
//                PhoneNumber = "+992925555555",
//                TelegramId = "@Alisher",
//                Role = Role.DriverLead
//            };
//
//            db.Employees.Add(driver);
//            await db.SaveChangesAsync();
//        
//            actualDriverId = driver.Id; 
//        }
//
//        var request = new CreateVehicleCommand(
//        DriverId: actualDriverId,
//        Plate: "1313OOTj02",
//        SeatCount: 2,
//        AmortizationBasis: 100);
//    
//
//        // Act
//        var response = await _httpClient.PostAsJsonAsync("/api/Vehicle/Create", request);
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//    }
//
//    [Fact]
//    public async Task GetById_ShouldReturnVehicle_WhenVehicleExists()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//
//        long GeneratedVehicleId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//           var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//
//           var driver =new Employee{
//     
//           Name="Alisher",
//           PhoneNumber = "+992925555555",
//           TelegramId="@Alisher",
//           Role = Role.DriverLead
//        
//           };
//
//           db.Employees.Add(driver);
//           await db.SaveChangesAsync();
//
//
//           var vehicle = new Vehicle
//           {
//  
//               DriverId = driver.Id,
//               Plate = "1313OOTj02",
//               SeatCount = 2,
//               AmortizationBasis = 100,
//           };
//
//           db.Vehicles.Add(vehicle);
//
//           await db.SaveChangesAsync();
//        
//           GeneratedVehicleId = vehicle.Id;
//
//
//      
//        }
//        // Act
//        var response = await _httpClient.GetAsync($"/api/Vehicle/GetById/{GeneratedVehicleId}");
//
//        if (response.StatusCode == HttpStatusCode.BadRequest)
//        {
//            var errorContent  =await response.Content.ReadAsStringAsync();
//            throw new Exception($"Server vernul Bad request. Text oshibki: {errorContent}");
//        }
//
//       // Assert
//       response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//       var result = await response.Content.ReadFromJsonAsync<VehicleDto>();
//       result.Should().NotBeNull();
//       result!.Id.Should().Be(GeneratedVehicleId);
//       result.Plate.Should().Be( "1313OOTj02");   
//    }
//
//   [Fact]
//    public async Task GetByDriverId_ShouldReturnVehicle_WhenVehicleExists()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//
//        long actualDriverId;
//
//        
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//            var driver = new Employee
//            {
//                Name="Alisher",
//                PhoneNumber = "+992925555555",
//                TelegramId="@Alisher",
//                Role = Role.DriverLead
//               
//            };
//            db.Employees.Add(driver);
//            await db.SaveChangesAsync();
//        
//            actualDriverId = driver.Id; 
//        }
//
//        var createCommand = new CreateVehicleCommand(
//        DriverId: actualDriverId, 
//        Plate: "1313OOTj02",
//        SeatCount: 2,
//        AmortizationBasis: 100);
//       
//    
//        var createResponse = await _httpClient.PostAsJsonAsync("/api/Vehicle/Create", createCommand);
//        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        // Act 
//        var response = await _httpClient.GetAsync($"/api/Vehicle/GetByDriverId/{actualDriverId}");
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var result = await response.Content.ReadFromJsonAsync<VehicleDto>();
//        result.Should().NotBeNull();
//        result!.DriverId.Should().Be(actualDriverId);
//        result.Plate.Should().Be("1313OOTj02");
//    } 
//
//    /*[Fact]
//    public async Task GetAll_ShouldReturnAllVehicles_WhenVehiclesExist()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//
//        long actualDriverId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//            var driver = new Employee
//            {
//                Name = "Alisher",
//                PhoneNumber = "+992925555555",
//                TelegramId = "@Alisher",
//                Role = Role.DriverLead
//            };
//            db.Employees.Add(driver);
//            await db.SaveChangesAsync();
//        
//            actualDriverId = driver.Id; 
//        }
//           
//        var createCommand = new CreateVehicleCommand(
//            DriverId: actualDriverId, 
//            Plate: "1313OOTj02",
//            SeatCount: 2,
//            AmortizationBasis: 100
//        );
//
//        var createResponse = await _httpClient.PostAsJsonAsync("/api/Vehicle/Create", createCommand);
//        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        // Act 
//        var response = await _httpClient.GetAsync("/api/Vehicle/GetAll?PageIndex=0&PageSize=10");  
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<VehicleDto>>();
//        result.Should().NotBeNull();
//        result!.Items.Should().NotBeEmpty();
//    
//        result.Items.Any(v => v.Plate == "1313OOTj02").Should().BeTrue();
//    }*/
//    
//    [Fact]
//    public async Task Update_ShouldUpdateVehicle_WhenVehicleExists()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//
//        long actualDriverId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//        
//            var driver = new Employee
//            {
//                Name = "Alisher",
//                PhoneNumber = "+992925555555",
//                TelegramId = "@Alisher",
//                Role = Role.DriverLead
//            };
//
//            db.Employees.Add(driver);
//            await db.SaveChangesAsync();
//
//            actualDriverId = driver.Id;
//        }
//
//        var createCommand = new CreateVehicleCommand(
//        DriverId: actualDriverId,
//        Plate: "1313OOTj02",
//        SeatCount: 2,
//        AmortizationBasis: 100);
//    
//
//        var createResponse = await _httpClient.PostAsJsonAsync("/api/Vehicle/Create", createCommand);
//        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var createdVehicle = await createResponse.Content.ReadFromJsonAsync<VehicleDto>();
//        long generatedVehicleId = createdVehicle!.Id;
//
//        var updateRequest = new UpdateVehicleRequest{
//        driver_id = actualDriverId,
//        plate = "5555TJ09",
//        seat_count = 6,
//        amortization_basis = 250
//        };
//
//        // Act 
//        var response = await _httpClient.PutAsJsonAsync($"/api/Vehicle/Update/{generatedVehicleId}", updateRequest);
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var result = await response.Content.ReadFromJsonAsync<VehicleDto>();
//        result.Should().NotBeNull();
//        result!.Id.Should().Be(generatedVehicleId);
//        result.DriverId.Should().Be(actualDriverId); 
//        result.Plate.Should().Be("5555TJ09");         
//        result.SeatCount.Should().Be(6);              
//    } 
//
//    /*[Fact]
//    public async Task Delete_ShouldReturnTrue_WhenVehicleExists()
//    {
//        // Arrange
//        await ResetDatabaseAsync();
//
//        long actualDriverId;
//        long generatedVehicleId;
//
//        using (var scope = _factory.Services.CreateScope())
//        {
//            var db = scope.ServiceProvider.GetRequiredService<TransportationDbContext>();
//
//            var driver = new Employee
//            {
//                Name = "Alisher",
//                PhoneNumber = "+992925555555",
//                TelegramId = "@Alisher",
//                Role = Role.DriverLead
//            };
//            db.Employees.Add(driver);
//            await db.SaveChangesAsync();
//
//            actualDriverId = driver.Id;
//            var vehicle = new Vehicle
//            {
//                DriverId = actualDriverId, 
//                Plate = "3333TJ03",
//                SeatCount = 4,
//                AmortizationBasis = 90 
//            };
//            
//            db.vehicles.Add(vehicle); 
//            await db.SaveChangesAsync();
//
//            generatedVehicleId = vehicle.Id;
//        }
//
//        // Act
//        var response = await _httpClient.DeleteAsync($"/api/Vehicle/Delete/{generatedVehicleId}");
//
//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);
//
//        var isDeleted = await response.Content.ReadFromJsonAsync<bool>();
//        isDeleted.Should().BeTrue();
//    }*/
//}
//    
//