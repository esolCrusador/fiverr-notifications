public enum StandardMessage
{
    Help,

    Started,
    Stopped,

    Resumed,
    Paused,
    NotPaused,

    Muted,
    Unmuted,
    NotMuted,

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
    CouldNotParseTime,

    PausePeriodRemoved,
    RequestPauseFrom,
    RequestPauseTo,
    PausePeriodSpecified,
    PausePeriodStarted,
    PausePeriodEnded,

    MutePeriodRemoved,
    RequestMuteFrom,
    RequestMuteTo,
    MutePeriodSpecified,
    MutePeriodStarted,
    MutePeriodEnded
}