using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Commands;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.TransportDay.Commands;

public class DeleteTransportDayCommandTests
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteTransportDayCommandHandler _transportDayhandler;

    public DeleteTransportDayCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();
        _transportDayRepository=fixture.Freeze<ITransportDayRepository>();
        _unitOfWork=fixture.Freeze<IUnitOfWork>();
        
        _transportDayhandler=fixture.Create<DeleteTransportDayCommandHandler>();
        
         
    }

    [Fact]
    public async Task DeleteTransportDayShouldDeleteTransportDay()
    {
        //arrange
        var fixture = new Fixture();
        var command = fixture.Create<DeleteTransportDayCommand>();
        _transportDayRepository
            .GetByIdAsync(command.Id)
            .Returns((Domain.Entities.TransportDay?)null);
        
        //act 
        var action =()=>_transportDayhandler.Handle(command, CancellationToken.None);
        
        //assert
        var exeption = await action.Should().ThrowAsync<BusinessLogicException>();
        exeption.Which.Error.Should().Be(TransportDayErrors.NotFound);
        
    }

    [Fact]
    public async Task DeleteTransportDayShouldNotDeleteTransportDay()
    {
        //arrange
        var fixture = new Fixture();
        var command = fixture.Create<DeleteTransportDayCommand>();
        
    }
   
}