﻿using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Logger
{
    public class MultipleLogger : ILog
    {
        public List<ILog> Loggers { get; set; }

        public MultipleLogger()
        {
            Loggers = new List<ILog>();
        }

        public MultipleLogger(params ILog[] logger)
        {
            Loggers = new List<ILog>();
            Loggers.AddRange(logger);
        }

        public void Error(string message)
        {
            foreach (var log in Loggers)
                log.Error(message);
        }

        public void Info(string message)
        {
            foreach (var log in Loggers)
                log.Info(message);
        }

        public void Warning(string message)
        {
            foreach (var log in Loggers)
                log.Warning(message);
        }
    }
}