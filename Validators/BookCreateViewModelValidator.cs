using FluentValidation;
using Marketplace.ViewModels;

namespace Marketplace.Validators
{
    public class BookCreateViewModelValidator : AbstractValidator<BookCreateViewModel>
    {
        public BookCreateViewModelValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Author).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);

        }
    }
}

