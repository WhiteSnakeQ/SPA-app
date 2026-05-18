namespace SPA_приложение.Constants
{
    public static class CaptchaConstants
    {
        public const int Width = 220;
        public const int Height = 100;
        public const int GrayPoint = 1200;

        public const string captchaChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        public static class Letters
        {
            public const int MaxY = 50;
            public const int MinY = 30;
            public const int StartX = 20;
            public const int indentX = 30;
            public const int angleMin = -5;
            public const int angleMax = 5;
        }
        
        public static class Lines
        {
            public const int LineCount = 7;
            public const int fPointX = Width / 10;
            public const int lPointX = Width - Width / 10;

            public const int PointYMin = Height / 10;
            public const int PointYMax = Height - Height / 10;
        }
    }
}
