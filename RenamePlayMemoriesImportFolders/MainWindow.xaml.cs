using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.CoreExtended.AppHelpers;
using EvilBaschdi.CoreExtended.Browsers;
using EvilBaschdi.CoreExtended.Metro;
using EvilBaschdi.CoreExtended.Mvvm;
using EvilBaschdi.CoreExtended.Mvvm.View;
using EvilBaschdi.CoreExtended.Mvvm.ViewModel;
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
        private readonly IMoveDirectory _moveDirectory;
        private readonly IThemeManagerHelper _themeManagerHelper;
        private string _initialDirectory;

        /// <inheritdoc />
        public MainWindow()
        {
            InitializeComponent();
            IAppSettingsBase appSettingsBase = new AppSettingsBase(Settings.Default);
            IApplicationStyleSettings applicationStyleSettings = new ApplicationStyleSettings(appSettingsBase);
            _themeManagerHelper = new ThemeManagerHelper();
            IApplicationStyle applicationStyle = new ApplicationStyle(_themeManagerHelper);
            applicationStyle.Load(true);
            _appSettings = new AppSettings(appSettingsBase);
            _moveDirectory = new MoveDirectory();

            Load();
        }

        private void Load()
        {
            RenameFolders.IsEnabled = !string.IsNullOrWhiteSpace(_appSettings.InitialDirectory) && Directory.Exists(_appSettings.InitialDirectory);
            _initialDirectory = _appSettings.InitialDirectory;
            InitialDirectory.Text = _initialDirectory;
            RenameFolders.MouseRightButtonDown += RenameFoldersOnMouseRightButtonDown;
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
            var pmFolder = $@"{_initialDirectory}\";
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
            if (Directory.Exists(InitialDirectory.Text))
            {
                _appSettings.InitialDirectory = InitialDirectory.Text;
                Load();
            }
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var browser = new ExplorerFolderBrowser
                          {
                              SelectedPath = _initialDirectory
                          };
            browser.ShowDialog();
            _appSettings.InitialDirectory = browser.SelectedPath;
            Load();
        }


        private void AboutWindowClick(object sender, RoutedEventArgs e)
        {
            var assembly = typeof(MainWindow).Assembly;
            IAboutWindowContent aboutWindowContent = new AboutWindowContent(assembly, $@"{AppDomain.CurrentDomain.BaseDirectory}\Resources\b.png");

            var aboutWindow = new AboutWindow
                              {
                                  DataContext = new AboutViewModel(aboutWindowContent, _themeManagerHelper)
                              };

            aboutWindow.ShowDialog();
        }
    }
}