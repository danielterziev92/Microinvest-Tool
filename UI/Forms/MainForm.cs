using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

public class MainForm : BaseForm
{
    private SQLServerService sqlServerService;
    private SQLServerDetailsService detailsService;
    private ValidationService validationService;
    private FirewallService firewallService;
    private TcpPortService portService;
    private WmiRepairService wmiRepairService;
    
    private TabControl mainTabControl;
    private Panel headerPanel;
    private ListView instanceListView;
    private StatusBar statusBar;
    private Button refreshButton;
    
    public MainForm()
    {
        InitializeServices();
        InitializeComponents();
        LoadInstances();
    }
    
    private void InitializeServices()
    {
        Logger = new LogService(null, "sql-manager.log");
        
        sqlServerService = new SQLServerService(Logger);
        detailsService = new SQLServerDetailsService(Logger);
        validationService = new ValidationService(Logger);
        firewallService = new FirewallService(Logger);
        portService = new TcpPortService(Logger);
        wmiRepairService = new WmiRepairService(Logger);
    }
    
    private void InitializeComponents()
    {
        this.Text = "SQL Server Manager";
        this.Size = new Size(1200, 800);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(800, 600);
        
        CreateStatusBar();
        CreateHeader();
        CreateTabControl();
        
        this.Resize += MainForm_Resize;
    }
    
    private void CreateStatusBar()
    {
        statusBar = new StatusBar(Logger);
        this.Controls.Add(statusBar);
    }
    
    private void CreateHeader()
    {
        headerPanel = new Panel();
        headerPanel.Height = 80;
        headerPanel.Dock = DockStyle.Top;
        headerPanel.BackColor = AppConstants.Colors.Primary;
        this.Controls.Add(headerPanel);
        
        Label titleLabel = new Label();
        titleLabel.Text = "SQL Server Manager";
        titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        titleLabel.ForeColor = Color.White;
        titleLabel.Location = new Point(20, 15);
        titleLabel.AutoSize = true;
        headerPanel.Controls.Add(titleLabel);
        
        Label subtitleLabel = new Label();
        subtitleLabel.Text = "Manage SQL Server Instances";
        subtitleLabel.Font = new Font("Segoe UI", 10F);
        subtitleLabel.ForeColor = Color.FromArgb(230, 230, 230);
        subtitleLabel.Location = new Point(20, 48);
        subtitleLabel.AutoSize = true;
        headerPanel.Controls.Add(subtitleLabel);
        
        refreshButton = new Button();
        refreshButton.Text = "Refresh";
        refreshButton.Size = new Size(120, 40);
        refreshButton.BackColor = Color.White;
        refreshButton.ForeColor = AppConstants.Colors.Primary;
        refreshButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        refreshButton.FlatStyle = FlatStyle.Flat;
        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.Cursor = Cursors.Hand;
        refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        refreshButton.Click += RefreshButton_Click;
        
        PositionRefreshButton();
        headerPanel.Controls.Add(refreshButton);
        headerPanel.Resize += (s, e) => PositionRefreshButton();
    }
    
    private void PositionRefreshButton()
    {
        if (refreshButton != null && headerPanel != null)
        {
            refreshButton.Location = new Point(headerPanel.Width - 140, 20);
        }
    }
    
    private void CreateTabControl()
    {
        mainTabControl = new TabControl();
        mainTabControl.Dock = DockStyle.Fill;
        mainTabControl.Font = new Font("Segoe UI", 10F);
        mainTabControl.Padding = new Point(10, 5);
        
        TabPage sqlServerTab = new TabPage("SQL Server Management");
        CreateSQLServerTab(sqlServerTab);
        mainTabControl.TabPages.Add(sqlServerTab);

        TabPage wmiToolsTab = new TabPage("WMI Tools");
        CreateSystemToolsTab(wmiToolsTab);
        mainTabControl.TabPages.Add(wmiToolsTab);
        
        TabPage settingsTab = new TabPage("Settings");
        CreateSettingsTab(settingsTab);
        mainTabControl.TabPages.Add(settingsTab);
        
        TabPage aboutTab = new TabPage("About");
        CreateAboutTab(aboutTab);
        mainTabControl.TabPages.Add(aboutTab);
        
        this.Controls.Add(mainTabControl);
        mainTabControl.BringToFront();
    }
    
