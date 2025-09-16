namespace TransitAFC.Services.Ticket.Core.Models
{
    public enum TicketStatus
    {
        Draft = 0,
        Generated = 1,
        Active = 2,
        Used = 3,
        Expired = 4,
        Cancelled = 5,
        Refunded = 6,
        Suspended = 7,
        Invalid = 8
    }

    public enum TicketType
    {
        SingleJourney = 0,
        Return = 1,
        DayPass = 2,
        WeeklyPass = 3,
        MonthlyPass = 4,
        Annual = 5,
        Student = 6,
        Senior = 7,
        Disabled = 8,
        Group = 9,
        Tourist = 10
    }

    public enum ValidationResult
    {
        Valid = 0,
        Invalid = 1,
        Expired = 2,
        AlreadyUsed = 3,
        Cancelled = 4,
        NotActive = 5,
        Suspended = 6,
        OutOfZone = 7,
        OutOfTime = 8,
        InsufficientBalance = 9
    }

    public enum QRCodeStatus
    {
        Active = 0,
        Expired = 1,
        Used = 2,
        Invalid = 3,
        Regenerated = 4
    }

    public enum TransportMode
    {
        Bus = 0,
        Metro = 1,
        Train = 2,
        Tram = 3,
        Ferry = 4,
        Mixed = 5
    }

    public enum TicketValidationType
    {
        Entry = 0,
        Exit = 1,
        Transfer = 2,
        Inspection = 3
    }

    public enum FareType
    {
        Standard = 0,
        Peak = 1,
        OffPeak = 2,
        Night = 3,
        Weekend = 4,
        Holiday = 5,
        Student = 6,
        Senior = 7,
        Disabled = 8
    }
}