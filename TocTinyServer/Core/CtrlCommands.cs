using System;
using System.Collections.Generic;
using System.Text;

namespace TocTiny.Server.Core
{
    class CtrlCommands
    {
        public TocTinyServer Server { get; private set; }
        public CtrlCommands(TocTinyServer server)
        {
            Server = server;
        }

        public void Help()
        {

        }
    }
}
