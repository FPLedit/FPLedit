using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }

    public class FileStateChangedEventArgs : EventArgs
    {
        public IFileState FileState { get; private set; }

        public FileStateChangedEventArgs(IFileState state)
        {
            FileState = state;
        }
    }
}
