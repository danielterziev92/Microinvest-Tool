using System.Drawing;

public static class AppConstants
{
    public static class Colors
    {
        public static readonly Color Primary = Color.FromArgb(0, 122, 255);
        public static readonly Color PrimaryLight = Color.FromArgb(52, 170, 255);
        public static readonly Color PrimaryDark = Color.FromArgb(0, 92, 230);
        
        public static readonly Color Success = Color.FromArgb(52, 199, 89);
        public static readonly Color SuccessLight = Color.FromArgb(90, 220, 120);
        
        public static readonly Color Warning = Color.FromArgb(255, 149, 0);
        public static readonly Color WarningLight = Color.FromArgb(255, 179, 64);
        
        public static readonly Color Danger = Color.FromArgb(255, 59, 48);
        public static readonly Color DangerLight = Color.FromArgb(255, 99, 88);
        
        public static readonly Color Info = Color.FromArgb(88, 86, 214);
        public static readonly Color InfoLight = Color.FromArgb(120, 118, 240);
        
        public static readonly Color Background = Color.FromArgb(242, 242, 247);
        public static readonly Color CardBackground = Color.FromArgb(255, 255, 255);
        public static readonly Color GlassBackground = Color.FromArgb(250, 250, 255, 255);
        
        public static readonly Color TextPrimary = Color.FromArgb(0, 0, 0);
        public static readonly Color TextSecondary = Color.FromArgb(142, 142, 147);
        public static readonly Color TextTertiary = Color.FromArgb(174, 174, 178);
        
        public static readonly Color Accent1 = Color.FromArgb(255, 45, 85);
        public static readonly Color Accent2 = Color.FromArgb(90, 200, 250);
        public static readonly Color Accent3 = Color.FromArgb(175, 82, 222);
        
        public static readonly Color Shadow = Color.FromArgb(40, 0, 0, 0);
    }
    
    public static class Fonts
    {
        public static readonly Font Title = new Font("Segoe UI", 24F, FontStyle.Bold);
        public static readonly Font Heading = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font Subheading = new Font("Segoe UI", 14F, FontStyle.Bold);
        public static readonly Font Normal = new Font("Segoe UI", 10F);
        public static readonly Font Small = new Font("Segoe UI", 9F);
        public static readonly Font Button = new Font("Segoe UI", 11F, FontStyle.Bold);
    }
    
    public static class Sizes
    {
        public static readonly int HeaderHeight = 120;
        public static readonly int FooterHeight = 80;
        public static readonly int ButtonWidth = 140;
        public static readonly int ButtonHeight = 44;
        public static readonly int Padding = 20;
        public static readonly int CardPadding = 16;
        public static readonly int CornerRadius = 12;
    }
}