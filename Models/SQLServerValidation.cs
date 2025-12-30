using System.Collections.Generic;

public class SQLServerValidation
{
    public string InstanceName { get; set; }
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; }
    public List<ValidationSuccess> Successes { get; set; }
    
    public SQLServerValidation()
    {
        Issues = new List<ValidationIssue>();
        Successes = new List<ValidationSuccess>();
    }
    
    public void AddIssue(string category, string message, ValidationSeverity severity)
    {
        Issues.Add(new ValidationIssue 
        { 
            Category = category, 
            Message = message, 
            Severity = severity 
        });
        
        if (severity == ValidationSeverity.Critical || severity == ValidationSeverity.Error)
        {
            IsValid = false;
        }
    }
    
    public void AddSuccess(string category, string message)
    {
        Successes.Add(new ValidationSuccess 
        { 
            Category = category, 
            Message = message 
        });
    }
    
    public string GetSummary()
    {
        int critical = 0;
        int errors = 0;
        int warnings = 0;
        
        foreach (ValidationIssue issue in Issues)
        {
            if (issue.Severity == ValidationSeverity.Critical)
                critical++;
            else if (issue.Severity == ValidationSeverity.Error)
                errors++;
            else if (issue.Severity == ValidationSeverity.Warning)
                warnings++;
        }
        
        return string.Format("Instance: {0}\nStatus: {1}\nIssues: {2} Critical, {3} Errors, {4} Warnings\nSuccesses: {5}",
            InstanceName,
            IsValid ? "VALID" : "INVALID",
            critical,
            errors,
            warnings,
            Successes.Count);
    }
}

public class ValidationIssue
{
    public string Category { get; set; }
    public string Message { get; set; }
    public ValidationSeverity Severity { get; set; }
}

public class ValidationSuccess
{
    public string Category { get; set; }
    public string Message { get; set; }
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}