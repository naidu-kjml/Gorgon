﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 30, 2018 12:48:54 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gorgon.Collections;
using Gorgon.Diagnostics;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.IO;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// Handles importing of metadata from v2 of Gorgon's file structure.
    /// </summary>
    internal class V2MetadataImporter
    {
        #region Constants.
        // The name of the root node in the metadata.
        private const string RootNodeName = "Gorgon.Editor.MetaData";
        // The name of the node containing the writer plugin name.
        private const string WriterNodeName = "WriterPlugIn";
        // The name of the attribute that stores the writer plugin type name.
        private const string WriterTypeNameAttr = "TypeName";
        // The name of the file node in the metadata.
        private const string FileNodeName = "File";
        // The name of the file path attribute.
        private const string FilePathAttr = "FilePath";

        /// <summary>
        /// The name of the v2 metadata file.
        /// </summary>
        public const string V2MetadataFilename = ".gorgon.editor.metadata";
        #endregion

        #region Variables.
        // The file containing the metadata.
        private readonly FileInfo _file;
        // The file system providers.
        private readonly IFileSystemProviders _providers;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to import the files in the metadata.
        /// </summary>
        /// <param name="files">The files to populate.</param>
        /// <param name="rootNode">The root node of the metadata.</param>
        private void GetFiles(IDictionary<string, ProjectItemMetadata> files, XElement rootNode)
        {
            Program.Log.Print("Importing file list.", LoggingLevel.Verbose);

            IEnumerable<XElement> fileNodes = rootNode.Descendants(FileNodeName);

            foreach (XElement fileNode in fileNodes)
            {
                string filePath = fileNode.Attribute(FilePathAttr)?.Value;                

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    continue;
                }

                string dirPath = Path.GetDirectoryName(filePath).FormatDirectory('/');

                // Add directories.
                if ((!string.IsNullOrWhiteSpace(dirPath)) && (!files.ContainsKey(dirPath)))
                {
                    files.Add(dirPath, new ProjectItemMetadata());
                }

                files.Add(filePath, new ProjectItemMetadata()
                {
                    PluginName = null
                });

                // TODO: We need to import attributes and dependencies.  
            }
        }

        /// <summary>
        /// Function to retrieve the writer plug in metadata.
        /// </summary>
        /// <param name="rootNode">The root node of the metadata.</param>
        /// <returns>The file writer plugin.</returns>
        private FileWriterPlugin GetWriterPlugin(XElement rootNode)
        {
            string writerTypeName = rootNode.Element(WriterNodeName)?.Attribute(WriterTypeNameAttr)?.Value;

            if (string.IsNullOrWhiteSpace(writerTypeName))
            {
                Program.Log.Print("No writer plugin associated with this file.", LoggingLevel.Verbose);
                return null;
            }

            FileWriterPlugin plugin = _providers.GetWriterByName(writerTypeName, true);

            if (plugin == null)
            {                
                Program.Log.Print($"Writer plugin '{writerTypeName}' is associated with this file, but no provider plugin is loaded with that name.", LoggingLevel.Verbose);
                return null;
            }

            Program.Log.Print($"Found writer plugin '{writerTypeName}'.", LoggingLevel.Verbose);
            return plugin;
        }

        /// <summary>
        /// Function to perform the import of the metadata.
        /// </summary>
        /// <param name="project">The project to update.</param>
        public void Import(IProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            project.AssignWriter(null);

            if (!_file.Exists)
            {
                return;
            }

            Program.Log.Print("Importing v2 Gorgon Editor file metadata...", LoggingLevel.Simple);

            var document = XDocument.Load(_file.FullName);

            XElement rootNode = document.Element(RootNodeName);

            if (rootNode == null)
            {
                Program.Log.Print("No root node found.  Not a v2 Gorgon Editor metadata file.", LoggingLevel.Verbose);
                return;
            }

            project.AssignWriter(GetWriterPlugin(rootNode));
                        
            GetFiles(project.ProjectItems, rootNode);

            try
            {
                // Delete the metadata file, we don't need it anymore.
                _file.Delete();
            }
            catch
            {
                // Do nothing if we can't delete the metadata file.
            }
            Program.Log.Print("Imported v2 Gorgon Editor metadata.", LoggingLevel.Simple);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the V2MetadataImporter class.</summary>
        /// <param name="metadataFile">The file containing the v2 metadata.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadataFile"/>, or the <paramref name="fileSystemProviders"/> parameter is <b>null</b>.</exception>
        public V2MetadataImporter(FileInfo metadataFile, IFileSystemProviders fileSystemProviders)
        {
            _file = metadataFile ?? throw new ArgumentNullException(nameof(metadataFile));
            _providers = fileSystemProviders ?? throw new ArgumentNullException(nameof(metadataFile));
        }
        #endregion
    }
}