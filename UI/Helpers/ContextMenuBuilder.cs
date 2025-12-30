using System;
using System.Windows.Forms;

public class ContextMenuBuilder
{
    private ContextMenuStrip menu;
    
    public ContextMenuBuilder()
    {
        menu = new ContextMenuStrip();
        menu.Font = AppConstants.Fonts.Normal;
    }
    
    public ContextMenuBuilder AddItem(string text, EventHandler onClick, bool enabled = true)
    {
        ToolStripMenuItem item = new ToolStripMenuItem(text);
        item.Click += onClick;
        item.Enabled = enabled;
        menu.Items.Add(item);
        
        return this;
    }
    
    public ContextMenuBuilder AddSeparator()
    {
        menu.Items.Add(new ToolStripSeparator());
        
        return this;
    }
    
    public ContextMenuStrip Build()
    {
        return menu;
    }
}