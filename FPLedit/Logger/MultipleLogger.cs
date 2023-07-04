using FPLedit.Shared;
using System;
using System.Collections.Generic;
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
            SafeExecute(() =>
            {
                foreach (var log in loggers)
                    log.Error(message);
            });
        }

        public void Info(string message)
        {
            SafeExecute(() =>
            {
                foreach (var log in loggers)
                    log.Info(message);
            });
        }

        public void Warning(string message)
        {
            SafeExecute(() =>
            {
                foreach (var log in loggers)
                    log.Warning(message);
            });
        }

        public void LogException(Exception e)
        {
            SafeExecute(() =>
            {
                foreach (var log in loggers)
                    log.LogException(e);
            });
        }

        public void Debug(string message)
        {
            SafeExecute(() =>
            {
                foreach (var log in loggers)
                    log.Debug(message);
            });
        }

        private void SafeExecute(Action action)
        {
            if (Application.Instance.MainForm != null && !Application.Instance.IsDisposed)
                Application.Instance.Invoke(action);
            else
                action();
        }
    }
}