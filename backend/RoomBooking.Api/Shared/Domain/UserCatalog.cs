namespace RoomBooking.Api.Shared.Domain;

/// <summary>
/// Initial seed values only. Runtime users come from the Users table via repositories.
/// </summary>
public static class UserCatalog
{
    public static readonly IReadOnlyList<string> DefaultUsernames =
    [
        "User1",
        "User2"
    ];

    public const string DefaultPassword = "TechnicalChallengePromtior";
}
