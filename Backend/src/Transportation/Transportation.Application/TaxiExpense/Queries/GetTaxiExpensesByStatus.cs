using Ardalis.Specification;
using FluentValidation;
using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.TaxiExpense.Queries;

public record GetTaxiExpensesByStatus(
    TaxiExpenseStatus TaxiExpenseStatus,
    PaginationInfo PaginationInfo
) : IQuery<PaginatedResult<TaxiExpenseDto>>;

// ReSharper disable once UnusedType.Global
public sealed class GetTaxiExpensesByStatusValidator : AbstractValidator<GetTaxiExpensesByStatus>
{
    public GetTaxiExpensesByStatusValidator()
    {
        RuleFor(x => x.TaxiExpenseStatus)
            .IsInEnum()
            .WithMessage("TaxiExpenseStatus must be in Enum");
    }
}

internal sealed class
    GetTaxiExpensesByStatusQueryHandler : IQueryHandler<GetTaxiExpensesByStatus, PaginatedResult<TaxiExpenseDto>>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly TaxiExpenseMapper _taxiExpenseMapper;

    public GetTaxiExpensesByStatusQueryHandler(
        ITaxiExpenseRepository taxiExpenseRepository,
        TaxiExpenseMapper taxiExpenseMapper)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _taxiExpenseMapper = taxiExpenseMapper;
    }

    public async Task<PaginatedResult<TaxiExpenseDto>> Handle(GetTaxiExpensesByStatus request,
        CancellationToken cancellationToken)
    {
        var spec = new ReadOnlySpecification<Domain.Entities.TaxiExpense>();
        spec.Query
            .Where(x => x.TaxiExpenseStatus == request.TaxiExpenseStatus && !x.IsDeleted)
            .WithPagination(request.PaginationInfo);

        var taxiExpenses = await _taxiExpenseRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _taxiExpenseRepository.CountAsync(spec, cancellationToken);

        var mappedExpenses = _taxiExpenseMapper.Map(taxiExpenses);

        return new PaginatedResult<TaxiExpenseDto>(mappedExpenses, totalCount);
    }
}