using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Collections.Generic;

public class WmiRepairService
{
    private ILogService logger;
    
    public WmiRepairService(ILogService logService)
    {
        this.logger = logService;
    }
    
    public bool CheckWmiHealth()
    {
        try
        {
            logger.Log("Checking WMI health...");
            
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectCollection results = searcher.Get();
            
            foreach (ManagementObject result in results)
            {
                string caption = result["Caption"].ToString();
                logger.LogSuccess("WMI is working - OS: " + caption);
                result.Dispose();
                
                results.Dispose();
                searcher.Dispose();
                
                return true;
            }
            
            results.Dispose();
            searcher.Dispose();
            
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError("WMI health check failed", ex);
            return false;
        }
    }
    
    public bool RepairWmi()
    {
        try
        {
            logger.LogHeader("STARTING WMI REPAIR PROCESS");
            
            // Step 1: Stop WMI service
            if (!StopWmiService())
            {
                logger.LogError("Failed to stop WMI service", null);
                return false;
            }
            
            System.Threading.Thread.Sleep(2000);
            
            // Step 2: Handle Repository backup/delete
            if (!HandleRepositoryBackup())
            {
                logger.LogError("Failed to handle repository backup", null);
                return false;
            }
            
            // Step 3: Re-register DLLs
            if (!ReregisterDlls())
            {
                logger.LogError("Failed to re-register DLLs", null);
                return false;
            }
            
            // Step 4: Start WMI service
            if (!StartWmiService())
            {
                logger.LogError("Failed to start WMI service", null);
                return false;
            }
            
            System.Threading.Thread.Sleep(3000);
            
            // Step 5: Recompile MOF files
            if (!RecompileMofFiles())
            {
                logger.LogWarning("MOF recompilation had some errors, but WMI should still work");
            }
            
            logger.LogSuccess("WMI REPAIR COMPLETED SUCCESSFULLY!");
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("WMI repair failed", ex);
            return false;
        }
    }
    
    private bool StopWmiService()
    {
        Process process = null;
        
        try
        {
            logger.Log("Stopping WMI service...");
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "net";
            startInfo.Arguments = "stop winmgmt";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.Verb = "runas";
            
            process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            
            process.WaitForExit(30000);
            
            if (process.ExitCode == 0)
            {
                logger.LogSuccess("WMI service stopped");
                return true;
            }
            else
            {
                logger.LogWarning("WMI service stop returned code: " + process.ExitCode);
                logger.Log("Output: " + output);
                logger.Log("Error: " + error);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error stopping WMI service", ex);
            return false;
        }
        finally
        {
            if (process != null)
            {
                process.Dispose();
            }
        }
    }
    
    private bool HandleRepositoryBackup()
    {
        try
        {
            string wbemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "wbem");
            string repositoryPath = Path.Combine(wbemPath, "Repository");
            string oldRepositoryPath = Path.Combine(wbemPath, "Repository.old");
            
            logger.Log("WBEM Path: " + wbemPath);
            logger.Log("Repository Path: " + repositoryPath);
            
            // Check if Repository.old exists and delete it
            if (Directory.Exists(oldRepositoryPath))
            {
                logger.Log("Found existing Repository.old, deleting...");
                
                try
                {
                    Directory.Delete(oldRepositoryPath, true);
                    logger.LogSuccess("Deleted old Repository.old");
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to delete Repository.old", ex);
                    
                    logger.Log("Attempting forceful delete with cmd...");
                    
                    if (!ForceDeleteDirectory(oldRepositoryPath))
                    {
                        return false;
                    }
                }
            }
            
            // Rename Repository to Repository.old
            if (Directory.Exists(repositoryPath))
            {
                logger.Log("Renaming Repository to Repository.old...");
                
                Directory.Move(repositoryPath, oldRepositoryPath);
                
                logger.LogSuccess("Repository renamed successfully");
            }
            else
            {
                logger.LogWarning("Repository folder not found - it may have been deleted already");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Error handling repository backup", ex);
            return false;
        }
    }
    
    private bool ForceDeleteDirectory(string path)
    {
        Process process = null;
        
        try
        {
            logger.Log("Force deleting: " + path);
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c rmdir /s /q \"" + path + "\"";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.Verb = "runas";
            
            process = Process.Start(startInfo);
            process.WaitForExit(30000);
            
            if (process.ExitCode == 0 || !Directory.Exists(path))
            {
                logger.LogSuccess("Directory deleted successfully");
                return true;
            }
            else
            {
                logger.LogError("Failed to delete directory", null);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error force deleting directory", ex);
            return false;
        }
        finally
        {
            if (process != null)
            {
                process.Dispose();
            }
        }
    }
    
    private bool ReregisterDlls()
    {
        Process process = null;
        
        try
        {
            logger.Log("Re-registering WMI DLLs...");
            
            string wbemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "wbem");
            
            string[] dllFiles = Directory.GetFiles(wbemPath, "*.dll", SearchOption.AllDirectories);
            
            logger.Log("Found " + dllFiles.Length + " DLL files to register");
            
            int successCount = 0;
            int failCount = 0;
            
            foreach (string dllFile in dllFiles)
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "regsvr32";
                    startInfo.Arguments = "/s \"" + dllFile + "\"";
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.Verb = "runas";
                    
                    process = Process.Start(startInfo);
                    process.WaitForExit(5000);
                    
                    if (process.ExitCode == 0)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                    
                    process.Dispose();
                    process = null;
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Failed to register: " + Path.GetFileName(dllFile) + " - " + ex.Message);
                    failCount++;
                }
            }
            
            logger.Log("DLL Registration: " + successCount + " succeeded, " + failCount + " failed");
            
            if (successCount > 0)
            {
                logger.LogSuccess("DLL re-registration completed");
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error re-registering DLLs", ex);
            return false;
        }
        finally
        {
            if (process != null)
            {
                process.Dispose();
            }
        }
    }
    
