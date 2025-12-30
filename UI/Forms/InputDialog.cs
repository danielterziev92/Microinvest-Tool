using System;
using System.Drawing;
using System.Windows.Forms;

public class InputDialog : Form
{
    private Label promptLabel;
    private TextBox inputTextBox;
    private Button okButton;
    private Button cancelButton;
    
    public string InputValue { get; private set; }
    
    public InputDialog(string title, string prompt, string defaultValue)
    {
        InitializeComponents(title, prompt, defaultValue);
    }
    
    private void InitializeComponents(string title, string prompt, string defaultValue)
    {
        this.Text = title;
        this.Size = new Size(400, 180);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        promptLabel = new Label();
        promptLabel.Text = prompt;
        promptLabel.Location = new Point(20, 20);
        promptLabel.Size = new Size(350, 40);
        promptLabel.Font = new Font("Segoe UI", 10F);
        this.Controls.Add(promptLabel);
        
        inputTextBox = new TextBox();
        inputTextBox.Text = defaultValue;
        inputTextBox.Location = new Point(20, 70);
        inputTextBox.Size = new Size(340, 25);
        inputTextBox.Font = new Font("Segoe UI", 10F);
        this.Controls.Add(inputTextBox);
        
        okButton = new Button();
        okButton.Text = "OK";
        okButton.Location = new Point(190, 105);
        okButton.Size = new Size(80, 30);
        okButton.BackColor = AppConstants.Colors.Primary;
        okButton.ForeColor = Color.White;
        okButton.FlatStyle = FlatStyle.Flat;
        okButton.DialogResult = DialogResult.OK;
        okButton.Click += OkButton_Click;
        this.Controls.Add(okButton);
        
        cancelButton = new Button();
        cancelButton.Text = "Cancel";
        cancelButton.Location = new Point(280, 105);
        cancelButton.Size = new Size(80, 30);
        cancelButton.BackColor = Color.FromArgb(150, 150, 150);
        cancelButton.ForeColor = Color.White;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.DialogResult = DialogResult.Cancel;
        this.Controls.Add(cancelButton);
        
        this.AcceptButton = okButton;
        this.CancelButton = cancelButton;
        
        inputTextBox.Select();
        inputTextBox.SelectAll();
    }
    
    private void OkButton_Click(object sender, EventArgs e)
    {
        InputValue = inputTextBox.Text;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
    
    public static string Show(string title, string prompt, string defaultValue)
    {
        InputDialog dialog = new InputDialog(title, prompt, defaultValue);
        
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return dialog.InputValue;
        }
        
        return null;
    }
}