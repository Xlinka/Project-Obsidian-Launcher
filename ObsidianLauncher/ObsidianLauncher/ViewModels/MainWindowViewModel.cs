using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI.Fody.Helpers;

namespace ObsidianLauncher.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {

        [Reactive]
        public string StatusText { get; set; } = string.Empty;

        [Reactive]
        public string LauncherArguments { get; set; } = string.Empty;

        [Reactive]
        public bool InstallEnabled { get; set; } = true;

        public ReactiveCommand<Unit, Unit> InstallCommand { get; }
        public ReactiveCommand<Unit, Unit> LaunchCommand { get; }

        private MainWindow mainWindow;

        public MainWindowViewModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            // Load the configuration
            Config config = Config.LoadConfig();
            // Set the LauncherArguments from the configuration
            LauncherArguments = config.LauncherArguments;

            InitializeControls();

            InstallCommand = ReactiveCommand.CreateFromTask(ExecuteInstall);
            LaunchCommand = ReactiveCommand.Create(LaunchProjectObsidian);
        }

        private void InitializeControls()
        {

        }

        private async Task ExecuteInstall()
        {
            try
            {
                InstallEnabled = false;
                StatusText = "Checking for updates...";

                string[] projectObsidianPaths = ObsdiianPathHelper.GetObsidianPath();
                string projectObsidianPath = null;

                if (projectObsidianPaths.Length > 0)
                {
                    projectObsidianPath = projectObsidianPaths[0];
                }
                else
                {   //openfolderdialog is obsolete replace it soon.
                    var dialog = new OpenFolderDialog { Title = "Select ProjectObsidian Directory", Directory = "." };
                    var result = await dialog.ShowAsync(mainWindow);

                    if (result == null)
                    {
                        StatusText = "No ProjectObsidian directory selected.";
                        InstallEnabled = true;
                        return;
                    }

                    projectObsidianPath = result;

                    // Save the custom directory to the configuration
                    Config config = Config.LoadConfig();
                    config.CustomInstallDir = projectObsidianPath;
                    config.SaveConfig();
                }

                string projectObsidianPlusDirectory = Path.Combine(projectObsidianPath, "Libraries", "Obsidian");

                bool downloadSuccess = await DownloadProjectObsidianPlus(projectObsidianPath, projectObsidianPlusDirectory);

                if (!downloadSuccess)
                {
                    InstallEnabled = true;
                    return;
                }

                StatusText = "Done";
                InstallEnabled = true;
            }
            catch (Exception ex)
            {
                StatusText = $"Failed to execute install: {ex.Message}";
            }
        }

        private async Task<bool> DownloadProjectObsidianPlus(string projectObsidianPath, string projectObsidianPlusDirectory)
        {
            try
            {
                StatusText = "Downloading Obsidian...";
                InstallEnabled = false;

                DownloadResult res = await Download.DownloadAndInstallObsidian(projectObsidianPath, projectObsidianPlusDirectory);
                StatusText = res.Message;


                InstallEnabled = true;
                return res.Succes;
            }
            catch (Exception ex)
            {
                StatusText = $"Error during Obsidian download: {ex.Message}";
                return false;
            }
        }

        private void LaunchProjectObsidian()
        {
            string[] projectObsidianPaths = ObsdiianPathHelper.GetObsidianPath();

            if (projectObsidianPaths.Length == 0)
            {
                StatusText = "No ProjectObsidian directory found.";
                return;
            }

            var path = projectObsidianPaths[0];

            string projectObsidianPlusDirectory = Path.Combine(path, "Libraries", "Obsidian");

            LaunchProjectObsidian(path, projectObsidianPlusDirectory);

            StatusText = "Done";
        }
        private void LaunchProjectObsidian(string ObsidianPath, string projectObsidianPlusDirectory)
        {
            string projectObsidianExePath = Path.Combine(ObsidianPath, "Resonite.exe");
            string projectObsidianPlusDllPath = Path.Combine(projectObsidianPlusDirectory, "Project-Obsidian.dll");
            string arguments = $"-LoadAssembly \"{projectObsidianPlusDllPath}\"";

            // Get the value of the LauncherArgumentsTextBox and add it as an argument
            string launcherArguments = LauncherArguments.Trim();
            if (!string.IsNullOrEmpty(launcherArguments))
            {
                arguments += $" {launcherArguments}";
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(projectObsidianExePath, arguments);
            startInfo.WorkingDirectory = ObsidianPath;

            try
            {
                Process.Start(startInfo);

                // Save the configuration after launching Resonite
                Config config = Config.LoadConfig();
                config.LauncherArguments = launcherArguments;
                config.SaveConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to launch Resonite: {ex.Message}");
            }
        }
    }
}
