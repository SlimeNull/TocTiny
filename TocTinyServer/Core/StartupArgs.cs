using System;
using System.Collections.Generic;
using System.Text;
using TocTiny.Public;

namespace TocTiny.Server.Core
{
    public class StartupArgs
    {
        public string PORT = "0";           // port 
        public string BUFFERSIZE = "0";      // buffer
        public string BACKLOG = "0";          // backlog
        public string BUFFERTIMEOUT = "0";
        public string CLEANINTERVAL = "0";

        public string P = "0";
        public string BS = "0";
        public string B = "0";
        public string BT = "0";       // buffer timeout : 缓冲区存活时间 (ms)
        public string CI = "0";      // cleaning interval : 内存清理间隔 (ms)

        public string NAME = string.Empty;

        public bool NOCMD = false;

        public bool NC;


        public bool HELP = false;
        public bool H = false;

        public ExecuteArgs DeepParse(bool checkHelp = true)
        {
            bool argsIntegerCorrect =
                int.TryParse(PORT, out int Port) &
                int.TryParse(BUFFERSIZE, out int BufferSize) &
                int.TryParse(BACKLOG, out int Backlog) &
                int.TryParse(BUFFERTIMEOUT, out int BufferTimeout) &
                int.TryParse(CLEANINTERVAL, out int CleanInterval) &
                int.TryParse(P, out int Int_P) &
                int.TryParse(BS, out int Int_BS) &
                int.TryParse(B, out int Int_B) &
                int.TryParse(BT, out int Int_BT) &
                int.TryParse(CI, out int Int_CI),
                PortUndefined = Port == 0 && Int_P == 0,
                BufferSizeUndefined = BufferSize == 0 && Int_BS == 0,
                BacklogUndefined = Backlog == 0 && Int_B == 0,
                BufferTimeoutUndefined = BufferTimeout == 0 && Int_BT == 0,
                CleanIntervalUndefined = CleanInterval == 0 && Int_CI == 0,
                NoCmd = NOCMD || NC;

            if (argsIntegerCorrect)
                ExFunc.ErrorExit("参数需要整数, 但指定了非整数值", -1);

            if (PortUndefined)
                Port = 2020;
            if (BufferSizeUndefined)
                BufferSize = 1048576;
            if (BacklogUndefined)
                Backlog = 50;
            if (BufferTimeoutUndefined)
                BufferTimeout = 2000;
            if (CleanIntervalUndefined)
                CleanInterval = 1000;

            return new ExecuteArgs()
            {
                Port = Port + Int_P,
                BufferSize = BufferSize + Int_BS,
                Backlog = Backlog + Int_B,
                BufferTimeout = BufferTimeout + Int_BS,
                CleanInterval = CleanInterval + Int_CI,
                NoCommand = NoCmd
            };
        }
    }
}
