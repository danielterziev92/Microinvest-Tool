using System;
using System.Drawing;
using System.Windows.Forms;

public class ChangePasswordDialog : Form
{
    private TextBox passwordTextBox;
    private TextBox confirmPasswordTextBox;
    private Button okButton;
    private Button cancelButton;
    
    public string NewPassword { get; private set; }
    
    public ChangePasswordDialog(string userName)
    {
        InitializeComponents(userName);
    }
    
    private void InitializeComponents(string userName)
    {
        this.Text = "Change Password";
        this.Size = new Size(450, 280);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        Label headerLabel = new Label();
        headerLabel.Text = "Change SQL User Password";
        headerLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        headerLabel.Location = new Point(20, 20);
        headerLabel.AutoSize = true;
        this.Controls.Add(headerLabel);
        
        Label userLabel = new Label();
        userLabel.Text = "User: " + userName;
        userLabel.Font = new Font("Segoe UI", 9F);
        userLabel.ForeColor = AppConstants.Colors.TextSecondary;
        userLabel.Location = new Point(20, 50);
        userLabel.AutoSize = true;
        this.Controls.Add(userLabel);
        
        Label passwordLabel = new Label();
        passwordLabel.Text = "New Password:";
        passwordLabel.Font = new Font("Segoe UI", 9F);
        passwordLabel.Location = new Point(20, 85);
        passwordLabel.AutoSize = true;
        this.Controls.Add(passwordLabel);
        
        passwordTextBox = new TextBox();
        passwordTextBox.Font = new Font("Segoe UI", 10F);
        passwordTextBox.Location = new Point(20, 110);
        passwordTextBox.Size = new Size(390, 25);
        passwordTextBox.UseSystemPasswordChar = true;
        this.Controls.Add(passwordTextBox);
        
        Label confirmPasswordLabel = new Label();
        confirmPasswordLabel.Text = "Confirm New Password:";
        confirmPasswordLabel.Font = new Font("Segoe UI", 9F);
        confirmPasswordLabel.Location = new Point(20, 150);
        confirmPasswordLabel.AutoSize = true;
        this.Controls.Add(confirmPasswordLabel);
        
        confirmPasswordTextBox = new TextBox();
        confirmPasswordTextBox.Font = new Font("Segoe UI", 10F);
        confirmPasswordTextBox.Location = new Point(20, 175);
        confirmPasswordTextBox.Size = new Size(390, 25);
        confirmPasswordTextBox.UseSystemPasswordChar = true;
        this.Controls.Add(confirmPasswordTextBox);
        
        okButton = new Button();
        okButton.Text = "Change Password";
        okButton.Size = new Size(150, 35);
        okButton.Location = new Point(160, 220);
        okButton.BackColor = AppConstants.Colors.Warning;
        okButton.ForeColor = Color.White;
        okButton.FlatStyle = FlatStyle.Flat;
        okButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        okButton.Cursor = Cursors.Hand;
        okButton.Click += OkButton_Click;
        this.Controls.Add(okButton);
        
        cancelButton = new Button();
        cancelButton.Text = "Cancel";
        cancelButton.Size = new Size(100, 35);
        cancelButton.Location = new Point(320, 220);
        cancelButton.BackColor = Color.FromArgb(180, 180, 180);
        cancelButton.ForeColor = Color.White;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        cancelButton.Cursor = Cursors.Hand;
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        this.Controls.Add(cancelButton);
    }
    
    private void OkButton_Click(object sender, EventArgs e)
    {
        string password = passwordTextBox.Text;
        string confirmPassword = confirmPasswordTextBox.Text;
        
        if (string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (password != confirmPassword)
        {
            MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (password.Length < 8)
        {
            MessageBox.Show("Password must be at least 8 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        NewPassword = password;
        this.DialogResult = DialogResult.OK;
    }
}