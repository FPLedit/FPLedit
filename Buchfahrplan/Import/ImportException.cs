using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.Import
{
    class ImportException: Exception
    {
        public ImportException()
        {

        }        

        public ImportException(string message) 
            : base(message)
        {
            
        }

        public ImportException(string message, Exception inner) 
            : base(message, inner)
        {

        }
    }
}
