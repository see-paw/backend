using FluentValidation;

using MediatR;

namespace WebAPI.Core;

/// <summary>
/// Pipeline behavior that performs request validation using FluentValidation before executing the handler.
/// </summary>
/// <remarks>
/// This behavior intercepts incoming requests in the MediatR pipeline, validates them using
/// the corresponding <see cref="IValidator{T}"/>, and throws a <see cref="ValidationException"/>
/// if any validation rules fail.  
/// It ensures that only valid requests reach their respective handlers.
/// </remarks>
public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null) :
    IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    /// <summary>
    /// Handles the request by validating it before passing it to the next handler in the MediatR pipeline.
    /// </summary>
    /// <param name="request">The incoming request to be validated and processed.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token used to cancel the operation if needed.</param>
    /// <returns>The response produced by the next handler in the pipeline.</returns>
    /// <remarks>
    /// If a validator is defined for the request type, it executes validation logic before calling the next handler.
    /// Throws a <see cref="ValidationException"/> if validation fails.
    /// </remarks>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (validator == null)
        {
            return await next(cancellationToken);
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await next(cancellationToken);
    }
}
