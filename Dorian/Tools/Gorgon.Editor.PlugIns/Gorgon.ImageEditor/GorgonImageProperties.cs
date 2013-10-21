﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, October 21, 2013 8:37:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using GorgonLibrary.Configuration;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Properties for the image editor plugin.
    /// </summary>
    class GorgonImageProperties
        : EditorPlugInSettings
    {
        #region Methods.
        /// <summary>
        /// Property to return the paths to assemblies that hold custom image codecs.
        /// </summary>
        [ApplicationSetting("CustomCodecs", typeof(IList<string>), "Codecs")]
        public IList<string> CustomCodecs
        {
            get;
            private set;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageProperties"/> class.
        /// </summary>
        public GorgonImageProperties()
            : base("ImageEditor.PlugIn", new Version(1, 0))
        {
            CustomCodecs = new List<string>();
        }
        #endregion
    }
}
