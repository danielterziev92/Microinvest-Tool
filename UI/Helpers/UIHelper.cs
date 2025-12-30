using System;
using System.Drawing;
using System.Windows.Forms;

public static class UIHelper
{
    public static Size GetOptimalFormSize()
    {
        Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
        
        int width = Math.Min(1100, (int)(workingArea.Width * 0.85));
        int height = Math.Min(750, (int)(workingArea.Height * 0.85));
        
        return new Size(width, height);
    }
    
    public static Panel CreateHeaderPanel(string title, string subtitle)
    {
        Panel panel = new Panel();
        panel.Dock = DockStyle.Top;
        panel.Height = AppConstants.Sizes.HeaderHeight;
        panel.BackColor = AppConstants.Colors.Primary;
        
        Label titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.Font = AppConstants.Fonts.Title;
        titleLabel.ForeColor = Color.White;
        titleLabel.Location = new Point(AppConstants.Sizes.Padding, 20);
        titleLabel.AutoSize = true;
        panel.Controls.Add(titleLabel);
        
        Label subtitleLabel = new Label();
        subtitleLabel.Text = subtitle;
        subtitleLabel.Font = AppConstants.Fonts.Normal;
        subtitleLabel.ForeColor = Color.FromArgb(220, 220, 220);
        subtitleLabel.Location = new Point(AppConstants.Sizes.Padding, 60);
        subtitleLabel.AutoSize = true;
        panel.Controls.Add(subtitleLabel);
        
        return panel;
    }
    
    public static Label CreateSectionLabel(string text, int x, int y)
    {
        Label label = new Label();
        label.Text = text;
        label.Font = AppConstants.Fonts.Heading;
        label.ForeColor = AppConstants.Colors.TextPrimary;
        label.Location = new Point(x, y);
        label.AutoSize = true;
        
        return label;
    }
    
    public static ThemedButton CreateButton(string text, ThemedButton.Style style)
    {
        ThemedButton button = new ThemedButton();
        button.Text = text;
        button.SetStyle(style);
        
        return button;
    }
    
    public static ListView CreateInstanceListView(int x, int y, int width, int height)
    {
        ListView listView = new ListView();
        listView.Location = new Point(x, y);
        listView.Size = new Size(width, height);
        listView.View = View.Details;
        listView.FullRowSelect = true;
        listView.GridLines = true;
        listView.BackColor = Color.White;
        listView.Font = AppConstants.Fonts.Normal;
        listView.MultiSelect = false;
        
        listView.Columns.Add("Instance Name", 180);
        listView.Columns.Add("Version", 150);
        listView.Columns.Add("Edition", 120);
        listView.Columns.Add("Service", 150);
        listView.Columns.Add("Status", 100);
        listView.Columns.Add("TCP/IP", 100);
        listView.Columns.Add("Named Pipes", 100);
        
        return listView;
    }
}