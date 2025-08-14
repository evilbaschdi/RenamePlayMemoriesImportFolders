using System.Windows;
#if (!DEBUG)
using ControlzEx.Theming;

#endif

namespace RenamePlayMemoriesImportFolders;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public partial class App : Application
{
    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
#if (!DEBUG)
            ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);
#endif

        base.OnStartup(e);
    }
}