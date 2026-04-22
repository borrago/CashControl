using CashControl.Core.Application;
using FluentValidation;
using MediatR;

namespace CashControl.Core.CrossCutting;

public class FailFastPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMediatorInput<TResponse>
    where TResponse : IMediatorResult
{
    private readonly IEnumerable<FluentValidation.IValidator> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<object>(request);

        var validationTasks = _validators
            .Select(async v => await v.ValidateAsync(context))
            .ToList();

        var validationResults = await Task.WhenAll(validationTasks);

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new Application.ApplicationException(failures.Select(x => new CustomValidationFailure
            (
                x.PropertyName,
                x.ErrorMessage
            )));

        return await next();
    }
}