using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Logging;
using pGina.Shared.Settings;

namespace pGina.Core
{
    public static class Framework
    {
        public static void Init()
        {            
            Logging.Init();
            Settings.Init();
        }
    }
}
