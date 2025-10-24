using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;

namespace ConsoleBaseTool
{
    public class UpdateManager
    {
        public const string CurrentVersion = null;
        private const string InfoUrl = "https://gitee.com/BaseUser/ConsoleBaseTool/raw/main/Update";

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
                    string response = client.GetStringAsync(InfoUrl).Result;
                    
                    // 解析响应内容：第一行是版本号，第二行是下载链接
                    string[] lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (lines.Length >= 1)
                    {
                        string version = lines[0].Trim();
                        Console.WriteLine($"从服务器获取到的版本号: {version}");
                        
                        // 如果有第二行，则更新下载链接
                        if (lines.Length >= 2)
                        {
                            string newDownloadUrl = lines[1].Trim();
                            if (!string.IsNullOrEmpty(newDownloadUrl))
                            {
                                typeof(UpdateManager).GetField("DownloadUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, newDownloadUrl);
                                Console.WriteLine($"从服务器获取到的下载链接: {newDownloadUrl}");
                            }
                        }
                        
                        return version;
                    }
                    
                    return !string.IsNullOrEmpty(CurrentVersion) ? CurrentVersion : "1.0.0.0";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取新版本文件时失败");
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
            
            // 获取当前的下载链接
            string currentDownloadUrl = (string)typeof(UpdateManager).GetField("DownloadUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            
            // 检查下载链接是否有效
            if (string.IsNullOrEmpty(currentDownloadUrl))
            {
                Console.WriteLine("下载链接无效，无法生成更新脚本。");
                return;
            }
            
            StringBuilder scriptContent = new StringBuilder();
            scriptContent.AppendLine("@echo off");
            scriptContent.AppendLine("chcp 65001 > nul");
            scriptContent.AppendLine("echo 开始更新程序...");
            scriptContent.AppendLine("echo 等待主程序退出...");
            scriptContent.AppendLine("ping -n 3 127.0.0.1 > nul");
            
            scriptContent.AppendLine($"powershell -Command \"Invoke-WebRequest -Uri '{currentDownloadUrl}' -OutFile '{tempFilePath}' -UseBasicParsing\"");
            
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