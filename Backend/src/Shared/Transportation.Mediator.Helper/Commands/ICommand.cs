using MediatR;

namespace Transportation.Mediator.Helper.Commands;

public interface IBaseCommand : IBaseRequest;

public interface ICommand : IBaseCommand, IRequest;

public interface ICommand<out TResponse> : IBaseCommand, IRequest<TResponse>;

public interface ICommandHandler<in TRequest> : IRequestHandler<TRequest>
    where TRequest : ICommand;

public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : ICommand<TResponse>;