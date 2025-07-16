using System;
using System.Windows;
using SizeChartGenerator.Services;
using SizeChartGenerator.ViewModels;

namespace SizeChartGenerator
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 初始化数据库
            DatabaseService.Instance.Initialize();
            
            // 创建并显示主窗口
            var mainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            mainWindow.Show();
        }
    }
}