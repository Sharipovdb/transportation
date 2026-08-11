using Ardalis.Specification;
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
    private readonly TaxiExpenseMapper _mapper;

    public GetAllTaxiExpenseHandler(ITaxiExpenseRepository taxiExpenseRepository, TaxiExpenseMapper mapper)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TaxiExpenseDto>> Handle(GetAllTaxiExpense request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.TaxiExpense>();

        // An empty list is a valid answer, not a 404 — a fresh month simply has no rides
        // yet, and throwing here made the whole expenses screen fail to load.
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