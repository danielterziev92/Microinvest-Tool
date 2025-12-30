using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class ValidationResultsForm : BaseForm
{
    private SQLServerValidation validation;
    private SQLServerInstanceDetails instanceDetails;
    private ValidationService validationService;
    private FirewallService firewallService;
    private TcpPortService portService;
    
    private Panel headerPanel;
    private Panel contentPanel;
    private Button closeButton;
    
    public ValidationResultsForm(SQLServerValidation validationResult, SQLServerInstanceDetails details, 
                                 ValidationService valService, FirewallService fwService, TcpPortService pService)
    {
        this.validation = validationResult;
        this.instanceDetails = details;
        this.validationService = valService;
        this.firewallService = fwService;
        this.portService = pService;
        
        InitializeComponents();
        LoadValidationResults();
    }
    
    private void InitializeComponents()
    {
        this.Text = "Validation Results - " + validation.InstanceName;
        this.Size = new Size(700, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        
        CreateHeader();
        CreateContent();
        CreateFooter();
    }
    
    private void CreateHeader()
    {
        headerPanel = new Panel();
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Height = 80;
        headerPanel.BackColor = validation.IsValid ? AppConstants.Colors.Success : AppConstants.Colors.Warning;
        this.Controls.Add(headerPanel);
        
        Label titleLabel = new Label();
        titleLabel.Text = validation.InstanceName;
        titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        titleLabel.ForeColor = Color.White;
        titleLabel.Location = new Point(20, 15);
        titleLabel.AutoSize = true;
        headerPanel.Controls.Add(titleLabel);
        
        Label statusLabel = new Label();
        statusLabel.Text = validation.IsValid ? "Status: VALID" : "Status: INVALID";
        statusLabel.Font = new Font("Segoe UI", 11F);
        statusLabel.ForeColor = Color.FromArgb(230, 230, 230);
        statusLabel.Location = new Point(20, 45);
        statusLabel.AutoSize = true;
        headerPanel.Controls.Add(statusLabel);
        
        // Summary counts
        int critical = 0, errors = 0, warnings = 0;
        foreach (ValidationIssue issue in validation.Issues)
        {
            if (issue.Severity == ValidationSeverity.Critical) critical++;
            else if (issue.Severity == ValidationSeverity.Error) errors++;
            else if (issue.Severity == ValidationSeverity.Warning) warnings++;
        }
        
        Label summaryLabel = new Label();
        summaryLabel.Text = string.Format("Critical: {0}  |  Errors: {1}  |  Warnings: {2}  |  Success: {3}",
            critical, errors, warnings, validation.Successes.Count);
        summaryLabel.Font = new Font("Segoe UI", 9F);
        summaryLabel.ForeColor = Color.White;
        summaryLabel.Location = new Point(400, 30);
        summaryLabel.AutoSize = true;
        headerPanel.Controls.Add(summaryLabel);
    }
    
    private void CreateContent()
    {
        contentPanel = new Panel();
        contentPanel.Location = new Point(0, 80);
        contentPanel.Size = new Size(684, 470);
        contentPanel.BackColor = AppConstants.Colors.Background;
        contentPanel.AutoScroll = true;
        this.Controls.Add(contentPanel);
        
        int yPos = 10;
        
        // Issues Section
        if (validation.Issues.Count > 0)
        {
            Label issuesHeader = new Label();
            issuesHeader.Text = "ISSUES FOUND:";
            issuesHeader.Font = AppConstants.Fonts.Subheading;
            issuesHeader.ForeColor = AppConstants.Colors.Danger;
            issuesHeader.Location = new Point(20, yPos);
            issuesHeader.AutoSize = true;
            contentPanel.Controls.Add(issuesHeader);
            yPos += 35;
            
            foreach (ValidationIssue issue in validation.Issues)
            {
                Panel issueCard = CreateIssueCard(issue, yPos);
                contentPanel.Controls.Add(issueCard);
                yPos += issueCard.Height + 10;
            }
            
            yPos += 20;
        }
        
        // Successes Section
        if (validation.Successes.Count > 0)
        {
            Label successHeader = new Label();
            successHeader.Text = "VALIDATED SUCCESSFULLY:";
            successHeader.Font = AppConstants.Fonts.Subheading;
            successHeader.ForeColor = AppConstants.Colors.Success;
            successHeader.Location = new Point(20, yPos);
            successHeader.AutoSize = true;
            contentPanel.Controls.Add(successHeader);
            yPos += 35;
            
            foreach (ValidationSuccess success in validation.Successes)
            {
                Panel successCard = CreateSuccessCard(success, yPos);
                contentPanel.Controls.Add(successCard);
                yPos += successCard.Height + 10;
            }
        }
    }
    
    private Panel CreateIssueCard(ValidationIssue issue, int yPos)
    {
        Panel card = new Panel();
        card.Location = new Point(20, yPos);
        card.Size = new Size(640, 80);
        card.BackColor = Color.White;
        card.BorderStyle = BorderStyle.FixedSingle;
        
        // Severity badge
        Label severityBadge = new Label();
        severityBadge.Text = issue.Severity.ToString().ToUpper();
        severityBadge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        severityBadge.ForeColor = Color.White;
        severityBadge.Location = new Point(10, 10);
        severityBadge.Size = new Size(80, 25);
        severityBadge.TextAlign = ContentAlignment.MiddleCenter;
        
        switch (issue.Severity)
        {
            case ValidationSeverity.Critical:
                severityBadge.BackColor = Color.FromArgb(139, 0, 0);
                break;
            case ValidationSeverity.Error:
                severityBadge.BackColor = AppConstants.Colors.Danger;
                break;
            case ValidationSeverity.Warning:
                severityBadge.BackColor = Color.FromArgb(255, 140, 0);
                break;
            default:
                severityBadge.BackColor = AppConstants.Colors.Info;
                break;
        }
        
        card.Controls.Add(severityBadge);
        
        // Category
        Label categoryLabel = new Label();
        categoryLabel.Text = issue.Category;
        categoryLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        categoryLabel.ForeColor = AppConstants.Colors.TextPrimary;
        categoryLabel.Location = new Point(100, 10);
        categoryLabel.AutoSize = true;
        card.Controls.Add(categoryLabel);
        
        // Message
        Label messageLabel = new Label();
        messageLabel.Text = issue.Message;
        messageLabel.Font = AppConstants.Fonts.Normal;
        messageLabel.ForeColor = AppConstants.Colors.TextSecondary;
        messageLabel.Location = new Point(100, 35);
        messageLabel.MaximumSize = new Size(420, 0);
        messageLabel.AutoSize = true;
        card.Controls.Add(messageLabel);
        
        // Action button (if applicable)
        Button actionButton = GetActionButton(issue);
        if (actionButton != null)
        {
            actionButton.Location = new Point(530, 20);
            actionButton.Size = new Size(90, 35);
            card.Controls.Add(actionButton);
        }
        
        return card;
    }
    
    private Button GetActionButton(ValidationIssue issue)
    {
        Button button = null;
        
        // Firewall issue
        if (issue.Category == "Firewall" && issue.Message.Contains("No firewall rule found"))
        {
            button = new Button();
            button.Text = "Create Rule";
            button.BackColor = AppConstants.Colors.Success;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Click += (s, e) => CreateFirewallRule();
        }
        // TCP/IP disabled
        else if (issue.Category == "Network" && issue.Message.Contains("TCP/IP protocol is disabled"))
        {
            button = new Button();
            button.Text = "Enable";
            button.BackColor = AppConstants.Colors.Success;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Click += (s, e) => EnableTcpIp();
        }
        // Service stopped
        else if (issue.Category == "Service" && issue.Message.Contains("stopped"))
        {
            button = new Button();
            button.Text = "Start Service";
            button.BackColor = AppConstants.Colors.Success;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Click += (s, e) => StartService();
        }
        // Port configuration
        else if (issue.Category == "Port" && issue.Message.Contains("No static port"))
        {
            button = new Button();
            button.Text = "Set Port";
            button.BackColor = AppConstants.Colors.Info;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Click += (s, e) => SetPort();
        }
        
        return button;
    }
    
    private Panel CreateSuccessCard(ValidationSuccess success, int yPos)
    {
        Panel card = new Panel();
        card.Location = new Point(20, yPos);
        card.Size = new Size(640, 50);
        card.BackColor = Color.FromArgb(240, 255, 240);
        card.BorderStyle = BorderStyle.FixedSingle;
        
        // Check icon
        Label checkIcon = new Label();
        checkIcon.Text = "OK";
        checkIcon.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        checkIcon.ForeColor = AppConstants.Colors.Success;
        checkIcon.Location = new Point(15, 12);
        checkIcon.Size = new Size(30, 25);
        card.Controls.Add(checkIcon);
        
        // Category
        Label categoryLabel = new Label();
        categoryLabel.Text = success.Category;
        categoryLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        categoryLabel.ForeColor = AppConstants.Colors.TextPrimary;
        categoryLabel.Location = new Point(55, 8);
        categoryLabel.AutoSize = true;
        card.Controls.Add(categoryLabel);
        
        // Message
        Label messageLabel = new Label();
        messageLabel.Text = success.Message;
        messageLabel.Font = AppConstants.Fonts.Small;
        messageLabel.ForeColor = AppConstants.Colors.TextSecondary;
        messageLabel.Location = new Point(55, 28);
        messageLabel.AutoSize = true;
        card.Controls.Add(messageLabel);
        
        return card;
    }
    
    private void CreateFooter()
    {
        closeButton = new Button();
        closeButton.Text = "Close";
        closeButton.Location = new Point(580, 560);
        closeButton.Size = new Size(100, 35);
        closeButton.BackColor = AppConstants.Colors.TextSecondary;
        closeButton.ForeColor = Color.White;
        closeButton.FlatStyle = FlatStyle.Flat;
        closeButton.Font = AppConstants.Fonts.Button;
        closeButton.Click += (s, e) => this.Close();
        this.Controls.Add(closeButton);
    }
    
    private void LoadValidationResults()
    {
        // Results already loaded in CreateContent
    }
    
    // Action handlers
    private void CreateFirewallRule()
    {
        int port = portService.GetPrimaryPort(instanceDetails.InstanceRegistryPath);
        
        if (port == 0)
        {
            ShowError("Cannot create firewall rule.\n\nNo static port configured.");
            return;
        }
        
        bool success = firewallService.CreateFirewallRule(port, instanceDetails.InstanceName);
        
        if (success)
        {
            ShowSuccess(string.Format("Firewall rule created for port {0}!", port));
            RefreshValidation();
        }
        else
        {
            ShowError("Failed to create firewall rule.");
        }
    }
    
    private void EnableTcpIp()
    {
        ShowWarning("TCP/IP will be enabled.\n\nPlease close this dialog and use the context menu to enable TCP/IP.");
        this.Close();
    }
    
    private void StartService()
    {
        ShowWarning("Service will be started.\n\nPlease close this dialog and use the context menu to start the service.");
        this.Close();
    }
    
    private void SetPort()
    {
        ShowWarning("Port configuration.\n\nPlease close this dialog and use 'Change Port' in the context menu.");
        this.Close();
    }
    
    private void RefreshValidation()
    {
        SQLServerValidation newValidation = validationService.ValidateInstance(instanceDetails);
        
        // Clear and reload
        contentPanel.Controls.Clear();
        validation = newValidation;
        
        // Update header
        headerPanel.BackColor = validation.IsValid ? AppConstants.Colors.Success : AppConstants.Colors.Warning;
        
        // Reload content
        CreateContent();
    }
}