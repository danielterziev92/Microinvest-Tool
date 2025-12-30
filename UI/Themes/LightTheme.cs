using System.Drawing;

namespace SQLServerManager.UI.Themes
{
    public class LightTheme : AppTheme
    {
        public override string Name 
        { 
            get { return "Light Professional"; }
        }
        
        public override Color Primary 
        { 
            get { return Color.FromArgb(80, 120, 180); }
        }
        
        public override Color PrimaryHover 
        { 
            get { return Color.FromArgb(100, 140, 200); }
        }
        
        public override Color PrimaryPressed 
        { 
            get { return Color.FromArgb(60, 100, 160); }
        }
        
        public override Color Background 
        { 
            get { return Color.FromArgb(248, 249, 250); }
        }
        
        public override Color Surface 
        { 
            get { return Color.FromArgb(255, 255, 255); }
        }
        
        public override Color SurfaceHover 
        { 
            get { return Color.FromArgb(250, 251, 252); }
        }
        
        public override Color TextPrimary 
        { 
            get { return Color.FromArgb(40, 45, 55); }
        }
        
        public override Color TextSecondary 
        { 
            get { return Color.FromArgb(110, 115, 125); }
        }
        
        public override Color TextDisabled 
        { 
            get { return Color.FromArgb(180, 185, 195); }
        }
        
        public override Color Success 
        { 
            get { return Color.FromArgb(70, 160, 90); }
        }
        
        public override Color Warning 
        { 
            get { return Color.FromArgb(220, 150, 50); }
        }
        
        public override Color Danger 
        { 
            get { return Color.FromArgb(210, 80, 70); }
        }
        
        public override Color Info 
        { 
            get { return Color.FromArgb(90, 130, 200); }
        }
        
        public override Color Border 
        { 
            get { return Color.FromArgb(225, 230, 235); }
        }
        
        public override Color BorderHover 
        { 
            get { return Color.FromArgb(200, 210, 220); }
        }
        
        public override Color SidebarBackground 
        { 
            get { return Color.FromArgb(50, 60, 75); }
        }
        
        public override Color SidebarText 
        { 
            get { return Color.FromArgb(220, 225, 230); }
        }
        
        public override Color SidebarActive 
        { 
            get { return Color.FromArgb(80, 120, 180); }
        }
        
        public override Color SidebarHover 
        { 
            get { return Color.FromArgb(65, 75, 90); }
        }
        
        public override Color Shadow 
        { 
            get { return Color.FromArgb(30, 0, 0, 0); }
        }
    }
}