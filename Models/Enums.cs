namespace Marketplace.Models
{
    public enum BookStatus
    {
        Pending = 0,
        Active = 1,
        Reserved = 2,
        Sold = 3
    }

    public enum PurchaseRequestStatus
    {
        New = 0,
        AwaitingPayment = 1,
        Paid = 2,
        Contacted = 3,
        Completed = 4,
        Rejected = 5
    }

    public enum BookCondition
    {
        New = 0,
        LikeNew = 1,
        VeryGood = 2,
        Good = 3,
        Acceptable = 4
    }

    public enum UserRole
    {
        Admin = 0,
        Buyer = 1,
        Seller = 2
    }
}

