public enum StandardMessage
{
    Help,

    Started,
    Stopped,

    Resumed,
    Paused,

    Muted,
    Unmuted,

    RequestUsername,
    UsernameSpecified,

    RequestSessionKey,
    SessionKeySpecified,

    RequestToken,
    TokenSpecified,

    SuccessfullyConnected,

    Cancelled,
    UnknownCommand,
    WrongCredentials,

    LocationForTimezone,
    TimezoneSpecified,
    CouldNotParseTime
}