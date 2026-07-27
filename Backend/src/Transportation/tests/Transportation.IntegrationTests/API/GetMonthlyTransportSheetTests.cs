using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence;
using Transportation.IntegrationTests.Common;

namespace Transportation.IntegrationTests.API;

public class GetMonthlyTransportSheetTests : IntegrationTestBase
{
    public GetMonthlyTransportSheetTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetById_ShouldReturnMonthlyTransportSheet()
    {
        await ResetDatabaseAsync();

        using var scope = Factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<TransportationDbContext>();

        var entity = new MonthlyTransportSheet
        {
            Id = 1,
            // fill required properties
        };

        db.MonthlyTransportSheets.Add(entity);
        await db.SaveChangesAsync();

        var response = await HttpClient.GetAsync("/api/MonthlyTransportSheets/1");

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<MonthlyTransportSheetDto>();

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }
}