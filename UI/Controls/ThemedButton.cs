using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SQLServerManager.UI.Themes;

namespace SQLServerManager.UI.Controls
{
    /// <summary>
    /// Themed Button component
    /// Automatically adapts to current theme
    /// Supports 7 button styles: Primary, Success, Warning, Danger, Info, Ghost, Outline
    /// </summary>
    public class ThemedButton : Button
    {
        public enum ButtonStyle
        {
            Primary,    // Blue - main action
            Success,    // Green - positive action
            Warning,    // Orange - caution
            Danger,     // Red - destructive action
            Info,       // Light blue - informational
            Ghost,      // Transparent - subtle action
            Outline     // Border only - secondary action
        }
        
        private ButtonStyle style = ButtonStyle.Primary;
        private bool isHovered = false;
        private bool isPressed = false;
        
        public ThemedButton()
        {
            InitializeButton();
            
            // Subscribe to theme changes
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        }
        
        private void InitializeButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(140, 40);
            this.Cursor = Cursors.Hand;
            this.Font = ThemeManager.Theme.FontButton;
            
            // Enable custom painting
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer, true);
            
            // Mouse events for hover/press effects
            this.MouseEnter += (s, e) => { if (this.Enabled) { isHovered = true; this.Invalidate(); } };
            this.MouseLeave += (s, e) => { isHovered = false; isPressed = false; this.Invalidate(); };
            this.MouseDown += (s, e) => { if (this.Enabled) { isPressed = true; this.Invalidate(); } };
            this.MouseUp += (s, e) => { isPressed = false; this.Invalidate(); };
        }
        
        /// <summary>
        /// Set button style (Primary, Success, Warning, etc.)
        /// </summary>
        public void SetStyle(ButtonStyle newStyle)
        {
            this.style = newStyle;
            this.Invalidate();
        }
        
        private void OnThemeChanged(object sender, EventArgs e)
        {
            this.Font = ThemeManager.Theme.FontButton;
            this.Invalidate();
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            AppTheme theme = ThemeManager.Theme;
            
            // Get colors based on style
            Color bgColor = GetBackgroundColor();
            Color textColor = GetTextColor();
            
            // State modifications
            if (isPressed && this.Enabled)
            {
                bgColor = DarkenColor(bgColor, 20);
            }
            else if (isHovered && this.Enabled)
            {
                bgColor = LightenColor(bgColor, 10);
            }
            
            if (!this.Enabled)
            {
                bgColor = theme.Border;
                textColor = theme.TextDisabled;
            }
            
            // Draw button
            using (GraphicsPath path = GetRoundedRect(this.ClientRectangle, theme.CornerRadius))
            {
                // Shadow (only if not pressed and not Ghost/Outline)
                if (!isPressed && this.Enabled && style != ButtonStyle.Ghost && style != ButtonStyle.Outline)
                {
                    DrawShadow(g, path, theme);
                }
                
                // Background
                if (style == ButtonStyle.Ghost)
                {
                    // Ghost style - transparent, only show on hover
                    if (isHovered && this.Enabled)
                    {
                        using (SolidBrush brush = new SolidBrush(Color.FromArgb(20, bgColor)))
                        {
                            g.FillPath(brush, path);
                        }
                    }
                }
                else if (style == ButtonStyle.Outline)
                {
                    // Outline style - border only
                    using (SolidBrush brush = new SolidBrush(theme.Surface))
                    {
                        g.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(bgColor, 2))
                    {
                        g.DrawPath(pen, path);
                    }
                }
                else
                {
                    // Solid style - filled background
                    using (SolidBrush brush = new SolidBrush(bgColor))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }
            
            // Text
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(this.Text, this.Font, textBrush, this.ClientRectangle, sf);
            }
        }
        
        private Color GetBackgroundColor()
        {
            AppTheme theme = ThemeManager.Theme;
            
            switch (style)
            {
                case ButtonStyle.Primary:
                    return theme.Primary;
                case ButtonStyle.Success:
                    return theme.Success;
                case ButtonStyle.Warning:
                    return theme.Warning;
                case ButtonStyle.Danger:
                    return theme.Danger;
                case ButtonStyle.Info:
                    return theme.Info;
                case ButtonStyle.Ghost:
                case ButtonStyle.Outline:
                    return theme.Primary;
                default:
                    return theme.Primary;
            }
        }
        
        private Color GetTextColor()
        {
            AppTheme theme = ThemeManager.Theme;
            
            if (style == ButtonStyle.Ghost || style == ButtonStyle.Outline)
            {
                return GetBackgroundColor();
            }
            
            // White text for colored buttons
            return Color.White;
        }
        
        private void DrawShadow(Graphics g, GraphicsPath path, AppTheme theme)
        {
            for (int i = 4; i >= 0; i--)
            {
                using (GraphicsPath shadow = (GraphicsPath)path.Clone())
                using (Matrix m = new Matrix())
                {
                    m.Translate(0, i / 2.0f);
                    shadow.Transform(m);
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(10, theme.Shadow)))
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
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
        
        private Color LightenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount)
            );
        }
        
        private Color DarkenColor(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, color.R - amount),
                Math.Max(0, color.G - amount),
                Math.Max(0, color.B - amount)
            );
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