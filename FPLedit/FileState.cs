using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit
{
    internal class FileState : IFileState
    {
        private bool opened, saved;
        private string fileName;

        public bool Opened { get => opened; set => TriggerEvent(opened = value); }

        public bool Saved { get => saved; set => TriggerEvent(saved = value); }

        public bool LineCreated { get; private set; }

        public bool TrainsCreated { get; private set; }

        public bool CanGoBack { get; private set; }

        public string FileName { get => fileName; set => TriggerEvent(fileName = value); }

        public int SelectedRoute { get; set; }

        public void UpdateMetaProperties(Timetable tt, UndoManager undo)
        {
            LineCreated = tt?.Stations.Count > 1; // Mind. 2 Bahnhöfe
            TrainsCreated = tt?.Trains.Count > 0;
            CanGoBack = undo.CanGoBack;
        }

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        private void TriggerEvent(object o) => FileStateInternalChanged?.Invoke(this, new FileStateChangedEventArgs(this));
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen

        internal event EventHandler<FileStateChangedEventArgs> FileStateInternalChanged;
    }
}
