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
// Created: Wednesday, March 6, 2013 11:23:31 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Content property changed event arguments.
    /// </summary>
    class ContentPropertyChangedEventArgs
        : EventArgs
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the name of the property that was updated.
        /// </summary>
        public string PropertyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the value for the property that was updated.
        /// </summary>
        public object Value
        {
            get;
            private set;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public ContentPropertyChangedEventArgs(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value = value;
        }
        #endregion
    }

    /// <summary>
	/// Base object for content that can be created/modified by the editor.
	/// </summary>
	public abstract class ContentObject
		: IDisposable, INamedObject
	{
		#region Variables.
		private ContentPanel _contentControl;		// Control used to edit/display the content.
		private string _name = "Content";			// Name of the content.
	    private bool _disposed;						// Flag to indicate that the object was disposed.
	    private bool _isOwned;						// Flag to indicate that this content is linked to another piece of content.
		#endregion

        #region Events.
        /// <summary>
        /// Event fired when the content has had a property change.
        /// </summary>
        [Browsable(false)]
        internal event EventHandler<ContentPropertyChangedEventArgs> ContentPropertyChanged;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type descriptor for this content.
        /// </summary>
        [Browsable(false)]
        internal ContentTypeDescriptor TypeDescriptor
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the plug-in that can create this content.
        /// </summary>
        [Browsable(false)]
        public ContentPlugIn PlugIn
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return whether the content has been initialized or not.
        /// </summary>
        [Browsable(false)]
        public bool IsInitialized
        {
            get
            {
                return _contentControl != null;
            }
        }

        /// <summary>
        /// Property to return whether this content has properties that can be manipulated in the properties tab.
        /// </summary>
        [Browsable(false)]
        public abstract bool HasProperties
        {
            get;
        }

		/// <summary>
		/// Property to return the type of content.
		/// </summary>
        [Browsable(false)]
		public abstract string ContentType
		{
			get;
		}

		/// <summary>
		/// Property to return the graphics interface for the application.
		/// </summary>
        [Browsable(false)]
		public GorgonGraphics Graphics
		{
			get
			{
				return Program.Graphics;
			}
		}

		/// <summary>
		/// Property to return whether the content object supports a renderer interface.
		/// </summary>
        [Browsable(false)]
		public abstract bool HasRenderer
		{
			get;
		}

		/// <summary>
		/// Property to return whether this content has a thumbnail or not.
		/// </summary>
		[Browsable(false)]
	    public bool HasThumbnail
	    {
		    get;
		    protected set;
	    }

		/// <summary>
		/// Property to return the name of the content object.
		/// </summary>
        [Browsable(true),
		LocalDisplayName(typeof(Resources), "PROP_NAME_NAME"),
		LocalCategory(typeof(Resources), "CATEGORY_DESIGN"), 
		LocalDescription(typeof(Resources), "PROP_NAME_DESC")]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					return;
				}

				if (string.Equals(value, _name, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				_name = ValidateName(value);

				if (string.IsNullOrWhiteSpace(_name))
				{
					return;
				}
				
				OnContentPropertyChanged("Name", _name);
			}
		}

		/// <summary>
		/// Property to return the list of dependencies for this content.
		/// </summary>
		[Browsable(false)]
	    public HashSet<string> Dependencies
	    {
		    get;
		    internal set;
	    }

		/// <summary>
		/// Property to return whether this content has an owner or not.
		/// </summary>
	    public bool HasOwner
	    {
			get
			{
				return _isOwned;
			}
			internal set
			{
				_isOwned = value;
				TypeDescriptor["Name"].IsReadOnly = true;
			}
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the registered image editor for the system.
		/// </summary>
		/// <returns>The registered image editor plug-in, or NULL (Nothing in VB.Net) if not found.</returns>
	    protected IImageEditorPlugIn GetRegisteredImageEditor()
		{
			// TODO: We need to make this a little more dynamic.
			// TODO: A couple of options are present.  We could make this a concrete class in the editor that's public. 
			// TODO: Or, we could allow the user to switch between editors in the interface.  This would make it so that only
			// TODO: one image editor plug-in can be active at any given time (this is ideal because it wouldn't make sense to have
			// TODO: multiple image editors available at once).
			return Gorgon.PlugIns.FirstOrDefault(item => string.Equals(item.Name,
			                                                           "GorgonLibrary.Editor.ImageEditorPlugIn.GorgonImageEditorPlugIn",
			                                                           StringComparison.OrdinalIgnoreCase)) as IImageEditorPlugIn;
		}

        /// <summary>
        /// Function called after the content data has been updated.
        /// </summary>
        protected virtual void OnContentUpdated()
        {
        }

		/// <summary>
		/// Function to persist the content data to a stream.
		/// </summary>
		/// <param name="stream">Stream that will receive the data.</param>
	    protected abstract void OnPersist(Stream stream);

		/// <summary>
		/// Function to read the content data from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the content data.</param>
	    protected abstract void OnRead(Stream stream);

		/// <summary>
		/// Function to load a dependency file.
		/// </summary>
		/// <param name="dependencyPath">Path to the dependency being loaded.</param>
		/// <param name="stream">Stream containing the dependency file.</param>
	    protected virtual void OnLoadDependencyFile(string dependencyPath, Stream stream)
	    {
	    }

        /// <summary>
        /// Function called when the name is about to be changed.
        /// </summary>
        /// <param name="proposedName">The proposed name for the content.</param>
        /// <returns>A valid name for the content.</returns>
        protected virtual string ValidateName(string proposedName)
        {
            return proposedName;
        }

        /// <summary>
        /// Function to notify the application that a property on the content object was changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that was updated.</param>
        /// <param name="value">Value assigned to the property.</param>
        protected virtual void OnContentPropertyChanged(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "propertyName");
            }

            if (ContentPropertyChanged != null)
            {
                ContentPropertyChanged(this, new ContentPropertyChangedEventArgs(propertyName, value));
            }
        }

        /// <summary>
        /// Function called when the content is being initialized.
        /// </summary>
        /// <returns>A control to place in the primary interface window.</returns>
        protected abstract ContentPanel OnInitialize();

        /// <summary>
        /// Function to retrieve default values for properties with the DefaultValue attribute.
        /// </summary>
        internal void SetDefaults()
        {
            foreach (var descriptor in TypeDescriptor.Where(descriptor => descriptor.HasDefaultValue))
            {
                descriptor.DefaultValue = descriptor.GetValue<object>();
            }
        }

		/// <summary>
		/// Function to read a dependency file from a stream.
		/// </summary>
		/// <param name="dependencyPath">Path to the dependency being loaded.</param>
		/// <param name="stream">Stream containing the file to read.</param>
	    internal void LoadDependencyFile(string dependencyPath, Stream stream)
	    {
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanRead)
			{
				throw new IOException(Resources.GOREDIT_STREAM_WRITE_ONLY);
			}

			if (stream.Position >= stream.Length)
			{
				throw new EndOfStreamException(Resources.GOREDIT_STREAM_EOS);
			}

			OnLoadDependencyFile(dependencyPath, stream);
		}

		/// <summary>
		/// Function to read the content from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the content data.</param>
	    internal void Read(Stream stream)
	    {
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanRead)
			{
				throw new IOException(Resources.GOREDIT_STREAM_WRITE_ONLY);
			}

			if (stream.Position >= stream.Length)
			{
				throw new EndOfStreamException(Resources.GOREDIT_STREAM_EOS);
			}

			OnRead(stream);

            // Update the properties.
            SetDefaults();

            // Update the panel
			if (_contentControl != null)
			{
				_contentControl.RefreshContent();
			}
	    }

		/// <summary>
		/// Function to persist the content data into a stream.
		/// </summary>
		/// <param name="stream">Stream that will receive the data for the content.</param>
		internal void Persist(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new IOException(Resources.GOREDIT_STREAM_READ_ONLY);
			}

			OnPersist(stream);

			if (_contentControl != null)
			{
				_contentControl.ContentPersisted();
			}
		}

		/// <summary>
		/// Function to retrieve a thumbnail image for the content plug-in.
		/// </summary>
		/// <returns>The image for the thumbnail of the content.</returns>
		/// <remarks>The size of the thumbnail should be set to 128x128.</remarks>
		public virtual System.Drawing.Image GetThumbNailImage()
		{
			return null;
		}

        /// <summary>
        /// Function called when a property is changed from the property grid.
        /// </summary>
        /// <param name="e">Event parameters called when a property is changed.</param>
        public virtual void PropertyChanged(PropertyValueChangedEventArgs e)
        {
            
        }

		/// <summary>
		/// Function to draw the interface for the content editor.
		/// </summary>
		public virtual void Draw()
		{
		}

		/// <summary>
		/// Function to initialize the content editor.
		/// </summary>
		/// <returns>A control to place in the primary interface window.</returns>
        public ContentPanel InitializeContent()
		{
			_contentControl = OnInitialize();
			return _contentControl;
		}

		/// <summary>
		/// Function to close the content object.
		/// </summary>
		/// <remarks>Ensure that any changes to the content are persisted before calling this method, otherwise those changes will be lost.</remarks>
	    public void CloseContent()
	    {
			if (ContentManagement.Current == this)
			{
				ContentManagement.LoadDefaultContentPane();
			}
	    }
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentObject"/> class.
		/// </summary>
		/// <param name="name">Name of the content.</param>
		protected ContentObject(string name)
		{
			Dependencies = new HashSet<string>(new EditorMetaDataFile.NameComparer());
			TypeDescriptor = new ContentTypeDescriptor(this);
            TypeDescriptor.Enumerate(GetType());

			Name = name;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string INamedObject.Name
		{
			get 
			{
				return Name;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
            _contentControl = null;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>Calling this method will -not- call the Close method.</remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
