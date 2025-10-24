using System;
using System.Reflection;

namespace ConsoleBaseTool {
    internal class Program {
        static void Main(string[] args) {
            // 获取程序集版本号
            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine($"发布版本号: {assemblyVersion}");
            // 检查更新
            UpdateManager.CheckForUpdates();

            // 这里可以添加原有的程序代码
            
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}
