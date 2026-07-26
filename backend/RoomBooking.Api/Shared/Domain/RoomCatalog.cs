namespace RoomBooking.Api.Shared.Domain;

/// <summary>
/// Initial seed values only. Runtime room catalog comes from the Rooms table via repositories.
/// </summary>
public static class RoomCatalog
{
    public static readonly IReadOnlyList<(string Code, int Capacity)> Defaults =
    [
        ("A", 4),
        ("B", 6),
        ("C", 8),
        ("D", 10),
        ("E", 12)
    ];
}
