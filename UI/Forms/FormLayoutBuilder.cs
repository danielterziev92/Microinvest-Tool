using System;
using System.Drawing;
using System.Windows.Forms;

public class FormLayoutBuilder
{
    private readonly Control _parent;
    private int _yPos;
    
    public FormLayoutBuilder(Control parent, int startY)
    {
        _parent = parent;
        _yPos = startY;
    }
    
    public FormLayoutBuilder AddSection(string title)
    {
        _parent.Controls.Add(new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = AppConstants.Colors.Primary,
            Location = new Point(20, _yPos),
            AutoSize = true
        });
        
        _parent.Controls.Add(new Panel
        {
            BackColor = Color.FromArgb(220, 220, 220),
            Location = new Point(20, _yPos + 28),
            Size = new Size(850, 1)
        });
        
        _yPos += 40;
        return this;
    }
    
    public FormLayoutBuilder AddRow(string label, string value)
    {
        _parent.Controls.AddRange(new Control[]
        {
            new Label 
            { 
                Text = label, 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(30, _yPos), 
                Size = new Size(180, 25) 
            },
            new Label 
            { 
                Text = value, 
                Font = AppConstants.Fonts.Normal,
                ForeColor = AppConstants.Colors.TextSecondary,
                Location = new Point(220, _yPos), 
                MaximumSize = new Size(620, 0),
                AutoSize = true 
            }
        });
        
        _yPos += 35;
        return this;
    }
    
    public FormLayoutBuilder AddProtocolRow(string label, bool enabled)
    {
        _parent.Controls.AddRange(new Control[]
        {
            new Label 
            { 
                Text = label, 
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Location = new Point(30, _yPos), 
                Size = new Size(180, 30) 
            },
            new Label 
            { 
                Text = enabled ? "ENABLED" : "DISABLED",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = enabled ? AppConstants.Colors.Success : AppConstants.Colors.Danger,
                Location = new Point(220, _yPos),
                Size = new Size(90, 28),
                TextAlign = ContentAlignment.MiddleCenter
            }
        });
        
        _yPos += 50;
        return this;
    }
    
    public FormLayoutBuilder AddPathRow(string label, string path, Action<string> onOpen)
    {
        var txtPath = new TextBox
        {
            Text = path,
            Font = new Font("Consolas", 9F),
            Location = new Point(190, _yPos),
            Size = new Size(570, 25),
            ReadOnly = true,
            BackColor = Color.FromArgb(250, 250, 250)
        };
        
        var btnOpen = new Button
        {
            Text = "Open",
            Location = new Point(770, _yPos - 2),
            Size = new Size(70, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppConstants.Colors.Primary,
            ForeColor = Color.White
        };
        btnOpen.Click += (s, e) => onOpen(path);
        
        _parent.Controls.AddRange(new Control[]
        {
            new Label 
            { 
                Text = label, 
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(30, _yPos), 
                Size = new Size(150, 25) 
            },
            txtPath,
            btnOpen
        });
        
        _yPos += 45;
        return this;
    }
}