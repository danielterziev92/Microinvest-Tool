using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class AddSqlUserDialog : Form
{
    private TextBox userNameTextBox;
    private TextBox passwordTextBox;
    private TextBox confirmPasswordTextBox;
    private CheckedListBox rolesListBox;
    private Button okButton;
    private Button cancelButton;
    
    public string UserName { get; private set; }
    public string Password { get; private set; }
    public List<string> SelectedRoles { get; private set; }
    
    public AddSqlUserDialog(List<string> availableRoles)
    {
        InitializeComponents(availableRoles);
    }
    
    private void InitializeComponents(List<string> availableRoles)
    {
        this.Text = "Add SQL User";
        this.Size = new Size(500, 550);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        Label headerLabel = new Label();
        headerLabel.Text = "Add SQL Authentication User";
        headerLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        headerLabel.Location = new Point(20, 20);
        headerLabel.AutoSize = true;
        this.Controls.Add(headerLabel);
        
        Label userNameLabel = new Label();
        userNameLabel.Text = "User Name:";
        userNameLabel.Font = new Font("Segoe UI", 9F);
        userNameLabel.Location = new Point(20, 60);
        userNameLabel.AutoSize = true;
        this.Controls.Add(userNameLabel);
        
        userNameTextBox = new TextBox();
        userNameTextBox.Font = new Font("Segoe UI", 10F);
        userNameTextBox.Location = new Point(20, 85);
        userNameTextBox.Size = new Size(440, 25);
        this.Controls.Add(userNameTextBox);
        
        Label passwordLabel = new Label();
        passwordLabel.Text = "Password:";
        passwordLabel.Font = new Font("Segoe UI", 9F);
        passwordLabel.Location = new Point(20, 125);
        passwordLabel.AutoSize = true;
        this.Controls.Add(passwordLabel);
        
        passwordTextBox = new TextBox();
        passwordTextBox.Font = new Font("Segoe UI", 10F);
        passwordTextBox.Location = new Point(20, 150);
        passwordTextBox.Size = new Size(440, 25);
        passwordTextBox.UseSystemPasswordChar = true;
        this.Controls.Add(passwordTextBox);
        
        Label confirmPasswordLabel = new Label();
        confirmPasswordLabel.Text = "Confirm Password:";
        confirmPasswordLabel.Font = new Font("Segoe UI", 9F);
        confirmPasswordLabel.Location = new Point(20, 190);
        confirmPasswordLabel.AutoSize = true;
        this.Controls.Add(confirmPasswordLabel);
        
        confirmPasswordTextBox = new TextBox();
        confirmPasswordTextBox.Font = new Font("Segoe UI", 10F);
        confirmPasswordTextBox.Location = new Point(20, 215);
        confirmPasswordTextBox.Size = new Size(440, 25);
        confirmPasswordTextBox.UseSystemPasswordChar = true;
        this.Controls.Add(confirmPasswordTextBox);
        
        Label rolesLabel = new Label();
        rolesLabel.Text = "Server Roles:";
        rolesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        rolesLabel.Location = new Point(20, 255);
        rolesLabel.AutoSize = true;
        this.Controls.Add(rolesLabel);
        
        rolesListBox = new CheckedListBox();
        rolesListBox.Font = new Font("Segoe UI", 9F);
        rolesListBox.Location = new Point(20, 280);
        rolesListBox.Size = new Size(440, 180);
        rolesListBox.CheckOnClick = true;
        
        foreach (string role in availableRoles)
        {
            rolesListBox.Items.Add(role);
        }
        
        this.Controls.Add(rolesListBox);
        
        okButton = new Button();
        okButton.Text = "Create User";
        okButton.Size = new Size(120, 35);
        okButton.Location = new Point(240, 475);
        okButton.BackColor = AppConstants.Colors.Success;
        okButton.ForeColor = Color.White;
        okButton.FlatStyle = FlatStyle.Flat;
        okButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        okButton.Cursor = Cursors.Hand;
        okButton.Click += OkButton_Click;
        this.Controls.Add(okButton);
        
        cancelButton = new Button();
        cancelButton.Text = "Cancel";
        cancelButton.Size = new Size(100, 35);
        cancelButton.Location = new Point(370, 475);
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
        string userName = userNameTextBox.Text.Trim();
        string password = passwordTextBox.Text;
        string confirmPassword = confirmPasswordTextBox.Text;
        
        if (string.IsNullOrEmpty(userName))
        {
            MessageBox.Show("Please enter a user name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
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
        
        UserName = userName;
        Password = password;
        SelectedRoles = new List<string>();
        
        foreach (object item in rolesListBox.CheckedItems)
        {
            SelectedRoles.Add(item.ToString());
        }
        
        this.DialogResult = DialogResult.OK;
    }
}