    private void CreateSQLServerTab(TabPage tab)
    {
        tab.BackColor = AppConstants.Colors.Background;
        
        instanceListView = new ListView();
        instanceListView.View = View.Details;
        instanceListView.FullRowSelect = true;
        instanceListView.GridLines = true;
        instanceListView.Font = AppConstants.Fonts.Normal;
        instanceListView.Dock = DockStyle.Fill;
        instanceListView.BackColor = Color.White;
        
        instanceListView.Columns.Add("Instance Name", 200);
        instanceListView.Columns.Add("Version", 150);
        instanceListView.Columns.Add("Edition", 150);
        instanceListView.Columns.Add("Service Status", 120);
        instanceListView.Columns.Add("TCP/IP", 80);
        instanceListView.Columns.Add("Named Pipes", 100);
        instanceListView.Columns.Add("Shared Memory", 110);
        
        ContextMenuStrip contextMenu = CreateContextMenu();
        instanceListView.ContextMenuStrip = contextMenu;
        
        instanceListView.DoubleClick += (s, e) => ViewDetails();
        instanceListView.Resize += InstanceListView_Resize;
        
        tab.Controls.Add(instanceListView);
    }
    
    private void InstanceListView_Resize(object sender, EventArgs e)
    {
        if (instanceListView.Columns.Count > 0 && instanceListView.Width > 0)
        {
            int totalWidth = instanceListView.ClientSize.Width - 20;
            instanceListView.Columns[0].Width = (int)(totalWidth * 0.25);
            instanceListView.Columns[1].Width = (int)(totalWidth * 0.18);
            instanceListView.Columns[2].Width = (int)(totalWidth * 0.18);
            instanceListView.Columns[3].Width = (int)(totalWidth * 0.13);
            instanceListView.Columns[4].Width = (int)(totalWidth * 0.08);
            instanceListView.Columns[5].Width = (int)(totalWidth * 0.10);
            instanceListView.Columns[6].Width = (int)(totalWidth * 0.08);
        }
    }
    
    private void CreateSettingsTab(TabPage tab)
    {
        tab.BackColor = Color.White;
        
        Label placeholderLabel = new Label();
        placeholderLabel.Text = "Settings will be available in future updates";
        placeholderLabel.Font = new Font("Segoe UI", 12F);
        placeholderLabel.ForeColor = AppConstants.Colors.TextSecondary;
        placeholderLabel.AutoSize = true;
        placeholderLabel.Location = new Point(20, 20);
        tab.Controls.Add(placeholderLabel);
    }
    
    private void CreateAboutTab(TabPage tab)
    {
        tab.BackColor = Color.White;
        
        Label titleLabel = new Label();
        titleLabel.Text = "SQL Server Manager";
        titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        titleLabel.ForeColor = AppConstants.Colors.Primary;
        titleLabel.AutoSize = true;
        titleLabel.Location = new Point(20, 20);
        tab.Controls.Add(titleLabel);
        
        Label versionLabel = new Label();
        versionLabel.Text = "Version 1.0.0";
        versionLabel.Font = new Font("Segoe UI", 10F);
        versionLabel.ForeColor = AppConstants.Colors.TextSecondary;
        versionLabel.AutoSize = true;
        versionLabel.Location = new Point(20, 55);
        tab.Controls.Add(versionLabel);
        
        Label descriptionLabel = new Label();
        descriptionLabel.Text = "A professional tool for managing SQL Server instances on Windows.";
        descriptionLabel.Font = new Font("Segoe UI", 10F);
        descriptionLabel.ForeColor = AppConstants.Colors.TextSecondary;
        descriptionLabel.AutoSize = true;
        descriptionLabel.Location = new Point(20, 85);
        descriptionLabel.MaximumSize = new Size(600, 0);
        tab.Controls.Add(descriptionLabel);
    }
    
