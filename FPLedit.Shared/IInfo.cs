using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IInfo
    {
        Timetable Timetable { get; set; }

        void BackupTimetable();

        void RestoreTimetable();

        void ClearBackup();

        FileState FileState { get; set; }

        void SetUnsaved();

        dynamic Menu { get; }

        dynamic ShowDialog(dynamic form);

        event EventHandler<FileStateChangedEventArgs> FileStateChanged;

        void RegisterExport(IExport export);

        void RegisterImport(IImport import);
    }

    public class FileStateChangedEventArgs : EventArgs
    {
        public FileState FileState { get; private set; }
   
        public FileStateChangedEventArgs(FileState state)
        {
            FileState = state;
        }
    }    
}