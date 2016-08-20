using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Shared
{
    public struct FileState
    {
        public bool Opened;

        public bool Saved;

        public bool LineCreated;

        public bool TrainsCreated;

        public string FileName;

        public static bool operator !=(FileState s1, FileState s2)
        {
            return !s1.Compare(s2);
        }

        public static bool operator ==(FileState s1, FileState s2)
        {
            return s1.Compare(s2);
        }

        public bool Compare(FileState s)
        {
            return (s.Opened == Opened) && (s.Saved == Saved)
                && (s.LineCreated == LineCreated) && (s.TrainsCreated == TrainsCreated);
        }
    }
}
