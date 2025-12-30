# Smart C# Compiler with Auto-Discovery and Error Logging
$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot
$errorLog = Join-Path $projectRoot "compile_errors.log"
$outputExe = Join-Path $projectRoot "SQLServerManager.exe"

# Clear previous error log
if (Test-Path $errorLog) { Remove-Item $errorLog }

function Write-Log {
    param([string]$message, [string]$color = "White")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $message"
    Add-Content -Path $errorLog -Value $logMessage
    Write-Host $message -ForegroundColor $color
}

function Get-CSharpFiles {
    param([string]$path)
    
    if (-not (Test-Path $path)) {
        Write-Log "WARNING: Path not found: $path" "Yellow"
        return @()
    }
    
    return Get-ChildItem -Path $path -Filter "*.cs" -Recurse | 
           Where-Object { $_.Name -ne "AssemblyInfo.cs" -and $_.Name -notlike "*.Designer.cs" } |
           Sort-Object FullName
}

function Get-CompilationOrder {
    $order = @(
        "Constants",
        "Models",
        "Services",
        "UI"
    )
    
    $orderedFiles = @()
    
    foreach ($category in $order) {
        $categoryPath = Join-Path $projectRoot $category
        $files = Get-CSharpFiles $categoryPath
        
        if ($files.Count -gt 0) {
            Write-Log "  Found $($files.Count) file(s) in $category" "Gray"
            $orderedFiles += $files
        }
    }
    
    $programFile = Join-Path $projectRoot "Program.cs"
    if (Test-Path $programFile) {
        $orderedFiles += Get-Item $programFile
        Write-Log "  Found Program.cs" "Gray"
    }
    
    return $orderedFiles
}

function Extract-Usings {
    param([string]$content)
    
    $usings = @()
    $lines = $content -split "`r?`n"
    
    foreach ($line in $lines) {
        $trimmed = $line.Trim()
        if ($trimmed -match '^using\s+[^;]+;$') {
            if ($usings -notcontains $trimmed) {
                $usings += $trimmed
            }
        }
    }
    
    return $usings
}

function Remove-Usings {
    param([string]$content)
    
    $lines = $content -split "`r?`n"
    $result = @()
    $foundNonUsing = $false
    
    foreach ($line in $lines) {
        $trimmed = $line.Trim()
        
        # Skip using statements
        if ($trimmed -match '^using\s+[^;]+;$') {
            continue
        }
        
        # Skip empty lines before first non-using line
        if (-not $foundNonUsing -and [string]::IsNullOrWhiteSpace($line)) {
            continue
        }
        
        # We've found content
        if (-not [string]::IsNullOrWhiteSpace($line)) {
            $foundNonUsing = $true
        }
        
        $result += $line
    }
    
    return ($result -join "`r`n").Trim()
}

function Test-FileContent {
    param([System.IO.FileInfo]$file)
    
    try {
        # Read with UTF-8
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        
        # Remove BOM if present
        if ($content.Length -gt 0 -and [int][char]$content[0] -eq 0xFEFF) {
            $content = $content.Substring(1)
        }
        
        if ([string]::IsNullOrWhiteSpace($content)) {
            Write-Log "WARNING: File is empty: $($file.Name)" "Yellow"
            return $null
        }
        
        return $content
    }
    catch {
        Write-Log "ERROR reading file $($file.Name): $($_.Exception.Message)" "Red"
        return $null
    }
}

# ============================================================================
# MAIN COMPILATION PROCESS
# ============================================================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SQL Server Manager - Smart Compiler" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Log "=== COMPILATION STARTED ===" "Cyan"
Write-Log "Project Root: $projectRoot" "Gray"
Write-Host ""

Write-Host "Discovering C# files..." -ForegroundColor Yellow
$allFiles = Get-CompilationOrder

if ($allFiles.Count -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  NO FILES FOUND!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Log "=== COMPILATION FAILED - NO FILES ==="
    pause
    exit 1
}

Write-Host ""
Write-Host "Found $($allFiles.Count) C# file(s) to compile" -ForegroundColor Green
Write-Log "Total files discovered: $($allFiles.Count)"
Write-Host ""

Write-Host "Reading source files..." -ForegroundColor Yellow
$allUsings = @()
$allCode = @()
$failedFiles = @()

