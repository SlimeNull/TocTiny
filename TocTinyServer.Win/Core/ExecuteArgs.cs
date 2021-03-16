using System;
using System.Collections.Generic;
using System.Text;

namespace TocTiny.Server.Core
{
    public class ExecuteArgs
    {
        public int Port;
        public int BufferSize;
        public int Backlog;
        public int BufferTimeout;
        public int CleanInterval;
        public bool NoCommand;
    }
}
