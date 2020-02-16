using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Forms;

namespace FPLedit.Logger
{
    public sealed class MultipleLogger : ILog
    {
        private readonly List<ILog> loggers;

        public MultipleLogger(params ILog[] logger)
        {
            loggers = new List<ILog>(logger);
        }

        public bool CanAttach => true;

        public void AttachLogger(ILog other) => loggers.Add(other);

        public void Error(string message)
        {
            Application.Instance.Invoke(() =>
            {
                foreach (var log in loggers)
                    log.Error(message);
            });
        }

        public void Info(string message)
        {
            Application.Instance.Invoke(() =>
            {
                foreach (var log in loggers)
                    log.Info(message);
            });
        }

        public void Warning(string message)
        {
            Application.Instance.Invoke(() =>
            {
                foreach (var log in loggers)
                    log.Warning(message);
            });
        }

        public void LogException(Exception e)
        {
            Application.Instance.Invoke(() =>
            {
                foreach (var log in loggers)
                    log.LogException(e);
            });
        }

        public void Debug(string message)
        {
            Application.Instance.Invoke(() =>
            {
                foreach (var log in loggers)
                    log.Debug(message);
            });
        }
    }
}