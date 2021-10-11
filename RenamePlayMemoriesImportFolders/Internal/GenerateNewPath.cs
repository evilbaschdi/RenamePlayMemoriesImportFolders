using System;
using JetBrains.Annotations;
using RenamePlayMemoriesImportFolders.Core;
#if (!DEBUG)
using System.IO;
#endif

namespace RenamePlayMemoriesImportFolders.Internal
{
    /// <inheritdoc />
    public class GenerateNewPath : IGenerateNewPath
    {
        private readonly IAppSettings _appSettings;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="appSettings"></param>
        public GenerateNewPath([NotNull] IAppSettings appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ValueFor([NotNull] string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var pmFolder = $@"{_appSettings.InitialDirectory}\";
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
}