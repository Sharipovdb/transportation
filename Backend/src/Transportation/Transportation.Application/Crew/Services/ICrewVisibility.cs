using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Shared;
using Transportation.Shared.Authorization;
using Transportation.Shared.Extensions;
using Transportation.Shared.Middlewares;

namespace Transportation.Application.Crew.Services;

public interface ICrewVisibility
{
    /// <summary>
    /// The crews the caller may read, or <see langword="null"/> when they may read every
    /// crew. Null means "no restriction", never "nothing" — an empty array is the real
    /// answer for a lead who currently leads no crew.
    /// </summary>
    Task<long[]?> VisibleCrewIdsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Who is allowed to see which crews, decided in exactly one place.
///
/// The office roles run the company and see all of it. A crew lead sees the crews they
/// lead and nothing else: they log those crews' days and front those crews' taxi fares,
/// and another crew's roster, travel and money are none of their business. Every list
/// endpoint asks this same question rather than re-deriving the rule, so a screen can
/// never show a lead more than the endpoint behind it would.
/// </summary>
internal sealed class CrewVisibility : ICrewVisibility
{
    private static readonly string[] CompanyWideRoles =
        [RoleNames.Admin, RoleNames.Accountant, RoleNames.RouteManager];

    private readonly ICrewRepository _crewRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CrewVisibility(
        ICrewRepository crewRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        _crewRepository = crewRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<long[]?> VisibleCrewIdsAsync(CancellationToken cancellationToken)
    {
        var user = _currentUserAccessor.GetRequiredUser();

        if (CompanyWideRoles.Any(user.IsInRole))
            return null;

        // A crew has one lead, held either as its driver-lead or as its manager-lead, so
        // the caller is matched against both keys.
        var leadId = user.GetUserId();

        var crews = await _crewRepository.ListAsync(
            new CrewByLeadsId(leadId, leadId, asNoTracking: true),
            cancellationToken);

        return crews.Select(crew => crew.Id).ToArray();
    }
}
