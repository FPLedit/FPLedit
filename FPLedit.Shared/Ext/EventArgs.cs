using System;

namespace FPLedit.Shared
{
    public sealed class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }

    public sealed class FileStateChangedEventArgs : EventArgs
    {
        public IFileState FileState { get; }

        public FileStateChangedEventArgs(IFileState state)
        {
            FileState = state;
        }
    }
}