    private ContextMenuStrip CreateContextMenu()
    {
        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Font = AppConstants.Fonts.Normal;
        
        ToolStripMenuItem viewDetailsItem = new ToolStripMenuItem("View Details");
        viewDetailsItem.Click += (s, e) => ViewDetails();
        menu.Items.Add(viewDetailsItem);
        
        menu.Items.Add(new ToolStripSeparator());
        
        ToolStripMenuItem enableTcpItem = new ToolStripMenuItem("Enable TCP/IP");
        enableTcpItem.Click += (s, e) => EnableTcpIp();
        menu.Items.Add(enableTcpItem);
        
        ToolStripMenuItem disableTcpItem = new ToolStripMenuItem("Disable TCP/IP");
        disableTcpItem.Click += (s, e) => DisableTcpIp();
        menu.Items.Add(disableTcpItem);
        
        menu.Items.Add(new ToolStripSeparator());
        
        ToolStripMenuItem startServiceItem = new ToolStripMenuItem("Start Service");
        startServiceItem.Click += (s, e) => StartService();
        menu.Items.Add(startServiceItem);
        
        ToolStripMenuItem stopServiceItem = new ToolStripMenuItem("Stop Service");
        stopServiceItem.Click += (s, e) => StopService();
        menu.Items.Add(stopServiceItem);
        
        ToolStripMenuItem restartServiceItem = new ToolStripMenuItem("Restart Service");
        restartServiceItem.Click += (s, e) => RestartService();
        menu.Items.Add(restartServiceItem);
        
        menu.Items.Add(new ToolStripSeparator());
        
        ToolStripMenuItem validateItem = new ToolStripMenuItem("Validate Instance");
        validateItem.Click += (s, e) => ValidateInstance();
        menu.Items.Add(validateItem);
        
        return menu;
    }
    
    private void MainForm_Resize(object sender, EventArgs e)
    {
        PositionRefreshButton();
    }
    
