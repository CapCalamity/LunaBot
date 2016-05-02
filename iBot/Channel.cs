﻿using System.Collections.Generic;

namespace IBot
{
    internal class Channel
    {
        public Channel()
        {
            Name = "";
            Users = new HashSet<User>();
        }

        public Channel(string name) : this()
        {
            Name = name;
        }

        public string Name { get; set; }

        public HashSet<User> Users { get; set; }
    }
}