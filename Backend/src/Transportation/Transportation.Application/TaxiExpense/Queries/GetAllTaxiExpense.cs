using Ardalis.Specification;
using Transportation.Application.Crew.Services;
using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.TaxiExpense.Queries;

public sealed record GetAllTaxiExpense(PaginationInfo PaginationInfo) : IQuery<PaginatedResult<TaxiExpenseDto>>;

public sealed class GetAllTaxiExpenseHandler : IQueryHandler<GetAllTaxiExpense, PaginatedResult<TaxiExpenseDto>>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly ICrewVisibility _crewVisibility;
    private readonly TaxiExpenseMapper _mapper;

    public GetAllTaxiExpenseHandler(
        ITaxiExpenseRepository taxiExpenseRepository,
        ICrewVisibility crewVisibility,
        TaxiExpenseMapper mapper)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _crewVisibility = crewVisibility;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TaxiExpenseDto>> Handle(GetAllTaxiExpense request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.TaxiExpense>();

        // An empty list is a valid answer, not a 404 — a fresh month simply has no rides
        // yet, and throwing here made the whole expenses screen fail to load.
        // A fare belongs to the day that produced it, so it inherits that day's crew —
        // and with it, who is allowed to see the money.
        var visibleCrewIds = await _crewVisibility.VisibleCrewIdsAsync(cancellationToken);

        if (visibleCrewIds is not null)
            spec.Query.Where(x => visibleCrewIds.Contains(x.TransportDay.CrewId));

        spec.Query
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .WithPagination(request.PaginationInfo);

        var taxiExpenses = await _taxiExpenseRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _taxiExpenseRepository.CountAsync(spec, cancellationToken);

        var mappedTaxiExpense = _mapper.Map(taxiExpenses);

        return new PaginatedResult<TaxiExpenseDto>(mappedTaxiExpense, totalCount);
    }
}