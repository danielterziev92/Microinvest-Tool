using System;
using System.Drawing;
using System.Windows.Forms;
using SQLServerManager.UI.Themes;

namespace SQLServerManager.UI.Controls
{
    /// <summary>
    /// Themed Panel component
    /// Simple panel with automatic background color from theme
    /// Supports 2 styles: Surface (for cards) and Background (for pages)
    /// </summary>
    public class ThemedPanel : Panel
    {
        public enum PanelStyle
        {
            Surface,     // Card/surface color (white in light, gray in dark)
            Background   // Page background color (off-white in light, dark gray in dark)
        }
        
        private PanelStyle panelStyle = PanelStyle.Surface;
        
        public ThemedPanel()
        {
            ApplyTheme();
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        }
        
        /// <summary>
        /// Set panel style (Surface or Background)
        /// </summary>
        public void SetPanelStyle(PanelStyle newStyle)
        {
            this.panelStyle = newStyle;
            ApplyTheme();
        }
        
        private void OnThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }
        
        private void ApplyTheme()
        {
            AppTheme theme = ThemeManager.Theme;
            
            switch (panelStyle)
            {
                case PanelStyle.Surface:
                    this.BackColor = theme.Surface;
                    break;
                case PanelStyle.Background:
                    this.BackColor = theme.Background;
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