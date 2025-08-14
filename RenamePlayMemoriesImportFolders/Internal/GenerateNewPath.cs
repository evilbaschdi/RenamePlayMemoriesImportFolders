using JetBrains.Annotations;
using RenamePlayMemoriesImportFolders.Settings;
#if (!DEBUG)
using System.IO;
#endif

namespace RenamePlayMemoriesImportFolders.Internal;

/// <inheritdoc />
public class GenerateNewPath : IGenerateNewPath
{
    private readonly IInitialDirectoryFromSettings _initialDirectoryFromSettings;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="initialDirectoryFromSettings"></param>
    public GenerateNewPath([NotNull] IInitialDirectoryFromSettings initialDirectoryFromSettings)
    {
        _initialDirectoryFromSettings = initialDirectoryFromSettings ?? throw new ArgumentNullException(nameof(initialDirectoryFromSettings));
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string ValueFor([NotNull] string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var pmFolder = $@"{_initialDirectoryFromSettings.Value}\";
        var oldName = path.Replace(pmFolder, "").Replace(@"\", "");
        var values = oldName.Split('.');
        var year = values[2];
        var month = values[1];
        var day = values[0];
        var folderByYear = $"{pmFolder}{year}";
        var newName = $@"{folderByYear}\{year}-{month}-{day}";

#if (!DEBUG)
            Directory.CreateDirectory(folderByYear);
#endif

        return newName;
    }
}