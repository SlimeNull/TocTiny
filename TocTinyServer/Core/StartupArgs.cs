using System;
using System.Collections.Generic;
using System.Text;
using TocTiny.Public;

namespace TocTiny.Server.Core
{
    public class StartupArgs
    {
        public string Port = "0";           // port 
        public string Backlog = "0";          // backlog
        public string BufferTimeout = "0";
        public string CleanInterval = "0";

        public string P = "0";
        public string B = "0";
        public string BT = "0";       // buffer timeout : 缓冲区存活时间 (ms)
        public string CI = "0";      // cleaning interval : 内存清理间隔 (ms)

        public string NAME = string.Empty;

        public bool NoCmd = false;

        public bool NC;


        public bool Help = false;
        public bool H = false;

        public ExecuteArgs DeepParse(bool checkHelp = true)
        {
            bool argsIntegerCorrect =
                int.TryParse(this.Port, out int Port) &
                int.TryParse(this.Backlog, out int Backlog) &
                int.TryParse(this.BufferTimeout, out int BufferTimeout) &
                int.TryParse(this.CleanInterval, out int CleanInterval) &
                int.TryParse(P, out int Int_P) &
                int.TryParse(B, out int Int_B) &
                int.TryParse(BT, out int Int_BT) &
                int.TryParse(CI, out int Int_CI),
                PortUndefined = Port == 0 && Int_P == 0,
                BacklogUndefined = Backlog == 0 && Int_B == 0,
                BufferTimeoutUndefined = BufferTimeout == 0 && Int_BT == 0,
                CleanIntervalUndefined = CleanInterval == 0 && Int_CI == 0,
                NoCmd = this.NoCmd || NC;

            if (!argsIntegerCorrect)
                ExFunc.ErrorExit("参数需要整数, 但指定了非整数值", -1);

            if (PortUndefined)
                Port = 2020;
            if (BacklogUndefined)
                Backlog = 50;
            if (BufferTimeoutUndefined)
                BufferTimeout = 2000;
            if (CleanIntervalUndefined)
                CleanInterval = 1000;

            return new ExecuteArgs()
            {
                Port = Port + Int_P,
                Backlog = Backlog + Int_B,
                BufferTimeout = BufferTimeout + Int_BT,
                CleanInterval = CleanInterval + Int_CI,
                NoCommand = NoCmd
            };
        }
    }
}
