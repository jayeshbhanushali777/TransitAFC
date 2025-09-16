namespace TransitAFC.Services.Booking.Core.Models
{
    public enum BookingStatus
    {
        Draft = 0,
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
        Completed = 4,
        Failed = 5,
        Refunded = 6,
        PartiallyRefunded = 7
    }

    public enum PassengerType
    {
        Adult = 0,
        Child = 1,
        Senior = 2,
        Student = 3,
        Disabled = 4
    }

    public enum SeatType
    {
        Regular = 0,
        Window = 1,
        Aisle = 2,
        Premium = 3,
        Accessible = 4
    }
}