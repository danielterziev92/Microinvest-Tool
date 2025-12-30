using System;
using System.Drawing;
using System.Windows.Forms;
using SQLServerManager.UI.Themes;

namespace SQLServerManager.UI.Controls
{
    /// <summary>
    /// Themed Label component
    /// Automatically adapts text color to current theme
    /// Supports 6 label styles: Primary, Secondary, Disabled, Success, Warning, Danger
    /// </summary>
    public class ThemedLabel : Label
    {
        public enum LabelStyle
        {
            Primary,    // Main text color
            Secondary,  // Gray text for less important info
            Disabled,   // Disabled gray text
            Success,    // Green text for positive messages
            Warning,    // Orange text for warnings
            Danger      // Red text for errors/dangers
        }
        
        private LabelStyle style = LabelStyle.Primary;
        
        public ThemedLabel()
        {
            ApplyTheme();
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        }
        
        /// <summary>
        /// Set label style (Primary, Secondary, Success, etc.)
        /// </summary>
        public void SetStyle(LabelStyle newStyle)
        {
            this.style = newStyle;
            ApplyTheme();
        }
        
        private void OnThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }
        
        private void ApplyTheme()
        {
            AppTheme theme = ThemeManager.Theme;
            
            switch (style)
            {
                case LabelStyle.Primary:
                    this.ForeColor = theme.TextPrimary;
                    break;
                case LabelStyle.Secondary:
                    this.ForeColor = theme.TextSecondary;
                    break;
                case LabelStyle.Disabled:
                    this.ForeColor = theme.TextDisabled;
                    break;
                case LabelStyle.Success:
                    this.ForeColor = theme.Success;
                    break;
                case LabelStyle.Warning:
                    this.ForeColor = theme.Warning;
                    break;
                case LabelStyle.Danger:
                    this.ForeColor = theme.Danger;
                    break;
            }
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