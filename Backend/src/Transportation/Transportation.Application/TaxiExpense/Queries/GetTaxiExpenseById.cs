using Ardalis.Specification;
using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Application.TaxiExpense.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.TaxiExpense.Queries;

public sealed record GetTaxiExpenseById(long Id) : IQuery<TaxiExpenseDto>;

internal sealed class GetTaxiExpenseByIdHandler : IQueryHandler<GetTaxiExpenseById, TaxiExpenseDto>
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly TaxiExpenseMapper _taxiExpenseMapper;

    public GetTaxiExpenseByIdHandler(ITaxiExpenseRepository taxiExpenseRepository, TaxiExpenseMapper taxiExpenseMapper)
    {
        _taxiExpenseRepository = taxiExpenseRepository;
        _taxiExpenseMapper = taxiExpenseMapper;
    }

    public async Task<TaxiExpenseDto> Handle(GetTaxiExpenseById request, CancellationToken cancellationToken)
    {
        var spec = new TaxiExpenseByIdSpec(request.Id);
        var entity = await _taxiExpenseRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(TaxiExpenseErrors.NotFound);
        
        return _taxiExpenseMapper.Map(entity);
    }
}