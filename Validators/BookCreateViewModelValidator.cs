using FluentValidation;
using Marketplace.ViewModels;
using Marketplace.Utilities;

namespace Marketplace.Validators
{
    public class BookCreateViewModelValidator : AbstractValidator<BookCreateViewModel>
    {
        public BookCreateViewModelValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Author).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Category)
                .Must(c => string.IsNullOrWhiteSpace(c) || System.Array.Exists(Categories.Allowed, a => a == c))
                .WithMessage("Please select a valid category");

        }
    }
}
