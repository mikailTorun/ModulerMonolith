using FluentValidation;

namespace Module.Order.Application.Commands;

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId boş olamaz.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sipariş en az bir ürün içermelidir.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId boş olamaz.");

            item.RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Ürün adı boş olamaz.")
                .MaximumLength(200).WithMessage("Ürün adı 200 karakterden uzun olamaz.");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Birim fiyat 0'dan büyük olmalıdır.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Adet 0'dan büyük olmalıdır.");
        });
    }
}
