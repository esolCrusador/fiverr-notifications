public enum MessageType
{
    Help,

    Started,
    Stopped,

    Resumed,
    Paused,

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