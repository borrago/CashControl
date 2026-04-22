using MediatR;

namespace CashControl.Core.Application;

public interface ICommandHandler<in TCommandInput, TCommandResult>
    : IMediatorHandler<TCommandInput, TCommandResult>
    where TCommandInput : IRequest<TCommandResult>, IMediatorInput<TCommandResult>
    where TCommandResult : IMediatorResult
{
    new Task<TCommandResult> Handle(TCommandInput command, CancellationToken cancellationToken);
}