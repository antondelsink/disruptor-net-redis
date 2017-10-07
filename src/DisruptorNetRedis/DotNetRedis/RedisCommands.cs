namespace DisruptorNetRedis.DisruptorRedis
{
    public enum RedisCommands : byte
    {
        // Strings
        SET,
        GET,

        // Core
        PING,
        ECHO,
        COMMAND,
        CLIENT_SETNAME,

        Unknown,
        SUBSCRIBE,
        INFO
    }
}