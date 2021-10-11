using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.CoreExtended;
using EvilBaschdi.CoreExtended.AppHelpers;
using EvilBaschdi.CoreExtended.Browsers;
using EvilBaschdi.CoreExtended.Controls.About;
using MahApps.Metro.Controls;
using RenamePlayMemoriesImportFolders.Core;
using RenamePlayMemoriesImportFolders.Internal;
using RenamePlayMemoriesImportFolders.Properties;

namespace RenamePlayMemoriesImportFolders
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
    {
        private readonly IAppSettings _appSettings;
        private readonly IGenerateNewPath _generateNewPath;
        private readonly IMoveDirectory _moveDirectory;
        private readonly IProcessByPath _processByPath;
        private readonly IRoundCorners _roundCorners;


        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            IAppSettingsBase appSettingsBase = new AppSettingsBase(Settings.Default);
            _roundCorners = new RoundCorners();
            IApplicationStyle style = new ApplicationStyle(_roundCorners, true);
            style.Run();
            _appSettings = new AppSettings(appSettingsBase);
            _moveDirectory = new MoveDirectory();
            _processByPath = new ProcessByPath();
            _generateNewPath = new GenerateNewPath(_appSettings);
            Load();
        }

        private void Load()
        {
            RenameFolders.IsEnabled = !string.IsNullOrWhiteSpace(_appSettings.InitialDirectory) && Directory.Exists(_appSettings.InitialDirectory);
            InitialDirectory.Text = _appSettings.InitialDirectory ?? string.Empty;
            RenameFolders.MouseRightButtonDown += RenameFoldersOnMouseRightButtonDown;
        }

        private void RenameFoldersOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\Sony\PlayMemories Home\x64\PMBBrowser.exe";
                _processByPath.RunFor(path);
            }
            else
            {
                _processByPath.RunFor(_appSettings.InitialDirectory);
            }
        }

        private async void RenameFoldersOnClick(object sender, RoutedEventArgs e)
        {
            await RunRenameAsync();
        }

        private async Task RunRenameAsync()
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            Cursor = Cursors.Wait;

            var task = Task<string>.Factory.StartNew(Rename);
            await task;

            RenameFoldersContent.Text = task.Result;

            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
            Cursor = Cursors.Arrow;
        }

        private string Rename()
        {
            var pmFolder = $@"{_appSettings.InitialDirectory}\";
            var paths = Directory.GetDirectories(pmFolder);
            var counter = 0;

            foreach (var path in paths.Where(item => item.Split('\\').Last().Contains(".")))
            {
                var newName = _generateNewPath.ValueFor(path);
                _moveDirectory.RunFor(path, newName);
                counter++;
            }

            var pluralHelper = counter != 1
                ? "folders were"
                : "folder was";

            return counter != 0
                ? $"{counter} {pluralHelper} renamed.{Environment.NewLine}Open PlayMemories and reload the database."
                : "Nothing has changed.";
        }

        private void InitialDirectoryOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(InitialDirectory.Text))
            {
                return;
            }

            _appSettings.InitialDirectory = InitialDirectory.Text;
            Load();
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var browser = new ExplorerFolderBrowser
                          {
                              SelectedPath = _appSettings.InitialDirectory
                          };
            browser.ShowDialog();
            _appSettings.InitialDirectory = browser.SelectedPath;
            Load();
        }


        private void AboutWindowClick(object sender, RoutedEventArgs e)
        {
            var assembly = typeof(MainWindow).Assembly;
            IAboutContent aboutWindowContent = new AboutContent(assembly, $@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\b.png");

            var aboutWindow = new AboutWindow
                              {
                                  DataContext = new AboutViewModel(aboutWindowContent, _roundCorners)
                              };

            aboutWindow.ShowDialog();
        }
    }
}