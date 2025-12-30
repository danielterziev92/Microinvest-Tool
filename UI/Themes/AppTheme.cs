using System.Drawing;

namespace SQLServerManager.UI.Themes
{
    public abstract class AppTheme
    {
        public abstract string Name { get; }
        
        public abstract Color Primary { get; }
        public abstract Color PrimaryHover { get; }
        public abstract Color PrimaryPressed { get; }
        
        public abstract Color Background { get; }
        public abstract Color Surface { get; }
        public abstract Color SurfaceHover { get; }
        
        public abstract Color TextPrimary { get; }
        public abstract Color TextSecondary { get; }
        public abstract Color TextDisabled { get; }
        
        public abstract Color Success { get; }
        public abstract Color Warning { get; }
        public abstract Color Danger { get; }
        public abstract Color Info { get; }
        
        public abstract Color Border { get; }
        public abstract Color BorderHover { get; }
        
        public abstract Color SidebarBackground { get; }
        public abstract Color SidebarText { get; }
        public abstract Color SidebarActive { get; }
        public abstract Color SidebarHover { get; }
        
        public abstract Color Shadow { get; }
        
        public virtual Font FontTitle 
        { 
            get { return new Font("Segoe UI", 24F, FontStyle.Bold); }
        }
        
        public virtual Font FontHeading 
        { 
            get { return new Font("Segoe UI", 18F, FontStyle.Bold); }
        }
        
        public virtual Font FontSubheading 
        { 
            get { return new Font("Segoe UI", 14F, FontStyle.Bold); }
        }
        
        public virtual Font FontNormal 
        { 
            get { return new Font("Segoe UI", 10F); }
        }
        
        public virtual Font FontSmall 
        { 
            get { return new Font("Segoe UI", 9F); }
        }
        
        public virtual Font FontButton 
        { 
            get { return new Font("Segoe UI", 11F, FontStyle.Bold); }
        }
        
        public virtual int CornerRadius 
        { 
            get { return 8; }
        }
        
        public virtual int Padding 
        { 
            get { return 20; }
        }
        
        public virtual int SmallPadding 
        { 
            get { return 10; }
        }
    }
}