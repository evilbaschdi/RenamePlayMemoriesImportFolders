using System.Windows;
using EvilBaschdi.CoreExtended.Metro;

namespace RenamePlayMemoriesImportFolders
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            var themeManagerHelper = new ThemeManagerHelper();
            themeManagerHelper.RegisterSystemColorTheme();

            base.OnStartup(e);
        }
    }
}