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
            using (var manager = new UpdateManager("https://autodeployedwpfdemo.blob.core.windows.net/src/"))
            {
                UpdateInfo info = await manager.CheckForUpdate();
                var sb = new StringBuilder("Check for updates result:");

                if (info != null)
                {
                    var releaseNotes = info.FetchReleaseNotes();
                    foreach (var item in releaseNotes)
                    {
                        sb.Append("Package ");
                        sb.Append(item.Key.PackageName);
                        sb.Append(" - File ");
                        sb.AppendLine(item.Key.Filename);
                    }

                    

                   // ReleaseEntry entry = await manager.UpdateApp();


                }
                else
                {
                    sb.AppendLine("No updates avavilable.");
                }

                this.updateInfoText.Text = sb.ToString();
            }
        }
    }
}
