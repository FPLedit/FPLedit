using FPLedit.Shared;
using System;

namespace FPLedit.jTrainGraphStarter;

internal sealed class SilentLogger : ILog
{
    public bool CanAttach => false;

    public void AttachLogger(ILog other)
    {
    }

    private readonly ILog parent;

    public SilentLogger(ILog parent)
    {
        this.parent = parent;
    }

    public void Debug(string message)
    {
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