﻿using IBot.Events.Tags;

namespace IBot.Events.Args.Users
{
    internal class UserPublicMessageEventArgs
    {
        public UserPublicMessageEventArgs(UserMessageTags tags, string userName, string channel, string message)
        {
            Tags = tags;
            UserName = userName;
            Channel = channel;
            Message = message;
        }

        public UserMessageTags Tags { get; }
        public string UserName { get; }
        public string Channel { get; }
        public string Message { get; }
    }
}