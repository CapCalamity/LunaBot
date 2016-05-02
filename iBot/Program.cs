﻿using System;
using System.Diagnostics;

namespace IBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var app = new App();
                app.StartApp();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }
    }
}