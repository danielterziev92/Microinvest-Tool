using System;
using System.Drawing;
using System.Windows.Forms;
using SQLServerManager.UI.Controls;
using SQLServerManager.UI.Themes;

namespace SQLServerManager.UI.Forms
{
    /// <summary>
    /// Theme Settings Dialog
    /// Allows user to choose between System, Light, and Dark themes
    /// Shows live preview of selected theme
    /// </summary>
    public class ThemeSettingsDialog : Form
    {
        private RadioButton radioSystem;
        private RadioButton radioLight;
        private RadioButton radioDark;
        private ThemedButton btnApply;
        private ThemedButton btnCancel;
        private ThemedPanel previewPanel;
        private ThemedLabel lblPreview;
        private ThemedButton previewBtn;
        
        public ThemeSettingsDialog()
        {
            InitializeDialog();
        }
        
        private void InitializeDialog()
        {
            this.Text = "Theme Settings";
            this.Size = new Size(500, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            AppTheme theme = ThemeManager.Theme;
            this.BackColor = theme.Background;
            
            // HEADER
            ThemedLabel header = new ThemedLabel
            {
                Text = "🎨 Choose Your Theme",
                Font = theme.FontHeading,
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(header);
            
            ThemedLabel subtitle = new ThemedLabel
            {
                Text = "Select a theme that's comfortable for your work",
                Location = new Point(20, 55),
                AutoSize = true
            };
            subtitle.SetStyle(ThemedLabel.LabelStyle.Secondary);
            this.Controls.Add(subtitle);
            
            // THEME OPTIONS
            int y = 100;
            
            radioSystem = CreateThemeOption(
                "🖥️ System Default",
                "Match Windows theme (automatically switches with Windows)",
                ref y
            );
            
            radioLight = CreateThemeOption(
                "☀️ Light Theme",
                "Warm colors, comfortable for daytime work",
                ref y
            );
            
            radioDark = CreateThemeOption(
                "🌙 Dark Theme",
                "Easy on eyes, perfect for night work and long sessions",
                ref y
            );
            
            // Select current theme
            SelectCurrentTheme();
            
            // PREVIEW PANEL
            y += 10;
            previewPanel = new ThemedPanel
            {
                Location = new Point(20, y),
                Size = new Size(440, 90)
            };
            previewPanel.SetPanelStyle(ThemedPanel.PanelStyle.Surface);
            
            ThemedLabel previewTitle = new ThemedLabel
            {
                Text = "Preview:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            previewPanel.Controls.Add(previewTitle);
            
            lblPreview = new ThemedLabel
            {
                Text = "This is how text will look in the selected theme",
                Font = theme.FontNormal,
                Location = new Point(10, 32),
                AutoSize = true
            };
            previewPanel.Controls.Add(lblPreview);
            
            previewBtn = new ThemedButton
            {
                Text = "Sample Button",
                Location = new Point(10, 55),
                Size = new Size(130, 28)
            };
            previewBtn.SetStyle(ThemedButton.ButtonStyle.Primary);
            previewPanel.Controls.Add(previewBtn);
            
            this.Controls.Add(previewPanel);
            
            // ACTION BUTTONS
            btnApply = new ThemedButton
            {
                Text = "Apply Theme",
                Size = new Size(120, 35),
                Location = new Point(250, 300)
            };
            btnApply.SetStyle(ThemedButton.ButtonStyle.Success);
            btnApply.Click += BtnApply_Click;
            this.Controls.Add(btnApply);
            
            btnCancel = new ThemedButton
            {
                Text = "Cancel",
                Size = new Size(100, 35),
                Location = new Point(380, 300)
            };
            btnCancel.SetStyle(ThemedButton.ButtonStyle.Outline);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
            
            // PREVIEW ON SELECTION CHANGE
            radioSystem.CheckedChanged += (s, e) => { if (radioSystem.Checked) PreviewTheme("System"); };
            radioLight.CheckedChanged += (s, e) => { if (radioLight.Checked) PreviewTheme("Light"); };
            radioDark.CheckedChanged += (s, e) => { if (radioDark.Checked) PreviewTheme("Dark"); };
            
            // Trigger initial preview
            if (radioSystem.Checked) PreviewTheme("System");
            else if (radioLight.Checked) PreviewTheme("Light");
            else if (radioDark.Checked) PreviewTheme("Dark");
        }
        
        private RadioButton CreateThemeOption(string title, string description, ref int y)
        {
            RadioButton radio = new RadioButton
            {
                Location = new Point(20, y),
                Size = new Size(20, 20),
                Cursor = Cursors.Hand
            };
            this.Controls.Add(radio);
            
            ThemedLabel lblTitle = new ThemedLabel
            {
                Text = title,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(50, y - 2),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            lblTitle.Click += (s, e) => radio.Checked = true;
            this.Controls.Add(lblTitle);
            
            ThemedLabel lblDesc = new ThemedLabel
            {
                Text = description,
                Location = new Point(50, y + 20),
                Size = new Size(410, 30),
                AutoSize = false
            };
            lblDesc.SetStyle(ThemedLabel.LabelStyle.Secondary);
            lblDesc.Click += (s, e) => radio.Checked = true;
            this.Controls.Add(lblDesc);
            
            y += 50;
            
            return radio;
        }
        
        private void SelectCurrentTheme()
        {
            string currentName = ThemeManager.Theme.Name;
            
            if (currentName == "Light Professional")
            {
                radioLight.Checked = true;
            }
            else if (currentName == "Dark Professional")
            {
                radioDark.Checked = true;
            }
            else
            {
                radioSystem.Checked = true;
            }
        }
        
        private void PreviewTheme(string themeName)
        {
            AppTheme previewTheme = null;
            
            switch (themeName)
            {
                case "System":
                    previewTheme = ThemeManager.GetSystemTheme();
                    break;
                case "Light":
                    previewTheme = new LightTheme();
                    break;
                case "Dark":
                    previewTheme = new DarkTheme();
                    break;
            }
            
            if (previewTheme != null)
            {
                // Update preview panel
                previewPanel.BackColor = previewTheme.Surface;
                
                // Update all controls in preview
                foreach (Control ctrl in previewPanel.Controls)
                {
                    if (ctrl is ThemedLabel lbl)
                    {
                        lbl.ForeColor = previewTheme.TextPrimary;
                    }
                }
                
                // Force redraw of preview button
                if (previewBtn != null)
                {
                    previewBtn.Invalidate();
                }
            }
        }
        
        private void BtnApply_Click(object sender, EventArgs e)
        {
            if (radioSystem.Checked)
            {
                ThemeManager.ApplySystemTheme();
                MessageBox.Show(
                    "Theme set to System Default.\n\nThe app will now match your Windows theme.",
                    "Theme Applied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else if (radioLight.Checked)
            {
                ThemeManager.SetLightTheme();
                MessageBox.Show(
                    "Light theme applied successfully!",
                    "Theme Applied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else if (radioDark.Checked)
            {
                ThemeManager.SetDarkTheme();
                MessageBox.Show(
                    "Dark theme applied successfully!",
                    "Theme Applied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            
            this.Close();
        }
    }
}