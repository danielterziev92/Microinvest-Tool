using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class InstanceDetailsForm : BaseForm
{
    private SQLServerInstanceDetails details;
    private Panel headerPanel;
    private TabControl tabs;
    private Panel footerPanel;
    
    private TcpIpConfigService tcpService;
    private TcpPortService portService;
    private FirewallService firewallService;
    private CollationService collationService;
    private SectorSizeService sectorService;
    private UserManagementService userService;
    
    private const int HEADER_HEIGHT = 100;
    private const int FOOTER_HEIGHT = 60;
    
    public InstanceDetailsForm(SQLServerInstanceDetails instanceDetails, ILogService logService)
    {
        this.details = instanceDetails;
        this.Logger = logService;
        
        InitServices();
        InitUI();
    }
    
    private void InitServices()
    {
        tcpService = new TcpIpConfigService(Logger);
        portService = new TcpPortService(Logger);
        firewallService = new FirewallService(Logger);
        collationService = new CollationService(Logger);
        sectorService = new SectorSizeService(Logger);
        userService = new UserManagementService(Logger);
    }
    
    private void InitUI()
    {
        this.Text = "Instance Details - " + details.InstanceName;
        this.Size = new Size(950, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.MinimumSize = new Size(800, 600);
        
        CreateFooter();
        CreateHeader();
        CreateTabs();
        
        this.Resize += (s, e) => UpdateLayout();
        UpdateLayout();
    }
    
    private void CreateHeader()
    {
        headerPanel = new Panel { BackColor = AppConstants.Colors.Primary };
        this.Controls.Add(headerPanel);
        
        Label title = new Label
        {
            Text = details.InstanceName,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 15),
            AutoSize = true
        };
        headerPanel.Controls.Add(title);
        
        Label version = new Label
        {
            Text = string.Format("{0} - {1}", details.Version, details.Edition),
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.FromArgb(230, 230, 230),
            Location = new Point(20, 48),
            AutoSize = true
        };
        headerPanel.Controls.Add(version);
        
        Label status = new Label
        {
            Text = details.ServiceStatus,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = details.IsRunning() ? AppConstants.Colors.Success : AppConstants.Colors.Danger,
            Padding = new Padding(10, 5, 10, 5),
            Location = new Point(800, 30),
            AutoSize = true,
            Name = "status"
        };
        headerPanel.Controls.Add(status);
        
        headerPanel.Resize += (s, e) =>
        {
            Control st = headerPanel.Controls["status"];
            if (st != null) st.Location = new Point(headerPanel.Width - st.Width - 20, 30);
        };
    }
    
    private void CreateTabs()
    {
        tabs = new TabControl
        {
            Font = AppConstants.Fonts.Normal,
            Padding = new Point(10, 5)
        };
        this.Controls.Add(tabs);
        
        tabs.TabPages.Add(BuildOverviewTab());
        tabs.TabPages.Add(BuildNetworkTab());
        tabs.TabPages.Add(BuildTcpDetailsTab());
        tabs.TabPages.Add(BuildPathsTab());
        tabs.TabPages.Add(BuildDiskTab());
        tabs.TabPages.Add(BuildUsersTab());
    }
    
    private TabPage BuildOverviewTab()
    {
        TabPage tab = new TabPage("Overview");
        
        TabBuilder builder = new TabBuilder(tab);
        builder.AddSection("Instance Information")
            .AddRow("Instance Name:", details.InstanceName)
            .AddRow("Version:", details.Version)
            .AddRow("Edition:", details.Edition)
            .AddSpace(20)
            .AddSection("Service Information")
            .AddRow("Service Name:", details.ServiceName)
            .AddRow("Service Status:", details.ServiceStatus)
            .AddRow("Service Account:", details.ServiceAccount)
            .AddSpace(20)
            .AddSection("Configuration")
            .AddRow("Collation:", details.Collation ?? "Unknown");
        
        if (details.Collation != null && !details.Collation.Equals("Cyrillic_General_CI_AS", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddButton("Change to Cyrillic", ChangeCollation_Click, ThemedButton.ButtonStyle.Warning);
        }
        
        return tab;
    }
    
    private TabPage BuildNetworkTab()
    {
        TabPage tab = new TabPage("Network");
        tab.BackColor = Color.White;
        
        Panel content = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(30, 20, 30, 20)
        };
        tab.Controls.Add(content);
        
        int y = 0;
        
        Label header = new Label
        {
            Text = "Network Protocols",
            Font = AppConstants.Fonts.Subheading,
            ForeColor = AppConstants.Colors.Primary,
            Location = new Point(0, y),
            AutoSize = true
        };
        content.Controls.Add(header);
        y += 40;
        
        AddProtocolRow(content, "TCP/IP:", details.TcpEnabled, ref y);
        AddProtocolRow(content, "Named Pipes:", details.NamedPipesEnabled, ref y);
        AddProtocolRow(content, "Shared Memory:", details.SharedMemoryEnabled, ref y);
        
        y += 20;
        
        Label portHeader = new Label
        {
            Text = "Port Configuration",
            Font = AppConstants.Fonts.Subheading,
            ForeColor = AppConstants.Colors.Primary,
            Location = new Point(0, y),
            AutoSize = true
        };
        content.Controls.Add(portHeader);
        y += 40;
        
        int currentPort = portService.GetPrimaryPort(details.InstanceRegistryPath);
        
        Label portLabel = new Label
        {
            Text = "Primary Port:",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        content.Controls.Add(portLabel);
        
        Label portValue = new Label
        {
            Text = currentPort > 0 ? currentPort.ToString() : "Dynamic",
            Font = AppConstants.Fonts.Normal,
            ForeColor = AppConstants.Colors.TextPrimary,
            Location = new Point(180, y + 5),
            AutoSize = true
        };
        content.Controls.Add(portValue);
        
        ThemedButton editPort = new ThemedButton
        {
            Text = "Edit Port",
            Size = new Size(120, 35),
            Location = new Point(300, y)
        };
        editPort.SetStyle(ThemedButton.ButtonStyle.Primary);
        editPort.Click += EditPort_Click;
        content.Controls.Add(editPort);
        
        return tab;
    }
    
    private void AddProtocolRow(Panel parent, string label, bool enabled, ref int y)
    {
        Label l = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location = new Point(20, y),
            AutoSize = true
        };
        parent.Controls.Add(l);
        
        Label status = new Label
        {
            Text = enabled ? "ENABLED" : "DISABLED",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = enabled ? AppConstants.Colors.Success : AppConstants.Colors.Danger,
            Size = new Size(90, 28),
            Location = new Point(180, y),
            TextAlign = ContentAlignment.MiddleCenter
        };
        parent.Controls.Add(status);
        
        y += 45;
    }
    
    private TabPage BuildTcpDetailsTab()
    {
        TabPage tab = new TabPage("TCP/IP Details");
        tab.BackColor = Color.White;
        
        Panel headerPanelTab = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(tab.Width, 60),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.FromArgb(245, 245, 245)
        };
        tab.Controls.Add(headerPanelTab);
        
        Label headerLabel = new Label
        {
            Text = string.Format("IP Configurations ({0} found)", details.TcpIpConfigs.Count),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = AppConstants.Colors.Primary,
            Location = new Point(20, 18),
            AutoSize = true
        };
        headerPanelTab.Controls.Add(headerLabel);
        
        ThemedButton enableAllButton = new ThemedButton
        {
            Text = "Enable All IPs",
            Size = new Size(140, 35),
            Location = new Point(750, 12),
            Name = "enableAllButton"
        };
        enableAllButton.SetStyle(ThemedButton.ButtonStyle.Success);
        enableAllButton.Click += EnableAllIPs_Click;
        headerPanelTab.Controls.Add(enableAllButton);
        
        headerPanelTab.Resize += (s, e) =>
        {
            Control btn = headerPanelTab.Controls["enableAllButton"];
            if (btn != null) btn.Location = new Point(headerPanelTab.Width - 160, 12);
        };
        
        ListView ipListView = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Location = new Point(0, 60),
            Size = new Size(tab.Width, tab.Height - 60),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Font = AppConstants.Fonts.Normal
        };
        
        ipListView.Columns.Add("Name", 80);
        ipListView.Columns.Add("Enabled", 80);
        ipListView.Columns.Add("Active", 70);
        ipListView.Columns.Add("IP Address", 200);
        ipListView.Columns.Add("Port", 80);
        ipListView.Columns.Add("Dynamic Ports", 120);
        
        foreach (TcpIpConfig config in details.TcpIpConfigs)
        {
            ListViewItem item = new ListViewItem(config.Name);
            item.SubItems.Add(config.Enabled ? "Yes" : "No");
            
            bool isActive = config.Name == "IPAll" ? details.TcpEnabled : config.Active;
            
            item.SubItems.Add(isActive ? "Yes" : "No");
            item.SubItems.Add(config.IpAddress ?? "");
            item.SubItems.Add(config.TcpPort ?? "");
            item.SubItems.Add(config.TcpDynamicPorts ?? "");
            
            item.ForeColor = config.Enabled ? AppConstants.Colors.Success : AppConstants.Colors.TextSecondary;
            
            ipListView.Items.Add(item);
        }
        
        tab.Controls.Add(ipListView);
        
        return tab;
    }
    
    private TabPage BuildPathsTab()
    {
        TabPage tab = new TabPage("Paths");
        tab.BackColor = Color.White;
        
        Panel content = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(30, 20, 30, 20)
        };
        tab.Controls.Add(content);
        
        int y = 0;
        
        Label header = new Label
        {
            Text = "Instance Paths",
            Font = AppConstants.Fonts.Subheading,
            ForeColor = AppConstants.Colors.Primary,
            Location = new Point(0, y),
            AutoSize = true
        };
        content.Controls.Add(header);
        y += 40;
        
        AddPathRow(content, "Install Path:", details.InstallPath, ref y);
        AddPathRow(content, "Data Path:", details.DataPath, ref y);
        AddPathRow(content, "Log Path:", details.LogPath, ref y);
        AddPathRow(content, "Backup Path:", details.BackupPath, ref y);
        AddPathRow(content, "Error Log:", details.ErrorLogPath, ref y);
        
        return tab;
    }
    
    private void AddPathRow(Panel parent, string label, string path, ref int y)
    {
        Label l = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Location = new Point(0, y),
            Size = new Size(120, 25)
        };
        parent.Controls.Add(l);
        
        TextBox pathBox = new TextBox
        {
            Text = path ?? "Unknown",
            Font = new Font("Consolas", 9F),
            Location = new Point(130, y),
            Size = new Size(550, 25),
            ReadOnly = true,
            BackColor = Color.FromArgb(250, 250, 250)
        };
        parent.Controls.Add(pathBox);
        
        ThemedButton openBtn = new ThemedButton
        {
            Text = "Open",
            Size = new Size(80, 28),
            Location = new Point(690, y - 2)
        };
        openBtn.SetStyle(ThemedButton.ButtonStyle.Primary);
        openBtn.Tag = path;
        openBtn.Click += OpenPath_Click;
        openBtn.Enabled = !string.IsNullOrEmpty(path) && path != "Unknown" && System.IO.Directory.Exists(path);
        parent.Controls.Add(openBtn);
        
        y += 35;
    }
    
    private TabPage BuildDiskTab()
    {
        TabPage tab = new TabPage("Disk");
        tab.BackColor = Color.White;
        
        Panel content = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(30, 20, 30, 20)
        };
        tab.Controls.Add(content);
        
        int y = 0;
        
        Label header = new Label
        {
            Text = "Disk Sector Size Configuration",
            Font = AppConstants.Fonts.Heading,
            ForeColor = AppConstants.Colors.Primary,
            Location = new Point(0, y),
            AutoSize = true
        };
        content.Controls.Add(header);
        y += 45;
        
        Label desc = new Label
        {
            Text = "SQL Server performs best with 4KB sector size. Larger sizes may cause performance issues.",
            Font = AppConstants.Fonts.Normal,
            ForeColor = AppConstants.Colors.TextSecondary,
            Location = new Point(0, y),
            Size = new Size(750, 30)
        };
        content.Controls.Add(desc);
        y += 40;
        
        string dataPath = details.DataPath ?? "C:";
        string drive = dataPath.Substring(0, 1);
        
        SectorSizeInfo sectorInfo = sectorService.GetSectorSizeInfo(drive);
        
        Panel resultsPanel = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(800, 150),
            BackColor = sectorInfo.HasIssue ? Color.FromArgb(255, 245, 245) : Color.FromArgb(245, 255, 245),
            BorderStyle = BorderStyle.FixedSingle
        };
        content.Controls.Add(resultsPanel);
        
        int ry = 15;
        AddResultRow(resultsPanel, "Drive:", drive + ":", ref ry);
        AddResultRow(resultsPanel, "Physical (Atomicity):", sectorInfo.PhysicalBytesPerSectorForAtomicity + " bytes", ref ry);
        AddResultRow(resultsPanel, "Physical (Performance):", sectorInfo.PhysicalBytesPerSectorForPerformance + " bytes", ref ry);
        AddResultRow(resultsPanel, "FileSystem Effective:", sectorInfo.FileSystemEffectivePhysicalBytes + " bytes", ref ry);
        
        Label statusLbl = new Label
        {
            Text = sectorInfo.HasIssue ? "WARNING: Sector size > 4KB!" : "OK: Optimal sector size",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = sectorInfo.HasIssue ? AppConstants.Colors.Danger : AppConstants.Colors.Success,
            Padding = new Padding(10, 5, 10, 5),
            Location = new Point(20, ry),
            AutoSize = true
        };
        resultsPanel.Controls.Add(statusLbl);
        
        y += 165;
        
        if (sectorInfo.HasIssue)
        {
            bool fixApplied = sectorService.IsRegistryFixApplied();
            
            if (fixApplied)
            {
                Label appliedLbl = new Label
                {
                    Text = "✓ Registry fix already applied - Please restart your computer",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = AppConstants.Colors.Success,
                    Location = new Point(0, y),
                    AutoSize = true
                };
                content.Controls.Add(appliedLbl);
            }
            else
            {
                ThemedButton applyBtn = new ThemedButton
                {
                    Text = "Apply Registry Fix",
                    Size = new Size(180, 40),
                    Location = new Point(0, y)
                };
                applyBtn.SetStyle(ThemedButton.ButtonStyle.Warning);
                applyBtn.Click += ApplySectorSizeFix_Click;
                content.Controls.Add(applyBtn);
            }
        }
        
        return tab;
    }
    
    private void AddResultRow(Panel panel, string label, string value, ref int y)
    {
        Label l = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(20, y),
            Size = new Size(250, 20)
        };
        panel.Controls.Add(l);
        
        Label v = new Label
        {
            Text = value,
            Font = AppConstants.Fonts.Normal,
            ForeColor = AppConstants.Colors.TextSecondary,
            Location = new Point(280, y),
            AutoSize = true
        };
        panel.Controls.Add(v);
        
        y += 25;
    }
    
    private TabPage BuildUsersTab()
    {
        TabPage tab = new TabPage("Users");
        tab.BackColor = Color.White;
        
        Panel headerPanel = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(tab.Width, 60),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.FromArgb(245, 245, 245)
        };
        tab.Controls.Add(headerPanel);
        
        Label headerLabel = new Label
        {
            Text = "SQL Server Users & Logins",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = AppConstants.Colors.Primary,
            Location = new Point(20, 18),
            AutoSize = true
        };
        headerPanel.Controls.Add(headerLabel);
        
        ThemedButton addWinBtn = new ThemedButton
        {
            Text = "Add Windows User",
            Size = new Size(150, 35),
            Location = new Point(450, 12)
        };
        addWinBtn.SetStyle(ThemedButton.ButtonStyle.Success);
        addWinBtn.Click += AddWindowsUser_Click;
        headerPanel.Controls.Add(addWinBtn);
        
        ThemedButton addSqlBtn = new ThemedButton
        {
            Text = "Add SQL User",
            Size = new Size(130, 35),
            Location = new Point(610, 12)
        };
        addSqlBtn.SetStyle(ThemedButton.ButtonStyle.Info);
        addSqlBtn.Click += AddSqlUser_Click;
        headerPanel.Controls.Add(addSqlBtn);
        
        ThemedButton refreshBtn = new ThemedButton
        {
            Text = "Refresh",
            Size = new Size(100, 35),
            Location = new Point(750, 12),
            Name = "refreshUsersButton"
        };
        refreshBtn.SetStyle(ThemedButton.ButtonStyle.Primary);
        refreshBtn.Click += (s, e) => LoadUsers();
        headerPanel.Controls.Add(refreshBtn);
        
        headerPanel.Resize += (s, e) =>
        {
            Control btn = headerPanel.Controls["refreshUsersButton"];
            if (btn != null) btn.Location = new Point(headerPanel.Width - 120, 12);
        };
        
        ListView usersListView = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Location = new Point(0, 60),
            Size = new Size(tab.Width, tab.Height - 60),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Font = AppConstants.Fonts.Normal,
            Name = "usersListView"
        };
        
        usersListView.Columns.Add("User Name", 200);
        usersListView.Columns.Add("Type", 150);
        usersListView.Columns.Add("Server Roles", 300);
        usersListView.Columns.Add("Created", 150);
        usersListView.Columns.Add("Status", 100);
        
        ContextMenuStrip contextMenu = new ContextMenuStrip();
        
        ToolStripMenuItem editRoles = new ToolStripMenuItem("Edit Roles");
        editRoles.Click += EditUserRoles_Click;
        contextMenu.Items.Add(editRoles);
        
        ToolStripMenuItem changePwd = new ToolStripMenuItem("Change Password");
        changePwd.Click += ChangeUserPassword_Click;
        changePwd.Name = "changePasswordItem";
        contextMenu.Items.Add(changePwd);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        ToolStripMenuItem deleteUser = new ToolStripMenuItem("Delete User");
        deleteUser.Click += DeleteUser_Click;
        contextMenu.Items.Add(deleteUser);
        
        usersListView.ContextMenuStrip = contextMenu;
        
        contextMenu.Opening += (s, e) =>
        {
            if (usersListView.SelectedItems.Count > 0)
            {
                SqlServerUser user = usersListView.SelectedItems[0].Tag as SqlServerUser;
                ToolStripMenuItem pwdItem = contextMenu.Items["changePasswordItem"] as ToolStripMenuItem;
                if (pwdItem != null && user != null)
                {
                    pwdItem.Enabled = user.IsSqlAuthentication();
                }
            }
        };
        
        tab.Controls.Add(usersListView);
        
        LoadUsersAsync();
        
        return tab;
    }
    
    private void CreateFooter()
    {
        footerPanel = new Panel { BackColor = Color.FromArgb(240, 240, 240) };
        this.Controls.Add(footerPanel);
        
        ThemedButton closeBtn = new ThemedButton
        {
            Text = "Close",
            Size = new Size(100, 35),
            Location = new Point(820, 12)
        };
        closeBtn.SetStyle(ThemedButton.ButtonStyle.Info);
        closeBtn.Click += (s, e) => this.Close();
        footerPanel.Controls.Add(closeBtn);
    }
    
    private void UpdateLayout()
    {
        int w = this.ClientSize.Width;
        int h = this.ClientSize.Height;
        
        headerPanel.Location = new Point(0, 0);
        headerPanel.Size = new Size(w, HEADER_HEIGHT);
        
        tabs.Location = new Point(0, HEADER_HEIGHT);
        tabs.Size = new Size(w, h - HEADER_HEIGHT - FOOTER_HEIGHT);
        
        footerPanel.Location = new Point(0, h - FOOTER_HEIGHT);
        footerPanel.Size = new Size(w, FOOTER_HEIGHT);
    }
    
    // Event handlers
    private void ChangeCollation_Click(object sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show(
            "WARNING: Changing collation is critical!\n\nContinue?",
            "Change Collation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        
        if (result == DialogResult.Yes)
        {
            bool success = collationService.ChangeServerCollation(
                details.InstanceName,
                "Cyrillic_General_CI_AS",
                details.ServiceName
            );
            
            if (success)
            {
                ShowSuccess("Collation changed successfully!");
            }
        }
    }
    
    private void EditPort_Click(object sender, EventArgs e)
    {
        int currentPort = portService.GetPrimaryPort(details.InstanceRegistryPath);
        string input = InputDialog.Show(
            "Change TCP Port",
            "Enter new port (1-65535):",
            currentPort > 0 ? currentPort.ToString() : "1433"
        );
        
        if (!string.IsNullOrEmpty(input))
        {
            int newPort;
            if (int.TryParse(input, out newPort) && newPort >= 1 && newPort <= 65535)
            {
                bool success = portService.SetPrimaryPort(details.InstanceRegistryPath, newPort);
                if (success)
                {
                    ShowSuccess(string.Format("Port changed to {0}!\n\nRestart SQL Server to apply.", newPort));
                }
            }
            else
            {
                ShowError("Invalid port number!");
            }
        }
    }
    
    private void EnableAllIPs_Click(object sender, EventArgs e)
    {
        int count = tcpService.EnableAllIpConfigs(details.InstanceRegistryPath);
        if (count > 0)
        {
            ShowSuccess(string.Format("Enabled {0} IP(s)!\n\nRestart SQL Server to apply.", count));
        }
    }
    
    private void OpenPath_Click(object sender, EventArgs e)
    {
        ThemedButton btn = sender as ThemedButton;
        if (btn != null && btn.Tag != null)
        {
            string path = btn.Tag.ToString();
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", "\"" + path + "\"");
            }
            catch (Exception ex)
            {
                ShowError("Failed to open path:\n\n" + ex.Message);
            }
        }
    }
    
    private void ApplySectorSizeFix_Click(object sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show(
            "Apply registry fix for 4KB sector size?\n\nRequires restart!",
            "Apply Fix",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        
        if (result == DialogResult.Yes)
        {
            bool success = sectorService.ApplyRegistryFix();
            if (success)
            {
                ShowSuccess("Fix applied!\n\nPlease restart your computer.");
            }
        }
    }
    
    private void AddWindowsUser_Click(object sender, EventArgs e)
    {
        List<string> roles = userService.GetAvailableServerRoles(details.InstanceName);
        AddWindowsUserDialog dialog = new AddWindowsUserDialog(roles);
        
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            bool success = userService.CreateWindowsUser(
                details.InstanceName,
                dialog.WindowsAccount,
                dialog.SelectedRoles
            );
            
            if (success)
            {
                ShowSuccess("User created: " + dialog.WindowsAccount);
                LoadUsers();
            }
        }
        
        dialog.Dispose();
    }
    
    private void AddSqlUser_Click(object sender, EventArgs e)
    {
        List<string> roles = userService.GetAvailableServerRoles(details.InstanceName);
        AddSqlUserDialog dialog = new AddSqlUserDialog(roles);
        
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            bool success = userService.CreateSqlUser(
                details.InstanceName,
                dialog.UserName,
                dialog.Password,
                dialog.SelectedRoles
            );
            
            if (success)
            {
                ShowSuccess("User created: " + dialog.UserName);
                LoadUsers();
            }
        }
        
        dialog.Dispose();
    }
    
    private void EditUserRoles_Click(object sender, EventArgs e)
    {
        ListView listView = FindUsersListView();
        if (listView == null || listView.SelectedItems.Count == 0) return;
        
        SqlServerUser user = listView.SelectedItems[0].Tag as SqlServerUser;
        if (user == null) return;
        
        List<string> roles = userService.GetAvailableServerRoles(details.InstanceName);
        EditRolesDialog dialog = new EditRolesDialog(user.Name, user.ServerRoles, roles);
        
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            bool success = userService.UpdateUserRoles(details.InstanceName, user.Name, dialog.SelectedRoles);
            if (success)
            {
                ShowSuccess("Roles updated!");
                LoadUsers();
            }
        }
        
        dialog.Dispose();
    }
    
    private void ChangeUserPassword_Click(object sender, EventArgs e)
    {
        ListView listView = FindUsersListView();
        if (listView == null || listView.SelectedItems.Count == 0) return;
        
        SqlServerUser user = listView.SelectedItems[0].Tag as SqlServerUser;
        if (user == null || !user.IsSqlAuthentication()) return;
        
        ChangePasswordDialog dialog = new ChangePasswordDialog(user.Name);
        
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            bool success = userService.ChangeUserPassword(details.InstanceName, user.Name, dialog.NewPassword);
            if (success)
            {
                ShowSuccess("Password changed!");
            }
        }
        
        dialog.Dispose();
    }
    
    private void DeleteUser_Click(object sender, EventArgs e)
    {
        ListView listView = FindUsersListView();
        if (listView == null || listView.SelectedItems.Count == 0) return;
        
        SqlServerUser user = listView.SelectedItems[0].Tag as SqlServerUser;
        if (user == null) return;
        
        DialogResult result = MessageBox.Show(
            "Delete user: " + user.Name + "?\n\nThis cannot be undone!",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        );
        
        if (result == DialogResult.Yes)
        {
            bool success = userService.DeleteUser(details.InstanceName, user.Name);
            if (success)
            {
                ShowSuccess("User deleted!");
                LoadUsers();
            }
        }
    }
    
    private void LoadUsersAsync()
    {
        System.Threading.ThreadPool.QueueUserWorkItem(delegate { LoadUsers(); });
    }
    
    private void LoadUsers()
    {
        try
        {
            ListView listView = FindUsersListView();
            if (listView == null) return;
            
            List<SqlServerUser> users = userService.GetAllUsers(details.InstanceName);
            
            if (listView.InvokeRequired)
            {
                listView.Invoke(new Action(delegate { PopulateUsers(listView, users); }));
            }
            else
            {
                PopulateUsers(listView, users);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error loading users", ex);
        }
    }
    
    private void PopulateUsers(ListView listView, List<SqlServerUser> users)
    {
        listView.Items.Clear();
        
        foreach (SqlServerUser user in users)
        {
            ListViewItem item = new ListViewItem(user.Name);
            item.SubItems.Add(user.TypeDescription);
            item.SubItems.Add(string.Join(", ", user.ServerRoles.ToArray()));
            item.SubItems.Add(user.CreateDate);
            item.SubItems.Add(user.IsDisabled ? "Disabled" : "Active");
            
            item.Tag = user;
            
            if (user.IsDisabled)
            {
                item.ForeColor = AppConstants.Colors.TextSecondary;
            }
            else if (user.ServerRoles.Contains("sysadmin"))
            {
                item.ForeColor = AppConstants.Colors.Danger;
                item.Font = new Font(item.Font, FontStyle.Bold);
            }
            else if (user.ServerRoles.Count > 0)
            {
                item.ForeColor = AppConstants.Colors.Success;
            }
            
            listView.Items.Add(item);
        }
    }
    
    private ListView FindUsersListView()
    {
        foreach (TabPage tab in tabs.TabPages)
        {
            if (tab.Text == "Users")
            {
                return tab.Controls["usersListView"] as ListView;
            }
        }
        return null;
    }
}