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
using EvilBaschdi.Core.Settings.ByMachineAndUser;
using EvilBaschdi.Core.Wpf;
using EvilBaschdi.Core.Wpf.Browsers;
using MahApps.Metro.Controls;
using RenamePlayMemoriesImportFolders.Internal;
using RenamePlayMemoriesImportFolders.Settings;

namespace RenamePlayMemoriesImportFolders;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public partial class MainWindow : MetroWindow
{
    private readonly IInitialDirectoryFromSettings _initialDirectoryFromSettings;
    private readonly IGenerateNewPath _generateNewPath;
    private readonly IMoveDirectory _moveDirectory;
    private readonly IProcessByPath _processByPath;
    private readonly string _initialDirectory;

    /// <inheritdoc />
    public MainWindow()
    {
        InitializeComponent();
        IAppSettingsFromJsonFile appSettingsFromJsonFile = new AppSettingsFromJsonFile();
        IAppSettingsFromJsonFileByMachineAndUser appSettingsFromJsonFileByMachineAndUser = new AppSettingsFromJsonFileByMachineAndUser();
        IAppSettingByKey appSettingByKey = new AppSettingByKey(appSettingsFromJsonFile, appSettingsFromJsonFileByMachineAndUser);
        IApplicationStyle applicationStyle = new ApplicationStyle();
        IApplicationLayout applicationLayout = new ApplicationLayout();
        applicationStyle.Run();
        applicationLayout.RunFor((true, false));

        _initialDirectoryFromSettings = new InitialDirectoryFromSettings(appSettingByKey);
        _moveDirectory = new MoveDirectory();
        _processByPath = new ProcessByPath();
        _generateNewPath = new GenerateNewPath(_initialDirectoryFromSettings);

        _initialDirectory = _initialDirectoryFromSettings.Value;
        Load();
    }

    private void Load()
    {
        RenameFolders.SetCurrentValue(IsEnabledProperty, !string.IsNullOrWhiteSpace(_initialDirectory) && Directory.Exists(_initialDirectory));
        InitialDirectory.SetCurrentValue(System.Windows.Controls.TextBox.TextProperty, _initialDirectory ?? string.Empty);
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
            _processByPath.RunFor(_initialDirectory);
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
        var pmFolder = $@"{_initialDirectory}\";
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

        _initialDirectoryFromSettings.Value = InitialDirectory.Text;
        Load();
    }

    private void BrowseClick(object sender, RoutedEventArgs e)
    {
        var browser = new ExplorerFolderBrowser
                      {
                          SelectedPath = _initialDirectory
                      };
        browser.ShowDialog();
        _initialDirectoryFromSettings.Value = browser.SelectedPath;
        Load();
    }

    private void AboutWindowClick(object sender, RoutedEventArgs e)
    {
        ICurrentAssembly currentAssembly = new CurrentAssembly();
        IAboutContent aboutContent = new AboutContent(currentAssembly);
        IAboutViewModel aboutModel = new AboutViewModel(aboutContent);
        IApplyMicaBrush applyMicaBrush = new ApplyMicaBrush();
        IApplicationLayout applicationLayout = new ApplicationLayout();
        var aboutWindow = new AboutWindow(aboutModel, applicationLayout, applyMicaBrush);

        aboutWindow.ShowDialog();
    }
}