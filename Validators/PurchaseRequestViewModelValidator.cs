using FluentValidation;
using Marketplace.ViewModels;

namespace Marketplace.Validators
{
    public class PurchaseRequestViewModelValidator : AbstractValidator<PurchaseRequestViewModel>
    {
        public PurchaseRequestViewModelValidator()
        {
            RuleFor(x => x.BookId).GreaterThan(0);

        }
    }
}

