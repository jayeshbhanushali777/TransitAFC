namespace TransitAFC.Services.Payment.Core.Models
{
    public enum PaymentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Refunded = 5,
        PartiallyRefunded = 6,
        Expired = 7,
        OnHold = 8,
        Disputed = 9
    }

    public enum PaymentMethod
    {
        UPI = 0,
        CreditCard = 1,
        DebitCard = 2,
        NetBanking = 3,
        Wallet = 4,
        Cash = 5,
        BankTransfer = 6,
        EMI = 7,
        PayLater = 8
    }

    public enum PaymentGateway
    {
        Razorpay = 0,
        Stripe = 1,
        PayU = 2,
        CCAvenue = 3,
        Paytm = 4,
        PhonePe = 5,
        GooglePay = 6,
        BHIM = 7,
        AmazonPay = 8,
        Internal = 9
    }

    public enum TransactionType
    {
        Payment = 0,
        Refund = 1,
        Chargeback = 2,
        Adjustment = 3,
        Fee = 4,
        Penalty = 5
    }

    public enum RefundStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        PartiallyCompleted = 5
    }

    public enum PaymentMode
    {
        Online = 0,
        Offline = 1,
        Subscription = 2,
        Recurring = 3,
        Installment = 4
    }
}