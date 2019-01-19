using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.jTrainGraphStarter
{
    internal class SilentLogger : ILog
    {
        private ILog parent;

        public SilentLogger(ILog parent)
        {
            this.parent = parent;
        }

        public void Error(string message) => parent.Error(message);

        public void Info(string message)
        {
        }

        public void LogException(Exception e) => parent.LogException(e);

        public void Warning(string message)
        {
        }
    }
}
