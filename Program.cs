using System;
using System.Windows.Forms;
using SQLServerManager.UI;

namespace SQLServerManager
{
    /// <summary>
    /// SQL Server Manager Application Entry Point
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Enable visual styles
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                // ============================================================
                // AUTO THEME DETECTION - Match Windows theme
                // ============================================================
                ThemeManager.ApplySystemTheme();
                
                // Log detected theme (optional - for debugging)
                string themeName = ThemeManager.Theme.Name;
                string systemMode = ThemeManager.IsSystemDarkMode() ? "Dark" : "Light";
                
                Console.WriteLine("===========================================");
                Console.WriteLine("SQL Server Manager - Theme System");
                Console.WriteLine("===========================================");
                Console.WriteLine("Windows Theme: " + systemMode + " Mode");
                Console.WriteLine("Applied Theme: " + themeName);
                Console.WriteLine("===========================================");
                
                // ============================================================
                // CREATE AND RUN MAIN FORM
                // ============================================================
                
                // TODO: Replace with your MainForm when ready
                // MainForm mainForm = new MainForm();
                
                // For now, show theme demo
                ShowThemeDemo();
                
                // Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Fatal error: " + ex.Message + "\n\nStack: " + ex.StackTrace,
                    "SQL Server Manager Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        
        /// <summary>
        /// Show theme demo form (temporary - for testing)
        /// </summary>
        private static void ShowThemeDemo()
        {
            Form demoForm = new Form
            {
                Text = "SQL Server Manager - Theme Demo",
                Size = new System.Drawing.Size(800, 600),
                StartPosition = FormStartPosition.CenterScreen
            };
            
            demoForm.BackColor = ThemeManager.Theme.Background;
            
            // Add demo content
            Controls.ThemedLabel lblTitle = new Controls.ThemedLabel
            {
                Text = "SQL Server Manager",
                Font = ThemeManager.Theme.FontTitle,
                Location = new System.Drawing.Point(50, 50),
                AutoSize = true
            };
            demoForm.Controls.Add(lblTitle);
            
            Controls.ThemedLabel lblSubtitle = new Controls.ThemedLabel
            {
                Text = "Theme system is working! ✨",
                Font = ThemeManager.Theme.FontHeading,
                Location = new System.Drawing.Point(50, 100),
                AutoSize = true
            };
            lblSubtitle.SetStyle(Controls.ThemedLabel.LabelStyle.Secondary);
            demoForm.Controls.Add(lblSubtitle);
            
            // Add theme buttons
            int x = 50;
            int y = 150;
            
            Controls.ThemedButton btnLight = new Controls.ThemedButton
            {
                Text = "☀️ Light Theme",
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(150, 40)
            };
            btnLight.SetStyle(Controls.ThemedButton.ButtonStyle.Primary);
            btnLight.Click += (s, e) => ThemeManager.SetLightTheme();
            demoForm.Controls.Add(btnLight);
            
            x += 160;
            Controls.ThemedButton btnDark = new Controls.ThemedButton
            {
                Text = "🌙 Dark Theme",
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(150, 40)
            };
            btnDark.SetStyle(Controls.ThemedButton.ButtonStyle.Primary);
            btnDark.Click += (s, e) => ThemeManager.SetDarkTheme();
            demoForm.Controls.Add(btnDark);
            
            x += 160;
            Controls.ThemedButton btnSystem = new Controls.ThemedButton
            {
                Text = "🖥️ System Theme",
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(150, 40)
            };
            btnSystem.SetStyle(Controls.ThemedButton.ButtonStyle.Info);
            btnSystem.Click += (s, e) => ThemeManager.ApplySystemTheme();
            demoForm.Controls.Add(btnSystem);
            
            // Add settings button
            y += 60;
            Controls.ThemedButton btnSettings = new Controls.ThemedButton
            {
                Text = "⚙️ Theme Settings",
                Location = new System.Drawing.Point(50, y),
                Size = new System.Drawing.Size(180, 40)
            };
            btnSettings.SetStyle(Controls.ThemedButton.ButtonStyle.Outline);
            btnSettings.Click += (s, e) =>
            {
                Forms.ThemeSettingsDialog dialog = new Forms.ThemeSettingsDialog();
                dialog.ShowDialog();
            };
            demoForm.Controls.Add(btnSettings);
            
            // Add demo card
            y += 70;
            Controls.ThemedCard card = new Controls.ThemedCard
            {
                Location = new System.Drawing.Point(50, y),
                Size = new System.Drawing.Size(680, 150),
                Elevated = true
            };
            
            Controls.ThemedLabel cardTitle = new Controls.ThemedLabel
            {
                Text = "📊 Sample Card",
                Font = ThemeManager.Theme.FontHeading,
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            card.Controls.Add(cardTitle);
            
            Controls.ThemedLabel cardText = new Controls.ThemedLabel
            {
                Text = "This is a themed card component. It automatically adapts to the current theme.\nTry switching themes to see how everything updates instantly!",
                Location = new System.Drawing.Point(20, 55),
                Size = new System.Drawing.Size(640, 50)
            };
            cardText.SetStyle(Controls.ThemedLabel.LabelStyle.Secondary);
            card.Controls.Add(cardText);
            
            demoForm.Controls.Add(card);
            
            // Info text
            y += 170;
            Controls.ThemedLabel lblInfo = new Controls.ThemedLabel
            {
                Text = "💡 Replace this demo with your MainForm in Program.cs",
                Location = new System.Drawing.Point(50, y),
                AutoSize = true
            };
            lblInfo.SetStyle(Controls.ThemedLabel.LabelStyle.Secondary);
            demoForm.Controls.Add(lblInfo);
            
            // Subscribe to theme changes to update form
            ThemeManager.Instance.ThemeChanged += (s, e) =>
            {
                demoForm.BackColor = ThemeManager.Theme.Background;
            };
            
            Application.Run(demoForm);
        }
    }
}