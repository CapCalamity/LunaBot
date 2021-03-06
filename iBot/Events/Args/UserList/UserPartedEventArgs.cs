﻿using System;
using IBot.Models;

namespace IBot.Events.Args.UserList
{
    internal class UserPartedEventArgs
    {
        public UserPartedEventArgs(User user, Channel channel, DateTime time)
        {
            PartedUser = user;
            PartedChannel = channel;
            PartTime = time;
        }

        public User PartedUser { get; }
        public Channel PartedChannel { get; }
        public DateTime PartTime { get; }
    }
}
