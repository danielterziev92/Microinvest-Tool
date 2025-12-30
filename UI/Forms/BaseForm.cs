using System;
using System.Drawing;
using System.Windows.Forms;

public class BaseForm : Form
{
    protected ILogService Logger { get; set; }
    
    public BaseForm()
    {
        InitializeBaseSettings();
    }
    
    private void InitializeBaseSettings()
    {
        this.Font = AppConstants.Fonts.Normal;
        this.BackColor = AppConstants.Colors.Background;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.KeyPreview = true;
        this.Size = UIHelper.GetOptimalFormSize();
    }
    
    protected void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    
    protected void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    protected void ShowWarning(string message)
    {
        MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    
    protected DialogResult AskConfirmation(string message, string title)
    {
        return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    }
}