using Ardalis.Specification;
using FluentAssertions;
using Transportation.Application.Crew.Specification;

namespace Transportation.UnitTests.Application.Crew;

/// <summary>
/// These specs answer "is this taken?", and nothing is ever really removed from this
/// database — so what they must not do is count deleted crews. Asserted against the
/// predicates themselves rather than through a mocked repository, because that is where
/// the bug was.
/// </summary>
public class CrewSpecificationTests
{
    private const string Name = "Crew A";
    private const long LeadId = 7;

    private static Domain.Entities.Crew ACrew(long id, bool isDeleted, long? driverLeadId = null, long? crewLeadId = null)
        => new()
        {
            Id = id,
            Name = Name,
            IsDeleted = isDeleted,
            DriverLeadId = driverLeadId,
            CrewLeadId = crewLeadId
        };

    [Fact]
    public void CrewExistsSpec_ShouldNotSeeADeletedCrew()
    {
        var crews = new[] { ACrew(1, isDeleted: true) };

        new CrewExistsSpec(Name).Evaluate(crews).Should().BeEmpty();
    }

    [Fact]
    public void CrewExistsSpec_ShouldSeeALiveCrew()
    {
        var crews = new[] { ACrew(1, isDeleted: false) };

        new CrewExistsSpec(Name).Evaluate(crews).Should().ContainSingle();
    }

    [Fact]
    public void CrewExistsSpec_ShouldIgnoreTheCrewBeingRenamed()
    {
        var crews = new[] { ACrew(1, isDeleted: false) };

        new CrewExistsSpec(Name, excludeCrewId: 1).Evaluate(crews).Should().BeEmpty();
    }

    [Fact]
    public void CrewByLeadsId_ShouldNotSeeADeletedCrewHoldingTheLead()
    {
        var crews = new[] { ACrew(1, isDeleted: true, driverLeadId: LeadId) };

        new CrewByLeadsId(leadId: null, driverLeadId: LeadId)
            .Evaluate(crews)
            .Should().BeEmpty();
    }

    [Fact]
    public void CrewByLeadsId_ShouldSeeALiveCrewHoldingTheLead()
    {
        var crews = new[] { ACrew(1, isDeleted: false, crewLeadId: LeadId) };

        new CrewByLeadsId(leadId: LeadId, driverLeadId: null)
            .Evaluate(crews)
            .Should().ContainSingle();
    }
}
