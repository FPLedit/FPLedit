using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit
{
    internal sealed class UndoManager
    {
        private const int MAX_STEPS = 2;

        private Timetable[] steps;
        private int pointer;

        private Timetable stagedStep = null;

        public bool CanGoBack => steps.Any(s => s != null);

        public UndoManager()
        {
            steps = new Timetable[MAX_STEPS];
        }

        public Timetable Undo()
        {
            if (steps.All(s => s == null))
                return null;

            // Pop
            pointer = (steps.Length + pointer - 1) % steps.Length;
            var step = steps[pointer];
            steps[pointer] = null;

            return step;
        }

        // Speichert den aktuellen Status, *vor* der Veränderung aufzurufen!
        public void StageUndoStep(Timetable tt)
        {
            stagedStep = tt.Clone();
        }

        // Fügt den vorher "gestageten" Step zum Stack hinzu
        public void AddUndoStep()
        {
            if (stagedStep == null)
                throw new Exception("Fehler in einer Erweiterung: Vor jeder *möglichen* Änderung muss `StageUndoStep()` aufgerufen werden!");

            // Push
            steps[pointer] = stagedStep;
            pointer = (pointer + 1) % steps.Length;

            stagedStep = null;
        }

        public void ClearHistory()
        {
            pointer = 0;
            steps = new Timetable[MAX_STEPS];
        }
    }
}
