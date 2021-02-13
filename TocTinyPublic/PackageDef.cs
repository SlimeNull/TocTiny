namespace TocTiny.Public
{
    public static class ConstDef
    {
        public const int NormalMessage       = 0b_0000000000000001;   // 普通消息                    | 直接广播
        public const int Verification        = 0b_0000000000000010;   // 验证消息 (某人加入时会发送)    | 直接广播
        public const int ImageMessage        = 0b_0000000000000011;   // 图像消息                    | 直接广播
        public const int DrawAttention       = 0b_0000000000000100;   // 吸引注意力                  | 直接广播
        public const int HeartPackage        = 0b_1000000000000001;   // 心跳包 (已经确认不可少        | 不需处理
        public const int ChangeChannelName   = 0b_1000000000000010;   // 改变频道名                  | 广播消息
        public const int ReportChannelOnline = 0b_1000000000000011;   // 报告这个频道中人数            | 回应消息
    }
    public class TransPackage
    {
        public string Name;
        public string Content;
        public string ClientGuid;

        public int PackageType;
    }
}
