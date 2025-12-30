using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

public class SectorSizeService
{
    private ILogService logger;
    
    public SectorSizeService(ILogService logService)
    {
        this.logger = logService;
    }
    
    public SectorSizeInfo GetSectorSizeInfo(string driveLetter)
    {
        SectorSizeInfo info = new SectorSizeInfo();
        info.DriveLetter = driveLetter;
        
        try
        {
            logger.Log("Checking sector size for drive " + driveLetter);
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "fsutil.exe";
            startInfo.Arguments = "fsinfo sectorinfo " + driveLetter + ":";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            
            Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                info.RawOutput = output;
                ParseSectorSizeOutput(output, info);
                
                logger.Log("Physical Bytes Per Sector For Atomicity: " + info.PhysicalBytesPerSectorForAtomicity);
                logger.Log("Physical Bytes Per Sector For Performance: " + info.PhysicalBytesPerSectorForPerformance);
                logger.Log("FileSystem Effective Physical Bytes: " + info.FileSystemEffectivePhysicalBytes);
                
                info.HasIssue = info.PhysicalBytesPerSectorForAtomicity > 4096 ||
                                info.PhysicalBytesPerSectorForPerformance > 4096 ||
                                info.FileSystemEffectivePhysicalBytes > 4096;
                
                if (info.HasIssue)
                {
                    logger.LogWarning("Sector size is greater than 4KB - SQL Server may have issues!");
                }
                else
                {
                    logger.LogSuccess("Sector size is optimal (4KB or less)");
                }
            }
            else
            {
                logger.LogError("Failed to get sector size info", new Exception(error));
                info.ErrorMessage = error;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error getting sector size", ex);
            info.ErrorMessage = ex.Message;
        }
        
        return info;
    }
    
    private void ParseSectorSizeOutput(string output, SectorSizeInfo info)
    {
        try
        {
            Match atomicityMatch = Regex.Match(output, @"PhysicalBytesPerSectorForAtomicity\s*:\s*(\d+)");
            if (atomicityMatch.Success)
            {
                info.PhysicalBytesPerSectorForAtomicity = int.Parse(atomicityMatch.Groups[1].Value);
            }
            
            Match performanceMatch = Regex.Match(output, @"PhysicalBytesPerSectorForPerformance\s*:\s*(\d+)");
            if (performanceMatch.Success)
            {
                info.PhysicalBytesPerSectorForPerformance = int.Parse(performanceMatch.Groups[1].Value);
            }
            
            Match effectiveMatch = Regex.Match(output, @"FileSystemEffectivePhysicalBytesPerSectorForAtomicity\s*:\s*(\d+)");
            if (effectiveMatch.Success)
            {
                info.FileSystemEffectivePhysicalBytes = int.Parse(effectiveMatch.Groups[1].Value);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error parsing sector size output", ex);
        }
    }
    
    public bool IsRegistryFixApplied()
    {
        try
        {
            string registryPath = @"SYSTEM\CurrentControlSet\Services\stornvme\Parameters\Device";
            
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath);
            
            if (key != null)
            {
                object value = key.GetValue("ForcedPhysicalSectorSizeInBytes");
                key.Close();
                
                if (value != null)
                {
                    logger.Log("Registry fix is already applied");
                    return true;
                }
            }
            
            logger.Log("Registry fix is NOT applied");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError("Error checking registry fix", ex);
            return false;
        }
    }
    
    public bool ApplyRegistryFix()
    {
        try
        {
            logger.Log("Applying registry fix for sector size...");
            
            string registryPath = @"SYSTEM\CurrentControlSet\Services\stornvme\Parameters\Device";
            
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath, true);
            
            if (key == null)
            {
                logger.Log("Creating registry path: " + registryPath);
                
                key = Registry.LocalMachine.CreateSubKey(registryPath);
                
                if (key == null)
                {
                    logger.LogError("Failed to create registry key", null);
                    return false;
                }
            }
            
            string[] values = new string[] { "* 4095" };
            
            key.SetValue("ForcedPhysicalSectorSizeInBytes", values, RegistryValueKind.MultiString);
            
            logger.Log("Registry key set: ForcedPhysicalSectorSizeInBytes = * 4095");
            
            key.Close();
            
            bool verified = VerifyRegistryFix();
            
            if (verified)
            {
                logger.LogSuccess("Registry fix applied successfully!");
                return true;
            }
            else
            {
                logger.LogError("Registry fix verification failed", null);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error applying registry fix", ex);
            return false;
        }
    }
    
    private bool VerifyRegistryFix()
    {
        try
        {
            logger.Log("Verifying registry fix...");
            
            string registryPath = @"SYSTEM\CurrentControlSet\Services\stornvme\Parameters\Device";
            
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath);
            
            if (key != null)
            {
                object value = key.GetValue("ForcedPhysicalSectorSizeInBytes");
                key.Close();
                
                if (value != null)
                {
                    logger.Log("Registry fix verified successfully");
                    return true;
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError("Error verifying registry fix", ex);
            return false;
        }
    }
}