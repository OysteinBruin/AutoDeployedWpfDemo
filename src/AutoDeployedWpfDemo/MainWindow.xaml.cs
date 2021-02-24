using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoDeployedWpfDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AddVersionNumber();
            CheckForUpdates();
        }



        private void AddVersionNumber()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            this.Title += $" v{ versionInfo.FileVersion }";
        }

        private async Task CheckForUpdates()
        {
            string urlOrPath;
            bool isDevelopment = false;

            if (isDevelopment)
            {
                urlOrPath = @"D:\Dev\C#\ProjectsReleased\SquirrelTest\Releases";
            }
            else
            {
                urlOrPath = @"https://autodeployedwpfdemo.blob.core.windows.net/releases";
            }
            using (var manager = new UpdateManager(urlOrPath))
            {
                await manager.UpdateApp();
            }
        }
    }
}
