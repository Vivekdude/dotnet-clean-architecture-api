using CleanArchitecture.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CleanArchitecture.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value == null) continue;

            var argumentType = argument.Value.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = _serviceProvider.GetService(validatorType);

            if (validator == null) continue;

            var validateMethod = validatorType.GetMethod("ValidateAsync", new[]
            {
                argumentType,
                typeof(CancellationToken)
            });

            if (validateMethod == null) continue;

            var validationTask = (Task<FluentValidation.Results.ValidationResult>?)validateMethod.Invoke(
                validator,
                new[] { argument.Value, CancellationToken.None });

            if (validationTask == null) continue;

            var validationResult = await validationTask;

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                };

                context.Result = new BadRequestObjectResult(response);
                return;
            }
        }

        await next();
    }
}
