public enum MessageType
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
}