    private bool StartWmiService()
    {
        Process process = null;
        
        try
        {
            logger.Log("Starting WMI service...");
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "net";
            startInfo.Arguments = "start winmgmt";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.Verb = "runas";
            
            process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            
            process.WaitForExit(30000);
            
            if (process.ExitCode == 0)
            {
                logger.LogSuccess("WMI service started");
                return true;
            }
            else
            {
                logger.LogWarning("WMI service start returned code: " + process.ExitCode);
                logger.Log("Output: " + output);
                logger.Log("Error: " + error);
                
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error starting WMI service", ex);
            return false;
        }
        finally
        {
            if (process != null)
            {
                process.Dispose();
            }
        }
    }
    
    private bool RecompileMofFiles()
    {
        Process process = null;
        
        try
        {
            logger.Log("Recompiling MOF files...");
            
            string systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
            
            List<string> mofFiles = new List<string>();
            
            FindMofFiles(systemDrive, mofFiles);
            
            logger.Log("Found " + mofFiles.Count + " MOF/MFL files to recompile");
            
            int successCount = 0;
            int failCount = 0;
            
            foreach (string mofFile in mofFiles)
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "mofcomp";
                    startInfo.Arguments = "\"" + mofFile + "\"";
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                    
                    process = Process.Start(startInfo);
                    process.WaitForExit(10000);
                    
                    if (process.ExitCode == 0)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                    
                    process.Dispose();
                    process = null;
                }
                catch (Exception ex)
                {
                    logger.LogWarning("Failed to compile: " + Path.GetFileName(mofFile) + " - " + ex.Message);
                    failCount++;
                }
            }
            
            logger.Log("MOF Compilation: " + successCount + " succeeded, " + failCount + " failed");
            
            logger.LogSuccess("MOF recompilation completed");
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Error recompiling MOF files", ex);
            return false;
        }
        finally
        {
            if (process != null)
            {
                process.Dispose();
            }
        }
    }
    
    private void FindMofFiles(string path, List<string> mofFiles)
    {
        try
        {
            string[] files = Directory.GetFiles(path, "*.mof");
            mofFiles.AddRange(files);
            
            files = Directory.GetFiles(path, "*.mfl");
            mofFiles.AddRange(files);
            
            string[] directories = Directory.GetDirectories(path);
            
            foreach (string directory in directories)
            {
                try
                {
                    FindMofFiles(directory, mofFiles);
                }
                catch
                {
                    // Skip inaccessible directories
                }
            }
        }
        catch
        {
            // Skip inaccessible paths
        }
    }
}