using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Browsers;
using EvilBaschdi.Core.Wpf;
using MahApps.Metro.Controls;
using RenamePlayMemoriesImportFolders.Core;

namespace RenamePlayMemoriesImportFolders
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
    {
        private readonly IMetroStyle _style;
        private readonly IAppSettings _appSettings;
        private string _initialDirectory;
        private int _overrideProtection;
        private readonly BackgroundWorker _bw;
        private string _result;
        private int _executionCount;

        public MainWindow()
        {
            _appSettings = new AppSettings();
            var coreSettings = new CoreSettings();
            InitializeComponent();
            _bw = new BackgroundWorker();
            _style = new MetroStyle(this, Accent, Dark, Light, coreSettings);
            _style.Load();
            Load();
        }

        private void Load()
        {
            RenameFolders.IsEnabled = !string.IsNullOrWhiteSpace(_appSettings.InitialDirectory) && Directory.Exists(_appSettings.InitialDirectory);

            _initialDirectory = _appSettings.InitialDirectory;
            InitialDirectory.Text = _initialDirectory;

            RenameFolders.MouseRightButtonDown += RenameFoldersOnMouseRightButtonDown;
            _overrideProtection = 1;
        }

        private void RenameFoldersOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Process.Start($@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Sony\PlayMemories Home\PMBBrowser.exe");
            }
            else
            {
                Process.Start(_appSettings.InitialDirectory);
            }
        }

        private void RenameFoldersOnClick(object sender, RoutedEventArgs e)
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            ConfigureRename();
        }

        private void ConfigureRename()
        {
            _executionCount++;
            Cursor = Cursors.Wait;
            if (_executionCount == 1)
            {
                _bw.DoWork += (o, args) => Rename();
                _bw.WorkerReportsProgress = true;
                _bw.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
            }
            _bw.RunWorkerAsync();
        }

        private void Rename()
        {
            var pmFolder = _initialDirectory + @"\";
            var paths = Directory.GetDirectories(pmFolder);
            var counter = 0;

            foreach (var path in paths.Where(item => item.Split('\\').Last().Contains(".")))
            {
                var oldName = path.Replace(pmFolder, "").Replace(@"\", "");
                var values = oldName.Split('.');
                var year = values[2];
                var month = values[1];
                var day = values[0];
                var newName = $"{pmFolder}{year}-{month}-{day}";
                MoveDirectory(path, newName);
                counter++;
            }

            var pluralHelper = counter != 1
                ? "folders were"
                : "folder was";

            _result = counter != 0
                ? $"{counter} {pluralHelper} renamed.{Environment.NewLine}Open PlayMemories and reload the database."
                : "Nothing has changed.";
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RenameFoldersContent.Text = _result;

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            Cursor = Cursors.Arrow;
        }

        private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(InitialDirectory.Text))
            {
                _appSettings.InitialDirectory = InitialDirectory.Text;
                Load();
            }
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var browser = new ExplorerFolderBrower
                          {
                              SelectedPath = _initialDirectory
                          };
            browser.ShowDialog();
            _appSettings.InitialDirectory = browser.SelectedPath;
            Load();
        }

        private void MoveDirectory(string source, string target)
        {
            if (!Directory.Exists(target))
            {
                Directory.Move(source, target);
            }
            else
            {
                var sourcePath = source.TrimEnd('\\', ' ');
                var targetPath = target.TrimEnd('\\', ' ');
                var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).GroupBy(Path.GetDirectoryName);
                foreach (var folder in files)
                {
                    var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                    Directory.CreateDirectory(targetFolder);
                    foreach (var file in folder)
                    {
                        if (file != null)
                        {
                            var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                            if (File.Exists(targetFile))
                            {
                                File.Delete(targetFile);
                            }
                            File.Move(file, targetFile);
                        }
                    }
                }
                Directory.Delete(source, true);
            }
        }

        #region Flyout

        private void ToggleSettingsFlyoutClick(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(0);
        }

        private void ToggleFlyout(int index, bool stayOpen = false)
        {
            var activeFlyout = (Flyout) Flyouts.Items[index];
            if (activeFlyout == null)
            {
                return;
            }

            foreach (
                var nonactiveFlyout in
                    Flyouts.Items.Cast<Flyout>()
                           .Where(nonactiveFlyout => nonactiveFlyout.IsOpen && nonactiveFlyout.Name != activeFlyout.Name))
            {
                nonactiveFlyout.IsOpen = false;
            }

            if (activeFlyout.IsOpen && stayOpen)
            {
                activeFlyout.IsOpen = true;
            }
            else
            {
                activeFlyout.IsOpen = !activeFlyout.IsOpen;
            }
        }

        #endregion Flyout

        #region MetroStyle

        private void SaveStyleClick(object sender, RoutedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SaveStyle();
        }

        private void Theme(object sender, RoutedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SetTheme(sender, e);
        }

        private void AccentOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_overrideProtection == 0)
            {
                return;
            }
            _style.SetAccent(sender, e);
        }

        #endregion MetroStyle
    }
}