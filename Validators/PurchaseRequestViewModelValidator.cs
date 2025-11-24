using FluentValidation;
using Marketplace.ViewModels;

namespace Marketplace.Validators
{
    public class PurchaseRequestViewModelValidator : AbstractValidator<PurchaseRequestViewModel>
    {
        public PurchaseRequestViewModelValidator()
        {
            RuleFor(x => x.BookId).GreaterThan(0);
            RuleFor(x => x.BuyerContact.Name).NotEmpty();
            RuleFor(x => x.BuyerContact.Email).EmailAddress().NotEmpty();
            RuleFor(x => x.BuyerContact.Phone).NotEmpty();
        }
    }
}

