﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NuGet.Versioning;

namespace NuGetGallery
{
    public class PackageFileService : CorePackageFileService, IPackageFileService
    {
        /// <summary>
        /// Active readme markdown file, formatted as 'active/{packageId}/{version}.md'
        /// </summary>
        private const string ReadMeFilePathTemplateActive = "active/{0}/{1}{2}";

        private readonly IFileStorageService _fileStorageService;

        public PackageFileService(IFileStorageService fileStorageService)
            : base(fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public Task<ActionResult> CreateDownloadPackageActionResultAsync(Uri requestUrl, Package package)
        {
            var fileName = BuildFileName(package, CoreConstants.PackageFileSavePathTemplate, CoreConstants.NuGetPackageFileExtension);
            return _fileStorageService.CreateDownloadFileActionResultAsync(requestUrl, CoreConstants.PackagesFolderName, fileName);
        }

        public Task<ActionResult> CreateDownloadPackageActionResultAsync(Uri requestUrl, string id, string version)
        {
            var fileName = BuildFileName(id, version, CoreConstants.PackageFileSavePathTemplate, CoreConstants.NuGetPackageFileExtension);
            return _fileStorageService.CreateDownloadFileActionResultAsync(requestUrl, CoreConstants.PackagesFolderName, fileName);
        }

        /// <summary>
        /// Deletes the package readme.md file from storage.
        /// </summary>
        /// <param name="package">The package associated with the readme.</param>
        public Task DeleteReadMeMdFileAsync(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }
            
            var fileName = BuildFileName(package, ReadMeFilePathTemplateActive, Constants.MarkdownFileExtension);

            return _fileStorageService.DeleteFileAsync(CoreConstants.PackageReadMesFolderName, fileName);
        }

        /// <summary>
        /// Saves the package readme.md file to storage.
        /// </summary>
        /// <param name="package">The package associated with the readme.</param>
        /// <param name="readMeMd">Markdown content.</param>
        public async Task SaveReadMeMdFileAsync(Package package, string readMeMd)
        {
            if (string.IsNullOrWhiteSpace(readMeMd))
            {
                throw new ArgumentNullException(nameof(readMeMd));
            }

            var fileName = BuildFileName(package, ReadMeFilePathTemplateActive, Constants.MarkdownFileExtension);

            using (var readMeMdStream = new MemoryStream(Encoding.UTF8.GetBytes(readMeMd)))
            {
                await _fileStorageService.SaveFileAsync(CoreConstants.PackageReadMesFolderName, fileName, readMeMdStream, overwrite: true);
            }
        }

        /// <summary>
        /// Downloads the readme.md from storage.
        /// </summary>
        /// <param name="package">The package associated with the readme.</param>
        public async Task<string> DownloadReadMeMdFileAsync(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }
            
            var fileName = BuildFileName(package, ReadMeFilePathTemplateActive, Constants.MarkdownFileExtension);

            using (var readMeMdStream = await _fileStorageService.GetFileAsync(CoreConstants.PackageReadMesFolderName, fileName))
            {
                // Note that fileStorageService implementations return null if not found.
                if (readMeMdStream != null)
                {
                    using (var readMeMdReader = new StreamReader(readMeMdStream))
                    {
                        return await readMeMdReader.ReadToEndAsync();
                    }
                }
            }

            return null;
        }
    }
}