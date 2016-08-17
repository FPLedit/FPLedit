using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Shared
{
    public interface IInfo
    {
        Timetable Timetable { get; set; }

        void BackupTimetable();

        void RestoreTimetable();

        void ClearBackup();

        bool FileOpened { get; }

        bool FileSaved { get; }

        dynamic Menu { get; }

        dynamic ShowDialog(dynamic form);

        event EventHandler<FileStateChangedEventArgs> FileStateChanged;
    }

    public class FileStateChangedEventArgs : EventArgs
    {
        public bool Opened { get; private set; }

        public bool Saved { get; private set; }

        public FileStateChangedEventArgs(bool opened, bool saved)
        {
            Opened = opened;
            Saved = saved;
        }
    }
}
