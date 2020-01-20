using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit
{
    public class LastFileHandler : ILastFileHandler
    {
        private List<string> lastFiles;
        private IPluginInterface pluginInterface;
        
        public bool Enabled { get; private set; }
        public IEnumerable<string> LastFiles => lastFiles.AsReadOnly();
        
        public event EventHandler LastFilesUpdates;

        public LastFileHandler()
        {
            lastFiles = new List<string>();
            Enabled = true;
        }

        public void Initialize(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            Enabled = pluginInterface.Settings.Get("files.save-last", true);
            lastFiles = pluginInterface.Settings.Get("files.last", "").Split(';').Where(s => s != "").Reverse().ToList();
            
            LastFilesUpdates?.Invoke(this, null);
        }

        public void AddLastFile(string filename)
        {
            if (!Enabled) 
                return;
            
            if (!filename.EndsWith(".fpl"))
                filename += ".fpl";

            lastFiles.RemoveAll(s => s == filename); // Doppelte Dateinamen verhindern
            lastFiles.Insert(0, filename);
            if (lastFiles.Count > 3) // Überlauf
                lastFiles.RemoveAt(lastFiles.Count - 1);

            LastFilesUpdates?.Invoke(this, null);
        }

        public void Persist()
        {
            if (Enabled)
                pluginInterface.Settings.Set("files.last", string.Join(";", lastFiles));
        }
    }
}