using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class AddWindowsUserDialog : Form
{
    private TextBox accountTextBox;
    private CheckedListBox rolesListBox;
    private Button okButton;
    private Button cancelButton;
    
    public string WindowsAccount { get; private set; }
    public List<string> SelectedRoles { get; private set; }
    
    public AddWindowsUserDialog(List<string> availableRoles)
    {
        InitializeComponents(availableRoles);
    }
    
    private void InitializeComponents(List<string> availableRoles)
    {
        this.Text = "Add Windows User";
        this.Size = new Size(500, 450);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        Label headerLabel = new Label();
        headerLabel.Text = "Add Windows User / Group";
        headerLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        headerLabel.Location = new Point(20, 20);
        headerLabel.AutoSize = true;
        this.Controls.Add(headerLabel);
        
        Label accountLabel = new Label();
        accountLabel.Text = "Windows Account (DOMAIN\\User or COMPUTERNAME\\User):";
        accountLabel.Font = new Font("Segoe UI", 9F);
        accountLabel.Location = new Point(20, 60);
        accountLabel.Size = new Size(450, 20);
        this.Controls.Add(accountLabel);
        
        accountTextBox = new TextBox();
        accountTextBox.Font = new Font("Segoe UI", 10F);
        accountTextBox.Location = new Point(20, 85);
        accountTextBox.Size = new Size(440, 25);
        this.Controls.Add(accountTextBox);
        
        Label exampleLabel = new Label();
        exampleLabel.Text = "Example: MYCOMPANY\\john.doe or .\\LocalUser";
        exampleLabel.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
        exampleLabel.ForeColor = Color.Gray;
        exampleLabel.Location = new Point(20, 115);
        exampleLabel.Size = new Size(450, 20);
        this.Controls.Add(exampleLabel);
        
        Label rolesLabel = new Label();
        rolesLabel.Text = "Server Roles:";
        rolesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        rolesLabel.Location = new Point(20, 145);
        rolesLabel.AutoSize = true;
        this.Controls.Add(rolesLabel);
        
        rolesListBox = new CheckedListBox();
        rolesListBox.Font = new Font("Segoe UI", 9F);
        rolesListBox.Location = new Point(20, 170);
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
        okButton.Location = new Point(240, 370);
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
        cancelButton.Location = new Point(370, 370);
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
        string account = accountTextBox.Text.Trim();
        
        if (string.IsNullOrEmpty(account))
        {
            MessageBox.Show("Please enter a Windows account.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (!account.Contains("\\"))
        {
            MessageBox.Show("Account must be in format DOMAIN\\User or COMPUTER\\User", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        WindowsAccount = account;
        SelectedRoles = new List<string>();
        
        foreach (object item in rolesListBox.CheckedItems)
        {
            SelectedRoles.Add(item.ToString());
        }
        
        this.DialogResult = DialogResult.OK;
    }
}