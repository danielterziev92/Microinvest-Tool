using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SQLServerManager.UI.Themes;

namespace SQLServerManager.UI.Controls
{
    /// <summary>
    /// Themed Card component
    /// Panel with rounded corners, shadow, and automatic theme adaptation
    /// Supports elevation (shadow) and hover effects
    /// </summary>
    public class ThemedCard : Panel
    {
        private bool elevated = true;
        private bool hoverable = false;
        private bool isHovered = false;
        
        public ThemedCard()
        {
            InitializeCard();
            
            // Subscribe to theme changes
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        }
        
        private void InitializeCard()
        {
            AppTheme theme = ThemeManager.Theme;
            
            this.BackColor = theme.Surface;
            this.Padding = new Padding(theme.Padding);
            
            // Enable custom painting
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer, true);
            
            // Hover effects
            this.MouseEnter += (s, e) => { if (hoverable) { isHovered = true; this.Invalidate(); } };
            this.MouseLeave += (s, e) => { isHovered = false; this.Invalidate(); };
        }
        
        /// <summary>
        /// Enable or disable shadow elevation
        /// </summary>
        public bool Elevated
        {
            get { return elevated; }
            set { elevated = value; this.Invalidate(); }
        }
        
        /// <summary>
        /// Enable or disable hover effect
        /// </summary>
        public bool Hoverable
        {
            get { return hoverable; }
            set
            {
                hoverable = value;
                this.Cursor = value ? Cursors.Hand : Cursors.Default;
            }
        }
        
        private void OnThemeChanged(object sender, EventArgs e)
        {
            AppTheme theme = ThemeManager.Theme;
            this.BackColor = theme.Surface;
            this.Padding = new Padding(theme.Padding);
            this.Invalidate();
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            AppTheme theme = ThemeManager.Theme;
            
            using (GraphicsPath path = GetRoundedRect(this.ClientRectangle, theme.CornerRadius))
            {
                // Shadow
                if (elevated)
                {
                    DrawShadow(g, path, theme);
                }
                
                // Background
                Color bgColor = isHovered ? theme.SurfaceHover : theme.Surface;
                using (SolidBrush bg = new SolidBrush(bgColor))
                {
                    g.FillPath(bg, path);
                }
                
                // Border
                using (Pen border = new Pen(theme.Border, 1))
                {
                    g.DrawPath(border, path);
                }
            }
        }
        
        private void DrawShadow(Graphics g, GraphicsPath path, AppTheme theme)
        {
            for (int i = 6; i >= 0; i--)
            {
                using (GraphicsPath shadow = (GraphicsPath)path.Clone())
                using (Matrix m = new Matrix())
                {
                    m.Translate(0, i / 2.0f);
                    shadow.Transform(m);
                    int alpha = 5 + (6 - i) * 2;
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(alpha, theme.Shadow)))
                    {
                        g.FillPath(sb, shadow);
                    }
                }
            }
        }
        
        private GraphicsPath GetRoundedRect(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            int d = radius * 2;
            r.Inflate(-1, -1);
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
            }
            base.Dispose(disposing);
        }
    }
}