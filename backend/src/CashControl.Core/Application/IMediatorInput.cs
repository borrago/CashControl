using MediatR;

namespace CashControl.Core.Application;

public interface IMediatorInput<out TMediatorResult> : IRequest<TMediatorResult> where TMediatorResult : IMediatorResult;