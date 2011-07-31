using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Shared.Types
{
    // Helper type, allows calls to return success/failure as well as a message
    //  to show the user on failure.
    public struct BooleanResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
