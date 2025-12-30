using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class EditRolesDialog : Form
{
    private CheckedListBox rolesListBox;
    private Button okButton;
    private Button cancelButton;
    
    public List<string> SelectedRoles { get; private set; }
    
    public EditRolesDialog(string userName, List<string> currentRoles, List<string> availableRoles)
    {
        InitializeComponents(userName, currentRoles, availableRoles);
    }
    
    private void InitializeComponents(string userName, List<string> currentRoles, List<string> availableRoles)
    {
        this.Text = "Edit User Roles";
        this.Size = new Size(450, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        Label headerLabel = new Label();
        headerLabel.Text = "Edit Server Roles";
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
        
        Label rolesLabel = new Label();
        rolesLabel.Text = "Server Roles:";
        rolesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        rolesLabel.Location = new Point(20, 80);
        rolesLabel.AutoSize = true;
        this.Controls.Add(rolesLabel);
        
        rolesListBox = new CheckedListBox();
        rolesListBox.Font = new Font("Segoe UI", 9F);
        rolesListBox.Location = new Point(20, 105);
        rolesListBox.Size = new Size(390, 200);
        rolesListBox.CheckOnClick = true;
        
        foreach (string role in availableRoles)
        {
            bool isChecked = currentRoles.Contains(role);
            rolesListBox.Items.Add(role, isChecked);
        }
        
        this.Controls.Add(rolesListBox);
        
        okButton = new Button();
        okButton.Text = "Save Changes";
        okButton.Size = new Size(130, 35);
        okButton.Location = new Point(180, 320);
        okButton.BackColor = AppConstants.Colors.Primary;
        okButton.ForeColor = Color.White;
        okButton.FlatStyle = FlatStyle.Flat;
        okButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        okButton.Cursor = Cursors.Hand;
        okButton.Click += OkButton_Click;
        this.Controls.Add(okButton);
        
        cancelButton = new Button();
        cancelButton.Text = "Cancel";
        cancelButton.Size = new Size(100, 35);
        cancelButton.Location = new Point(320, 320);
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
        SelectedRoles = new List<string>();
        
        foreach (object item in rolesListBox.CheckedItems)
        {
            SelectedRoles.Add(item.ToString());
        }
        
        this.DialogResult = DialogResult.OK;
    }
}