foreach ($file in $allFiles) {
    $relativePath = $file.FullName.Replace($projectRoot, "").TrimStart("\")
    Write-Host "  Processing: $relativePath" -ForegroundColor Gray
    Write-Log "Reading: $relativePath"
    
    $content = Test-FileContent $file
    
    if ($null -eq $content) {
        $failedFiles += $relativePath
        continue
    }
    
    $usings = Extract-Usings $content
    foreach ($using in $usings) {
        if ($allUsings -notcontains $using) {
            $allUsings += $using
        }
    }
    
    $codeOnly = Remove-Usings $content
    if (-not [string]::IsNullOrWhiteSpace($codeOnly)) {
        $allCode += $codeOnly
    }
}

if ($failedFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  FAILED TO READ FILES!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "The following files could not be read:" -ForegroundColor Yellow
    foreach ($file in $failedFiles) {
        Write-Host "  - $file" -ForegroundColor Yellow
        Write-Log "Failed to read: $file"
    }
    Write-Host ""
    Write-Log "=== COMPILATION FAILED - FILE READ ERRORS ==="
    pause
    exit 1
}

Write-Host ""
Write-Host "Building final source..." -ForegroundColor Yellow
Write-Log "Unique using statements: $($allUsings.Count)"
Write-Log "Total code blocks: $($allCode.Count)"

# Sort usings alphabetically
$allUsings = $allUsings | Sort-Object

# Build final code with proper spacing
$usingBlock = ($allUsings -join "`r`n")
$codeBlock = ($allCode -join "`r`n`r`n")
$finalCode = $usingBlock + "`r`n`r`n" + $codeBlock

Write-Host ""
Write-Host "Compiling C# code..." -ForegroundColor Yellow
Write-Log "Starting C# compilation..."

try {
    $cscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
    
    if (-not (Test-Path $cscPath)) {
        throw "C# compiler (csc.exe) not found at: $cscPath"
    }
    
    Write-Log "Using compiler: $cscPath" "Gray"
    
    # Create temporary source file with UTF-8 WITHOUT BOM
    $tempSource = Join-Path $env:TEMP "SQLServerManager_temp.cs"
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($tempSource, $finalCode, $utf8NoBom)
    
    Write-Log "Temp source file: $tempSource" "Gray"
    Write-Log "Source file size: $((Get-Item $tempSource).Length / 1KB) KB" "Gray"
    
    $compilerArgs = @(
        "/target:winexe",
        "/out:$outputExe",
        "/reference:System.Windows.Forms.dll",
        "/reference:System.Drawing.dll",
        "/reference:System.ServiceProcess.dll",
        "/reference:Microsoft.CSharp.dll",
        "/reference:System.Data.dll",
        "/reference:System.Management.dll",
        "/reference:System.Core.dll",
        "/reference:System.dll",
        "/nologo",
        "/optimize+",
        "/platform:anycpu",
        "/warn:0",
        $tempSource
    )
    
    Write-Host "Invoking C# compiler..." -ForegroundColor Yellow
    
    $pinfo = New-Object System.Diagnostics.ProcessStartInfo
    $pinfo.FileName = $cscPath
    $pinfo.RedirectStandardError = $true
    $pinfo.RedirectStandardOutput = $true
    $pinfo.UseShellExecute = $false
    $pinfo.Arguments = $compilerArgs -join " "
    
    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $pinfo
    $p.Start() | Out-Null
    
    $stdout = $p.StandardOutput.ReadToEnd()
    $stderr = $p.StandardError.ReadToEnd()
    
    $p.WaitForExit()
    $exitCode = $p.ExitCode
    
    if ($exitCode -ne 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "  COMPILATION ERRORS:" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        Write-Host ""
        
        if ($stdout) {
            Write-Host $stdout -ForegroundColor Red
            Write-Log "COMPILER OUTPUT:"
            Write-Log $stdout
        }
        
        if ($stderr) {
            Write-Host $stderr -ForegroundColor Yellow
            Write-Log "COMPILER STDERR:"
            Write-Log $stderr
        }
        
        Write-Host ""
        Write-Host "Temp source file kept for debugging: $tempSource" -ForegroundColor Yellow
        Write-Log "Temp source kept: $tempSource"
        
        throw "Compilation failed with exit code $exitCode"
    }
    
    # Clean up temp file on success
    if (Test-Path $tempSource) {
        Remove-Item $tempSource
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  BUILD SUCCESS!" -ForegroundColor Green  
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output: SQLServerManager.exe" -ForegroundColor Cyan
    
    if (Test-Path $outputExe) {
        $size = (Get-Item $outputExe).Length / 1KB
        Write-Host "Size: $($size.ToString('0.00')) KB" -ForegroundColor Gray
        Write-Log "=== COMPILATION SUCCESS ==="
        Write-Log "Output: SQLServerManager.exe ($($size.ToString('0.00')) KB)"
    }
    
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  BUILD FAILED!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    
    $errorMessage = $_.Exception.Message
    Write-Host $errorMessage -ForegroundColor Red
    
    Write-Log "=== COMPILATION FAILED ==="
    Write-Log "Error: $errorMessage"
    
    Write-Host ""
    Write-Host "Error details saved to: compile_errors.log" -ForegroundColor Yellow
    Write-Host ""
    
    exit 1
}