    private void LoadInstances()
    {
        try
        {
            instanceListView.Items.Clear();
            statusBar.SetStatus("Loading SQL Server instances...");
            
            List<SQLServerInfo> instances = sqlServerService.DiscoverInstances();
            
            if (instances == null || instances.Count == 0)
            {
                statusBar.SetStatus("No SQL Server instances found");
                return;
            }
            
            foreach (SQLServerInfo instance in instances)
            {
                ListViewItem item = new ListViewItem(instance.InstanceName ?? "Unknown");
                item.SubItems.Add(instance.Version ?? "Unknown");
                item.SubItems.Add(instance.Edition ?? "Unknown");
                item.SubItems.Add(instance.ServiceStatus ?? "Unknown");
                item.SubItems.Add(instance.TcpEnabled ? "Enabled" : "Disabled");
                item.SubItems.Add(instance.NamedPipesEnabled ? "Enabled" : "Disabled");
                item.SubItems.Add(instance.SharedMemoryEnabled ? "Enabled" : "Disabled");
                
                item.Tag = instance;
                
                if (instance.IsRunning())
                {
                    item.ForeColor = AppConstants.Colors.Success;
                }
                else
                {
                    item.ForeColor = AppConstants.Colors.Danger;
                }
                
                instanceListView.Items.Add(item);
            }
            
            statusBar.SetStatus(string.Format("Found {0} SQL Server instance(s)", instances.Count));
        }
        catch (Exception ex)
        {
            if (Logger != null)
            {
                Logger.LogError("Error loading instances", ex);
            }
            
            MessageBox.Show(
                "Failed to load SQL Server instances:\n\n" + ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            
            statusBar.SetStatus("Error loading instances");
        }
    }
    
    private void RefreshButton_Click(object sender, EventArgs e)
    {
        LoadInstances();
    }
    
    private SQLServerInfo GetSelectedInstance()
    {
        if (instanceListView.SelectedItems.Count == 0)
        {
            ShowWarning("Please select an instance first.");
            return null;
        }
        
        return (SQLServerInfo)instanceListView.SelectedItems[0].Tag;
    }
    
    private void ViewDetails()
    {
        SQLServerInfo selected = GetSelectedInstance();
        if (selected == null) return;
        
        try
        {
            SQLServerInstanceDetails details = detailsService.GetInstanceDetails(selected.InstanceName);
            InstanceDetailsForm detailsForm = new InstanceDetailsForm(details, Logger);
            detailsForm.ShowDialog();
            
            LoadInstances();
        }
        catch (Exception ex)
        {
            Logger.LogError("Error viewing details", ex);
            ShowError("Failed to load instance details: " + ex.Message);
        }
    }
    
    private void EnableTcpIp()
    {
        SQLServerInfo selected = GetSelectedInstance();
        if (selected == null) return;
        
        try
        {
            TcpIpConfigService tcpService = new TcpIpConfigService(Logger);
            bool success = tcpService.EnableAllTcpIp(selected.InstanceRegistryPath);
            
            if (success)
            {
                ShowSuccess("TCP/IP enabled!\n\nPlease restart the SQL Server service for changes to take effect.");
                LoadInstances();
            }
            else
            {
                ShowError("Failed to enable TCP/IP.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error enabling TCP/IP", ex);
            ShowError("Failed to enable TCP/IP: " + ex.Message);
        }
    }
    
    private void DisableTcpIp()
    {
        SQLServerInfo selected = GetSelectedInstance();
        if (selected == null) return;
        
        DialogResult result = AskConfirmation(
            "Are you sure you want to disable TCP/IP?\n\nThis will prevent remote connections.",
            "Confirm Disable"
        );
        
        if (result != DialogResult.Yes) return;
        
        try
        {
            TcpIpConfigService tcpService = new TcpIpConfigService(Logger);
            bool success = tcpService.DisableAllTcpIp(selected.InstanceRegistryPath);
            
            if (success)
            {
                ShowSuccess("TCP/IP disabled!\n\nPlease restart the SQL Server service for changes to take effect.");
                LoadInstances();
            }
            else
            {
                ShowError("Failed to disable TCP/IP.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error disabling TCP/IP", ex);
            ShowError("Failed to disable TCP/IP: " + ex.Message);
        }
    }
    
    private void StartService()
    {
        SQLServerInfo selected = GetSelectedInstance();
        if (selected == null) return;
        
        try
        {
            bool success = sqlServerService.StartService(selected.ServiceName);
            
            if (success)
            {
                ShowSuccess("Service started successfully!");
                LoadInstances();
            }
            else
            {
                ShowError("Failed to start service.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error starting service", ex);
            ShowError("Failed to start service: " + ex.Message);
        }
    }
    
    private void StopService()
    {
        SQLServerInfo selected = GetSelectedInstance();
        if (selected == null) return;
        
        DialogResult result = AskConfirmation(
            "Are you sure you want to stop this SQL Server instance?",
            "Confirm Stop"
        );
        
        if (result != DialogResult.Yes) return;
        
        try
        {
            bool success = sqlServerService.StopService(selected.ServiceName);
            
            if (success)
            {
                ShowSuccess("Service stopped successfully!");
                LoadInstances();
            }
            else
            {
                ShowError("Failed to stop service.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error stopping service", ex);
            ShowError("Failed to stop service: " + ex.Message);
        }
    }
    
    private void RestartService()
    {
        SQLServerInfo selected = GetSelectedInstance();
        if (selected == null) return;
        
        try
        {
            bool success = sqlServerService.RestartService(selected.ServiceName);
            
            if (success)
            {
                ShowSuccess("Service restarted successfully!");
                LoadInstances();
            }
            else
            {
                ShowError("Failed to restart service.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error restarting service", ex);
            ShowError("Failed to restart service: " + ex.Message);
        }
    }
    
    private void ValidateInstance()
    {
        SQLServerInfo selected = GetSelectedInstance();
        if (selected == null) return;
        
        try
        {
            SQLServerInstanceDetails details = detailsService.GetInstanceDetails(selected.InstanceName);
            InstanceDetailsForm detailsForm = new InstanceDetailsForm(details, Logger);
            detailsForm.ShowDialog();
            
            LoadInstances();
        }
        catch (Exception ex)
        {
            Logger.LogError("Error validating instance", ex);
            ShowError("Failed to validate instance: " + ex.Message);
        }
    }
    
    private void CreateSystemToolsTab(TabPage tab)
{
    tab.BackColor = Color.White;
    
    Panel contentPanel = new Panel();
    contentPanel.Dock = DockStyle.Fill;
    contentPanel.AutoScroll = true;
    contentPanel.Padding = new Padding(30, 20, 30, 20);
    tab.Controls.Add(contentPanel);
    
    int yPos = 0;
    
    // WMI Repair Section
    Label wmiHeaderLabel = new Label();
    wmiHeaderLabel.Text = "Windows Management Instrumentation (WMI) Repair";
    wmiHeaderLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
    wmiHeaderLabel.ForeColor = AppConstants.Colors.Primary;
    wmiHeaderLabel.Location = new Point(0, yPos);
    wmiHeaderLabel.AutoSize = true;
    contentPanel.Controls.Add(wmiHeaderLabel);
    yPos += 40;
    
    Label wmiDescLabel = new Label();
    wmiDescLabel.Text = "WMI is essential for SQL Server management. If WMI is corrupted, use this tool to repair it.";
    wmiDescLabel.Font = new Font("Segoe UI", 9F);
    wmiDescLabel.ForeColor = AppConstants.Colors.TextSecondary;
    wmiDescLabel.Location = new Point(0, yPos);
    wmiDescLabel.Size = new Size(800, 30);
    contentPanel.Controls.Add(wmiDescLabel);
    yPos += 40;
    
    // WMI Status Panel
    Panel wmiStatusPanel = new Panel();
    wmiStatusPanel.Location = new Point(0, yPos);
    wmiStatusPanel.Size = new Size(850, 80);
    wmiStatusPanel.BackColor = Color.FromArgb(245, 245, 245);
    wmiStatusPanel.BorderStyle = BorderStyle.FixedSingle;
    wmiStatusPanel.Name = "wmiStatusPanel";
    contentPanel.Controls.Add(wmiStatusPanel);
    
    Label wmiStatusLabel = new Label();
    wmiStatusLabel.Text = "Status:";
    wmiStatusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    wmiStatusLabel.Location = new Point(20, 15);
    wmiStatusLabel.Size = new Size(100, 25);
    wmiStatusPanel.Controls.Add(wmiStatusLabel);
    
    Label wmiStatusValue = new Label();
    wmiStatusValue.Name = "wmiStatusValue";
    wmiStatusValue.Text = "Checking...";
    wmiStatusValue.Font = new Font("Segoe UI", 10F);
    wmiStatusValue.ForeColor = AppConstants.Colors.TextSecondary;
    wmiStatusValue.Location = new Point(120, 15);
    wmiStatusValue.Size = new Size(700, 25);
    wmiStatusPanel.Controls.Add(wmiStatusValue);
    
    Button checkWmiButton = new Button();
    checkWmiButton.Text = "Check WMI Health";
    checkWmiButton.Size = new Size(150, 35);
    checkWmiButton.Location = new Point(20, 45);
    checkWmiButton.BackColor = AppConstants.Colors.Info;
    checkWmiButton.ForeColor = Color.White;
    checkWmiButton.FlatStyle = FlatStyle.Flat;
    checkWmiButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    checkWmiButton.Cursor = Cursors.Hand;
    checkWmiButton.Click += CheckWmiHealth_Click;
    wmiStatusPanel.Controls.Add(checkWmiButton);
    
    yPos += 95;
    
    // Repair Section
    Label repairHeaderLabel = new Label();
    repairHeaderLabel.Text = "WMI Repair Tool";
    repairHeaderLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
    repairHeaderLabel.ForeColor = AppConstants.Colors.Danger;
    repairHeaderLabel.Location = new Point(0, yPos);
    repairHeaderLabel.AutoSize = true;
    contentPanel.Controls.Add(repairHeaderLabel);
    yPos += 35;
    
    Label repairDescLabel = new Label();
    repairDescLabel.Text = "This will:\n" +
        "  1. Stop the WMI service\n" +
        "  2. Backup and reset the WMI repository\n" +
        "  3. Re-register all WMI DLL files\n" +
        "  4. Restart the WMI service\n" +
        "  5. Recompile all MOF configuration files\n\n" +
        "This process may take 2-5 minutes.";
    repairDescLabel.Font = new Font("Segoe UI", 9F);
    repairDescLabel.ForeColor = AppConstants.Colors.TextSecondary;
    repairDescLabel.Location = new Point(0, yPos);
    repairDescLabel.Size = new Size(800, 140);
    contentPanel.Controls.Add(repairDescLabel);
    yPos += 150;
    
    Button repairWmiButton = new Button();
    repairWmiButton.Text = "Repair WMI";
    repairWmiButton.Size = new Size(180, 45);
    repairWmiButton.Location = new Point(0, yPos);
    repairWmiButton.BackColor = AppConstants.Colors.Warning;
    repairWmiButton.ForeColor = Color.White;
    repairWmiButton.FlatStyle = FlatStyle.Flat;
    repairWmiButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
    repairWmiButton.Cursor = Cursors.Hand;
    repairWmiButton.Click += RepairWmi_Click;
    contentPanel.Controls.Add(repairWmiButton);
    yPos += 55;
    
    Label warningLabel = new Label();
    warningLabel.Text = "⚠ Warning: Requires Administrator privileges! Close all applications before running.";
    warningLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
    warningLabel.ForeColor = AppConstants.Colors.Danger;
    warningLabel.Location = new Point(0, yPos);
    warningLabel.Size = new Size(800, 30);
    contentPanel.Controls.Add(warningLabel);
    
    // Initial WMI check
    CheckWmiHealthAsync(wmiStatusValue);
}

private void CheckWmiHealthAsync(Label statusLabel)
{
    System.Threading.ThreadPool.QueueUserWorkItem(delegate
    {
        bool isHealthy = wmiRepairService.CheckWmiHealth();
        
        if (statusLabel.InvokeRequired)
        {
            statusLabel.Invoke(new Action(delegate
            {
                if (isHealthy)
                {
                    statusLabel.Text = "WMI is working correctly ✓";
                    statusLabel.ForeColor = AppConstants.Colors.Success;
                }
                else
                {
                    statusLabel.Text = "WMI has issues - Repair recommended ✗";
                    statusLabel.ForeColor = AppConstants.Colors.Danger;
                }
            }));
        }
        else
        {
            if (isHealthy)
            {
                statusLabel.Text = "WMI is working correctly ✓";
                statusLabel.ForeColor = AppConstants.Colors.Success;
            }
            else
            {
                statusLabel.Text = "WMI has issues - Repair recommended ✗";
                statusLabel.ForeColor = AppConstants.Colors.Danger;
            }
        }
    });
}

private void CheckWmiHealth_Click(object sender, EventArgs e)
{
    Button button = sender as Button;
    
    if (button != null)
    {
        button.Enabled = false;
        button.Text = "Checking...";
    }
    
    Panel statusPanel = this.Controls.Find("wmiStatusPanel", true)[0] as Panel;
    Label statusValue = statusPanel.Controls["wmiStatusValue"] as Label;
    
    CheckWmiHealthAsync(statusValue);
    
    System.Threading.ThreadPool.QueueUserWorkItem(delegate
    {
        System.Threading.Thread.Sleep(1000);
        
        if (button != null && button.InvokeRequired)
        {
            button.Invoke(new Action(delegate
            {
                button.Enabled = true;
                button.Text = "Check WMI Health";
            }));
        }
    });
}

private void RepairWmi_Click(object sender, EventArgs e)
{
    DialogResult result = MessageBox.Show(
        "⚠ WARNING: WMI Repair Process\n\n" +
        "This will completely rebuild the WMI repository.\n\n" +
        "BEFORE YOU CONTINUE:\n" +
        "- Close all applications\n" +
        "- Make sure you run this as Administrator\n" +
        "- This process takes 2-5 minutes\n" +
        "- Do NOT interrupt the process\n\n" +
        "Are you sure you want to continue?",
        "WMI Repair Confirmation",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning
    );
    
    if (result != DialogResult.Yes)
        return;
    
    Button button = sender as Button;
    
    if (button != null)
    {
        button.Enabled = false;
        button.Text = "Repairing... Please wait...";
    }
    
    statusBar.SetStatus("Repairing WMI... This may take several minutes...");
    
    System.Threading.ThreadPool.QueueUserWorkItem(delegate
    {
        bool success = wmiRepairService.RepairWmi();
        
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(delegate
            {
                if (success)
                {
                    MessageBox.Show(
                        "WMI repair completed successfully!\n\n" +
                        "Please restart your computer to ensure all changes take effect.",
                        "Repair Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    
                    statusBar.SetSuccess("WMI repair completed successfully");
                }
                else
                {
                    MessageBox.Show(
                        "WMI repair failed!\n\n" +
                        "Please check the log for details and make sure you run as Administrator.",
                        "Repair Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    
                    statusBar.SetError("WMI repair failed");
                }
                
                if (button != null)
                {
                    button.Enabled = true;
                    button.Text = "Repair WMI";
                }
            }));
        }
    });
}
}