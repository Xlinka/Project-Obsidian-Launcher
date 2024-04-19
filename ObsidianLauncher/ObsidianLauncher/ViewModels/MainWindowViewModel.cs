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
            LaunchCommand = ReactiveCommand.Create(LaunchNeosPlus);
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

                string[] neosPaths = ObsdiianPathHelper.GetObsidianPath();
                string neosPath = null;

                if (neosPaths.Length > 0)
                {
                    neosPath = neosPaths[0];
                }
                else
                {   //openfolderdialog is obsolete replace it soon.
                    var dialog = new OpenFolderDialog { Title = "Select Resonite Directory", Directory = "." };
                    var result = await dialog.ShowAsync(mainWindow);

                    if (result == null)
                    {
                        StatusText = "No Resonite directory selected.";
                        InstallEnabled = true;
                        return;
                    }

                    neosPath = result;

                    // Save the custom directory to the configuration
                    Config config = Config.LoadConfig();
                    config.CustomInstallDir = neosPath;
                    config.SaveConfig();
                }

                string neosPlusDirectory = Path.Combine(neosPath, "Libraries", "Obsidian");

                bool downloadSuccess = await DownloadNeosPlus(neosPath, neosPlusDirectory);

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

        private async Task<bool> DownloadNeosPlus(string neosPath, string neosPlusDirectory)
        {
            try
            {
                StatusText = "Downloading Obsidian...";
                InstallEnabled = false;
                
                DownloadResult res = await Download.DownloadAndInstallObsidian(neosPath, neosPlusDirectory);
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

        private void LaunchNeosPlus()
        {
            string[] neosPaths = ObsdiianPathHelper.GetObsidianPath();

            if (neosPaths.Length == 0)
            {
                StatusText = "No Resonite directory found.";
                return;
            }

            var path = neosPaths[0];

            string neosPlusDirectory = Path.Combine(path, "Libraries", "Obsidian");

            LaunchNeosPlus(path, neosPlusDirectory);

            StatusText = "Done";
        }
        private void LaunchNeosPlus(string ObsidianPath, string neosPlusDirectory)
        {
            string neosExePath = Path.Combine(ObsidianPath, "Obsidian.exe");
            string neosPlusDllPath = Path.Combine(neosPlusDirectory, "Obsidian.dll");
            string arguments = $"-LoadAssembly \"{neosPlusDllPath}\"";

            // Get the value of the LauncherArgumentsTextBox and add it as an argument
            string launcherArguments = LauncherArguments.Trim();
            if (!string.IsNullOrEmpty(launcherArguments))
            {
                arguments += $" {launcherArguments}";
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(neosExePath, arguments);
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
