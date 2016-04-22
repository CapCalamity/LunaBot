﻿namespace IRCConnectionTest.Misc
{
    internal static class GlobalTwitchPatterns
    {
        public const string TwitchUserNamePattern = @"[a-zA-Z0-9][\w]{2,24}";
        public const string TwitchChannelNamePattern = @"[a-zA-Z0-9][\w]{2,24}";
        public const string TwitchHostNamePattern = @"tmi.twitch.tv";
        public const string JtvPattern = @"jtv";

        /// <summary>
        /// Group 1: UserName
        /// </summary>
        public const string TwichUserBasePattern =
            ":(" + TwitchUserNamePattern + ")" + 
            "!" + TwitchUserNamePattern + 
            "@" + TwitchUserNamePattern +
            "." + TwitchHostNamePattern + @"\s";
    }
}