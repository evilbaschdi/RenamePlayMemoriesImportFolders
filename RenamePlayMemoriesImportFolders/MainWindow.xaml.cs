using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using EvilBaschdi.About.Core;
using EvilBaschdi.About.Core.Models;
using EvilBaschdi.About.Wpf;
using EvilBaschdi.Core;
using EvilBaschdi.Core.AppHelpers;
using EvilBaschdi.Core.Internal;
using EvilBaschdi.CoreExtended;
using EvilBaschdi.CoreExtended.Browsers;
using EvilBaschdi.Settings.ByMachineAndUser;
using MahApps.Metro.Controls;
using RenamePlayMemoriesImportFolders.Core;
using RenamePlayMemoriesImportFolders.Internal;

namespace RenamePlayMemoriesImportFolders;

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

    /// <inheritdoc />
    public MainWindow()
    {
        InitializeComponent();
        IAppSettingsFromJsonFile appSettingsFromJsonFile = new AppSettingsFromJsonFile();
        IAppSettingsFromJsonFileByMachineAndUser appSettingsFromJsonFileByMachineAndUser = new AppSettingsFromJsonFileByMachineAndUser();
        IAppSettingByKey appSettingByKey = new AppSettingByKey(appSettingsFromJsonFile, appSettingsFromJsonFileByMachineAndUser);
        IApplicationStyle applicationStyle = new ApplicationStyle(true);
        applicationStyle.Run();

        _appSettings = new AppSettings(appSettingByKey);
        _moveDirectory = new MoveDirectory();
        _processByPath = new ProcessByPath();
        _generateNewPath = new GenerateNewPath(_appSettings);
        Load();
    }

    private void Load()
    {
        RenameFolders.SetCurrentValue(IsEnabledProperty, !string.IsNullOrWhiteSpace(_appSettings.InitialDirectory) && Directory.Exists(_appSettings.InitialDirectory));
        InitialDirectory.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, _appSettings.InitialDirectory ?? string.Empty);
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
        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressStateProperty, TaskbarItemProgressState.Indeterminate);
        SetCurrentValue(CursorProperty, Cursors.Wait);

        var task = Task<string>.Factory.StartNew(Rename);
        await task;

        RenameFoldersContent.SetCurrentValue(System.Windows.Controls.TextBlock.TextProperty, task.Result);

        TaskbarItemInfo.SetCurrentValue(TaskbarItemInfo.ProgressStateProperty, TaskbarItemProgressState.Normal);
        SetCurrentValue(CursorProperty, Cursors.Arrow);
    }

    private string Rename()
    {
        var pmFolder = $@"{_appSettings.InitialDirectory}\";
        var paths = Directory.GetDirectories(pmFolder);
        var counter = 0;

        foreach (var path in paths.Where(item => item.Split('\\').Last().Contains('.')))
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
        ICurrentAssembly currentAssembly = new CurrentAssembly();
        IAboutContent aboutContent = new AboutContent(currentAssembly);
        IAboutModel aboutModel = new AboutViewModel(aboutContent);
        var aboutWindow = new AboutWindow(aboutModel);

        aboutWindow.ShowDialog();
    }
}