using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;

namespace ConsoleBaseTool
{
    public class UpdateManager
    {
        public const string CurrentVersion = "1.0.0.0";
        private const string VersionInfoUrl = "https://gitee.com/BaseUser/ConsoleBaseTool/raw/main/Update";
        private const string DownloadUrl = "https://example.com/downloads/ConsoleBaseTool.exe";
        
        public static void CheckForUpdates()
        {
            try
            {   
                string latestVersion = GetLatestVersion();
                
                if (IsNewerVersion(latestVersion, CurrentVersion))
                {
                    Console.WriteLine($"发现新版本: {latestVersion}");
                    
                    var result = NativeMessageBox.Show(
                        $"发现新版本 {latestVersion}，是否更新？",
                        "程序更新",
                        NativeMessageBox.MessageBoxButtons.YesNo,
                        NativeMessageBox.MessageBoxIcon.Information);
                    
                    if (result == NativeMessageBox.MessageBoxResult.Yes)
                    {
                        GenerateUpdateScript();
                        Console.WriteLine("更新脚本已生成，将在程序退出后执行更新...");
                        Console.WriteLine("程序将退出以执行更新...");
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("用户选择暂不更新，程序将继续执行...");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查更新时出错: {ex.Message}");
            }
        }
        
        private static string GetLatestVersion()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("ConsoleBaseTool/" + CurrentVersion);
                    string response = client.GetStringAsync(VersionInfoUrl).Result;
                    string version = response.Trim();
                    Console.WriteLine($"从服务器获取到的版本号: {version}");
                    return version;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取最新版本号失败: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部错误: {ex.InnerException.Message}");
                }
                return !string.IsNullOrEmpty(CurrentVersion) ? CurrentVersion : "1.0.0.0";
            }
        }
        
        private static bool IsNewerVersion(string newVersion, string currentVersion)
        {
            try
            {
                Version newVer = new Version(newVersion);
                Version currVer = new Version(currentVersion);
                return newVer > currVer;
            }
            catch
            {
                return false;
            }
        }
        
        private static void GenerateUpdateScript()
        {
            string programPath = Process.GetCurrentProcess().MainModule.FileName;
            string programDirectory = Path.GetDirectoryName(programPath);
            string updateScriptPath = Path.Combine(programDirectory, "update.bat");
            string tempFilePath = Path.Combine(programDirectory, "temp_ConsoleBaseTool.exe");
            string mainExeFileName = Path.GetFileName(programPath);
            
            StringBuilder scriptContent = new StringBuilder();
            scriptContent.AppendLine("@echo off");
            scriptContent.AppendLine("chcp 65001 > nul");
            scriptContent.AppendLine("echo 开始更新程序...");
            scriptContent.AppendLine("echo 等待主程序退出...");
            scriptContent.AppendLine("ping -n 3 127.0.0.1 > nul");
            
            scriptContent.AppendLine($"powershell -Command \"Invoke-WebRequest -Uri '{DownloadUrl}' -OutFile '{tempFilePath}'\"");
            
            scriptContent.AppendLine("echo 替换程序文件...");
            scriptContent.AppendLine($"move /Y \"{tempFilePath}\" \"{mainExeFileName}\" > nul");
            
            scriptContent.AppendLine("echo 程序更新完成，启动程序...");
            scriptContent.AppendLine($"start \"\" \"{mainExeFileName}\"");
            
            scriptContent.AppendLine("echo 删除更新脚本...");
            scriptContent.AppendLine("del \"%0\"");
            
            File.WriteAllText(updateScriptPath, scriptContent.ToString(), Encoding.UTF8);
            Process.Start(updateScriptPath);
        }
    }
}