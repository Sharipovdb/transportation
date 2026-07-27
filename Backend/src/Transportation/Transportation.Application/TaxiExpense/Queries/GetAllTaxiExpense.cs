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

        spec.Query
            .WithPagination(request.PaginationInfo);

        var taxiExpenses = await _taxiExpenseRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _taxiExpenseRepository.CountAsync(spec, cancellationToken);

        if (!taxiExpenses.Any())
            throw new ResourceNotFoundException(TaxiExpenseErrors.NotFound);

        var mappedTaxiExpence = _mapper.Map(taxiExpenses);

        return new PaginatedResult<TaxiExpenseDto>(mappedTaxiExpence, totalCount);
    }
}