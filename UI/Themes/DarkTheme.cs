using System.Drawing;

namespace SQLServerManager.UI.Themes
{
    public class DarkTheme : AppTheme
    {
        public override string Name 
        { 
            get { return "Dark Professional"; }
        }
        
        public override Color Primary 
        { 
            get { return Color.FromArgb(100, 180, 255); }
        }
        
        public override Color PrimaryHover 
        { 
            get { return Color.FromArgb(120, 200, 255); }
        }
        
        public override Color PrimaryPressed 
        { 
            get { return Color.FromArgb(80, 160, 235); }
        }
        
        public override Color Background 
        { 
            get { return Color.FromArgb(24, 26, 30); }
        }
        
        public override Color Surface 
        { 
            get { return Color.FromArgb(32, 35, 40); }
        }
        
        public override Color SurfaceHover 
        { 
            get { return Color.FromArgb(40, 43, 48); }
        }
        
        public override Color TextPrimary 
        { 
            get { return Color.FromArgb(230, 235, 240); }
        }
        
        public override Color TextSecondary 
        { 
            get { return Color.FromArgb(160, 165, 175); }
        }
        
        public override Color TextDisabled 
        { 
            get { return Color.FromArgb(100, 105, 115); }
        }
        
        public override Color Success 
        { 
            get { return Color.FromArgb(80, 200, 120); }
        }
        
        public override Color Warning 
        { 
            get { return Color.FromArgb(255, 180, 60); }
        }
        
        public override Color Danger 
        { 
            get { return Color.FromArgb(255, 100, 90); }
        }
        
        public override Color Info 
        { 
            get { return Color.FromArgb(100, 180, 255); }
        }
        
        public override Color Border 
        { 
            get { return Color.FromArgb(55, 60, 70); }
        }
        
        public override Color BorderHover 
        { 
            get { return Color.FromArgb(75, 80, 90); }
        }
        
        public override Color SidebarBackground 
        { 
            get { return Color.FromArgb(18, 20, 24); }
        }
        
        public override Color SidebarText 
        { 
            get { return Color.FromArgb(200, 205, 210); }
        }
        
        public override Color SidebarActive 
        { 
            get { return Color.FromArgb(100, 180, 255); }
        }
        
        public override Color SidebarHover 
        { 
            get { return Color.FromArgb(28, 30, 35); }
        }
        
        public override Color Shadow 
        { 
            get { return Color.FromArgb(50, 0, 0, 0); }
        }
    }
}