namespace Marketplace.Models
{
    public enum BookStatus
    {
        Active = 0,
        Reserved = 1,
        Sold = 2
    }

    public enum PurchaseRequestStatus
    {
        New = 0,
        Contacted = 1,
        Completed = 2,
        Rejected = 3
    }

    public enum BookCondition
    {
        New = 0,
        LikeNew = 1,
        VeryGood = 2,
        Good = 3,
        Acceptable = 4
    }
}

