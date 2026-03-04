using FluentValidation;

namespace Module.Order.Application.Queries;

internal sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotEqual(Guid.Empty)
            .WithMessage("Sipariş ID'si boş olamaz.");
    }
}
