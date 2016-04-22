﻿using IRCConnectionTest.Events.Misc;

namespace IRCConnectionTest.Events.ComstumEventArgs
{
    internal class RoomStateAllEventArgs
    {
        public RoomStateAllEventArgs(RoomStateTags tags, string channel)
        {
            Tags = tags;
            Channel = channel;
        }

        public RoomStateTags Tags { get; }
        public string Channel { get; }
    }
}