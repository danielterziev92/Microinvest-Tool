using System;
using System.Drawing;
using System.Windows.Forms;

public class TabBuilder
{
    private TabPage tab;
    private Panel content;
    private int yPos = 0;
    
    public TabBuilder(TabPage tabPage)
    {
        this.tab = tabPage;
        tab.BackColor = Color.White;
        
        content = new Panel();
        content.Dock = DockStyle.Fill;
        content.AutoScroll = true;
        content.Padding = new Padding(30, 20, 30, 20);
        tab.Controls.Add(content);
    }
    
    public TabBuilder AddHeader(string title, string subtitle = null)
    {
        Label h = new Label();
        h.Text = title;
        h.Font = AppConstants.Fonts.Heading;
        h.ForeColor = AppConstants.Colors.TextPrimary;
        h.Location = new Point(0, yPos);
        h.AutoSize = true;
        content.Controls.Add(h);
        yPos += 35;
        
        if (!string.IsNullOrEmpty(subtitle))
        {
            Label s = new Label();
            s.Text = subtitle;
            s.Font = AppConstants.Fonts.Normal;
            s.ForeColor = AppConstants.Colors.TextSecondary;
            s.Location = new Point(0, yPos);
            s.Size = new Size(750, 30);
            content.Controls.Add(s);
            yPos += 35;
        }
        
        return this;
    }
    
    public TabBuilder AddSection(string title)
    {
        yPos += 10;
        Label h = new Label();
        h.Text = title;
        h.Font = AppConstants.Fonts.Subheading;
        h.ForeColor = AppConstants.Colors.Primary;
        h.Location = new Point(0, yPos);
        h.AutoSize = true;
        content.Controls.Add(h);
        yPos += 30;
        
        Panel sep = new Panel();
        sep.BackColor = Color.FromArgb(220, 220, 220);
        sep.Location = new Point(0, yPos);
        sep.Size = new Size(800, 1);
        content.Controls.Add(sep);
        yPos += 15;
        
        return this;
    }
    
    public TabBuilder AddRow(string label, string value)
    {
        Label l = new Label();
        l.Text = label;
        l.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        l.Location = new Point(20, yPos);
        l.Size = new Size(180, 25);
        content.Controls.Add(l);
        
        Label v = new Label();
        v.Text = value ?? "Unknown";
        v.Font = AppConstants.Fonts.Normal;
        v.ForeColor = AppConstants.Colors.TextSecondary;
        v.Location = new Point(220, yPos);
        v.Size = new Size(600, 25);
        content.Controls.Add(v);
        
        yPos += 35;
        return this;
    }
    
    public TabBuilder AddButton(string text, EventHandler onClick, ThemedButton.Style style = ThemedButton.ButtonStyle.Primary)
    {
        ThemedButton btn = new ThemedButton();
        btn.Text = text;
        btn.SetStyle(style);
        btn.Location = new Point(20, yPos);
        btn.Click += onClick;
        content.Controls.Add(btn);
        yPos += 50;
        return this;
    }
    
    public TabBuilder AddControl(Control control)
    {
        control.Location = new Point(0, yPos);
        content.Controls.Add(control);
        yPos += control.Height + 15;
        return this;
    }
    
    public TabBuilder AddSpace(int height)
    {
        yPos += height;
        return this;
    }
    
    public Panel GetContent()
    {
        return content;
    }
}