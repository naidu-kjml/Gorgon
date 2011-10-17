﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Wednesday, October 12, 2011 6:38:14 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A swap chain used to display graphics to a window.
	/// </summary>
	public class GorgonSwapChain
		: GorgonNamedObject, IDisposable
	{
		#region Classes.
		/// <summary>
		/// Records the form state.
		/// </summary>
		private class FormStateRecord
		{
			#region Variables.
			private Point _location;					// Window location.
			private Size _size;							// Window size.
			private FormBorderStyle _border;			// Window border.
			private bool _topMost;						// Topmost flag.
			private bool _sysMenu;						// System menu flag.
			private bool _maximizeButton;				// Maxmimize button flag.
			private bool _minimizeButton;				// Minimize button flag.
			private bool _visible;						// Visible flag.
			private bool _enabled;						// Enabled flag.
			private Form _window;						// Window.
			#endregion

			#region Methods.
			/// <summary>
			/// Function to restore the original window state.
			/// </summary>
			/// <param name="keepSize">TRUE to keep the size of the window, FALSE to restore it.</param>
			/// <param name="dontMove">TRUE to keep the window from moving, FALSE to restore the last location.</param>
			public void Restore(bool keepSize, bool dontMove)
			{
				if (_window == null)
					return;

				if (!dontMove)
					_window.Location = _location;
				if (!keepSize)
					_window.Size = _size;
				_window.FormBorderStyle = _border;
				_window.TopMost = _topMost;
				_window.ControlBox = _sysMenu;
				_window.MaximizeBox = _maximizeButton;
				_window.MinimizeBox = _minimizeButton;
				_window.Enabled = _enabled;
				_window.Visible = _visible;
			}

			/// <summary>
			/// Function to update the form state.
			/// </summary>
			public void Update()
			{
				_location = _window.Location;
				_size = _window.Size;
				_border = _window.FormBorderStyle;
				_topMost = _window.TopMost;
				_sysMenu = _window.ControlBox;
				_maximizeButton = _window.MaximizeBox;
				_minimizeButton = _window.MinimizeBox;
				_enabled = _window.Enabled;
				_visible = _window.Visible;
			}
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="FormStateRecord"/> struct.
			/// </summary>
			/// <param name="window">The window.</param>
			public FormStateRecord(Form window)
			{
				_location = window.Location;
				_size = window.Size;
				_border = window.FormBorderStyle;
				_topMost = window.TopMost;
				_sysMenu = window.ControlBox;
				_maximizeButton = window.MaximizeBox;
				_minimizeButton = window.MinimizeBox;
				_enabled = window.Enabled;
				_visible = window.Visible;
				_window = window;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private Form _parentForm = null;						// Parent form for our window.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private bool _wasWindowed = true;						// Flag to indicate that the window was in windowed mode because of a transition.
		private FormStateRecord _formState = null;				// Form state.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the DXGI swap chain interface.
		/// </summary>
		internal GI.SwapChain GISwapChain
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the D3D render target interface.
		/// </summary>
		internal D3D.RenderTargetView D3DRenderTarget
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the D3D view data.
		/// </summary>
		internal D3D.Viewport D3DView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the format of the swap chain.
		/// </summary>
		public GorgonBufferFormat Format
		{
			get
			{
				if (Settings == null)
					return GorgonBufferFormat.Unknown;

				return Settings.VideoMode.Value.Format;
			}			
		}

		/// <summary>
		/// Property to return the width of the swap chain.
		/// </summary>
		public int Width
		{
			get
			{
				if (Settings == null)
					return 0;

				return Settings.VideoMode.Value.Width;
			}
		}

		/// <summary>
		/// Property to return the height of the swap chain.
		/// </summary>
		public int Height
		{
			get
			{
				if (Settings == null)
					return 0;

				return Settings.VideoMode.Value.Height;
			}
		}

		/// <summary>
		/// Property to return the video output that the swap chain is operating on.
		/// </summary>
		public GorgonVideoOutput VideoOutput
		{
			get
			{
				if (GISwapChain == null)
					return null;

				GI.Output d3doutput = GISwapChain.ContainingOutput;

				var videoOutput = (from output in Settings.VideoDevice.Outputs
								   where output.Handle == d3doutput.Description.MonitorHandle
								   select output).SingleOrDefault();

				return videoOutput;
			}
		}

		/// <summary>
		/// Property to return the settings for this swap chain.
		/// </summary>
		public GorgonSwapChainSettings Settings
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to release any resources bound to the swap chain.
		/// </summary>
		private void ReleaseResources()
		{
			if (D3DRenderTarget != null)
			{
				Gorgon.Log.Print("GorgonSwapChain '{0}': Releasing D3D11 render target view...", Diagnostics.GorgonLoggingLevel.Intermediate, Name);
				D3DRenderTarget.Dispose();
			}
			D3DRenderTarget = null;
		}

		/// <summary>
		/// Function to create any resources bound to the swap chain.
		/// </summary>
		private void CreateResources()
		{			
			D3D.Texture2D texture = null;

			if (D3DRenderTarget != null)
				ReleaseResources();

			Gorgon.Log.Print("GorgonSwapChain '{0}': Creating D3D11 render target view...", Diagnostics.GorgonLoggingLevel.Intermediate, Name);

			try
			{
				texture = D3D.Texture2D.FromSwapChain<D3D.Texture2D>(GISwapChain, 0);
				D3DRenderTarget = new D3D.RenderTargetView(Settings.VideoDevice.D3DDevice, texture);
				D3DView = new D3D.Viewport(0, 0, Width, Height);
			}
			finally
			{
				if (texture != null)
					texture.Dispose();
			}
		}

		/// <summary>
		/// Handles the Deactivate event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _parentForm_Deactivate(object sender, EventArgs e)
		{
			if (Settings.IsWindowed)
			    return;

			_wasWindowed = false;
			UpdateSettings(true);
			_parentForm.WindowState = FormWindowState.Minimized;
		}

		/// <summary>
		/// Handles the Activated event of the _parentForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _parentForm_Activated(object sender, EventArgs e)
		{
			if (!_wasWindowed)
			{
				_wasWindowed = true;
				UpdateSettings(false);
			}
		}

		/// <summary>
		/// Handles the Resize event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Window_Resize(object sender, EventArgs e)
		{
			// Only do this if the size has changed, if we're just restoring the window, then don't bother.
			if ((_parentForm.WindowState != FormWindowState.Minimized) && (GISwapChain != null))
			{
				ReleaseResources();
				GI.SwapChainFlags flags = GI.SwapChainFlags.AllowModeSwitch;

				if ((!Settings.IsWindowed) && (!Settings.AllowRotation))
					flags |= GI.SwapChainFlags.NonPrerotated;

				Settings.VideoMode = new GorgonVideoMode(Settings.Window.ClientSize.Width, Settings.Window.ClientSize.Height, Settings.VideoMode.Value.Format, Settings.VideoMode.Value.RefreshRateNumerator, Settings.VideoMode.Value.RefreshRateDenominator);
				GISwapChain.ResizeBuffers(Settings.BufferCount, Settings.VideoMode.Value.Width, Settings.VideoMode.Value.Height, (GI.Format)Settings.VideoMode.Value.Format, flags);

				CreateResources();
			}
		}
		
		/// <summary>
		/// Function to intialize the swap chain.
		/// </summary>
		internal void Initialize()
		{
			GI.SwapChainFlags flags = GI.SwapChainFlags.AllowModeSwitch;
			GI.SwapChainDescription d3dSettings = new GI.SwapChainDescription();

			d3dSettings.BufferCount = Settings.BufferCount;
			d3dSettings.Flags = flags;

			d3dSettings.IsWindowed = true;
			d3dSettings.ModeDescription = GorgonVideoMode.Convert(Settings.VideoMode.Value);
			d3dSettings.OutputHandle = Settings.Window.Handle;
			d3dSettings.SampleDescription = GorgonMultiSampling.Convert(Settings.MultiSamples);
			d3dSettings.SwapEffect = GorgonSwapChainSettings.Convert(Settings.SwapEffect);

			if ((Settings.Flags & SwapChainUsageFlags.RenderTarget) == SwapChainUsageFlags.RenderTarget)
				d3dSettings.Usage = GI.Usage.RenderTargetOutput;

			if ((Settings.Flags & SwapChainUsageFlags.ShaderInput) == SwapChainUsageFlags.ShaderInput)
				d3dSettings.Usage |= GI.Usage.ShaderInput;

			Gorgon.Log.Print("GorgonSwapChain '{0}': Creating D3D11 swap chain...", Diagnostics.GorgonLoggingLevel.Simple, Name);
			GISwapChain = new GI.SwapChain(Graphics.GIFactory, Settings.VideoDevice.D3DDevice, d3dSettings);

			// Due to a bug with winforms and DXGI, we have to manually handle transitions ourselves.
			Graphics.GIFactory.SetWindowAssociation(Settings.Window.Handle, GI.WindowAssociationFlags.IgnoreAll);

			// We need to handle focus loss ourselves because of the aforementioned bug.
			_parentForm.Activated += new EventHandler(_parentForm_Activated);
			_parentForm.Deactivate += new EventHandler(_parentForm_Deactivate);

			if ((!Settings.IsWindowed) && (!Settings.AllowRotation))
				flags |= GI.SwapChainFlags.NonPrerotated;

			if (!Settings.IsWindowed)
				ModeStateUpdate(false, d3dSettings.ModeDescription, d3dSettings.BufferCount, flags, true);
			CreateResources();

			if (Settings.IsWindowed)
				Settings.Window.Resize += new EventHandler(Window_Resize);
		}

		/// <summary>
		/// Function to update the fullscreen/windowed mode state.
		/// </summary>
		/// <param name="windowed">TRUE for windowed mode, FALSE for fullscreen.</param>
		/// <param name="mode">Video mode to use.</param>
		/// <param name="bufferCount">Number of back buffers.</param>
		/// <param name="flags">Flags for the swap chain.</param>
		/// <param name="overrideState">TRUE to force the mode switch, FALSE to let the swap chain determine if it needs to switch modes.</param>
		private void ModeStateUpdate(bool windowed, GI.ModeDescription mode, int bufferCount, GI.SwapChainFlags flags, bool overrideState)
		{
			GISwapChain.ResizeTarget(mode);

			if ((overrideState) || (windowed != Settings.IsWindowed))
			{
				if (!windowed)
				    GISwapChain.SetFullScreenState(true, VideoOutput.GIOutput);
				else
				    GISwapChain.SetFullScreenState(false, null);
			}

			GISwapChain.ResizeBuffers(bufferCount, mode.Width, mode.Height, mode.Format, flags);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode)
		{
			UpdateSettings(mode, Settings.IsWindowed, Settings.BufferCount, Settings.AllowRotation);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="isWindowed">TRUE to use windowed mode, FALSE to use full screen mode.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(bool isWindowed)
		{
			UpdateSettings(Settings.VideoMode.Value, isWindowed, Settings.BufferCount, Settings.AllowRotation);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <param name="isWindowed">TRUE to use windowed mode, FALSE to use full screen mode.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode, bool isWindowed)
		{
			UpdateSettings(Settings.VideoMode.Value, isWindowed, Settings.BufferCount, Settings.AllowRotation);
		}

		/// <summary>
		/// Function to update the settings for the swap chain.
		/// </summary>
		/// <param name="mode">New video mode to use.</param>
		/// <param name="isWindowed">TRUE to switch to windowed mode, FALSE to switch to full screen.</param>
		/// <param name="bufferCount">Number of back buffers.</param>
		/// <param name="allowRotation">TRUE to auto-rotate data before display, FALSE to leave alone.</param>
		/// <remarks>If the <paramref name="allowRotation"/> parameter is set to TRUE, then a slight performance penalty will be introduced.
		/// <para>If the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.SwapEffect">SwapEffect</see> for the swap chain is set to discard, then the <paramref name="bufferCount"/> must be greater than 1.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		public void UpdateSettings(GorgonVideoMode mode, bool isWindowed, int bufferCount, bool allowRotation)
		{
			GI.SwapChainFlags flags = GI.SwapChainFlags.AllowModeSwitch;
			GorgonSwapChainSettings settings = new GorgonSwapChainSettings();

			if (GISwapChain == null)
				return;

			Settings.Window.Resize -= new EventHandler(Window_Resize);

			try
			{
				// Use these previous settings.
				settings.Window = Settings.Window;
				settings.VideoDevice = Settings.VideoDevice;
				settings.Flags = Settings.Flags;
				settings.MultiSamples = Settings.MultiSamples;
				settings.SwapEffect = Settings.SwapEffect;
				settings.IsWindowed = isWindowed;
				settings.AllowRotation = allowRotation;
				settings.VideoMode = mode;
				settings.BufferCount = bufferCount;

				// If we're going to full screen mode, remember the current form state.
				if ((!settings.IsWindowed) && (Settings.IsWindowed) && (_parentForm.WindowState != FormWindowState.Minimized))
					_formState.Update();

				// Validate and modify the settings as appropriate.
				Graphics.ValidateSwapChainSettings(settings, this);

				// Modify the buffers.
				if ((!settings.IsWindowed) && (!settings.AllowRotation))
					flags |= GI.SwapChainFlags.NonPrerotated;

				ReleaseResources();
				ModeStateUpdate(settings.IsWindowed, GorgonVideoMode.Convert(settings.VideoMode.Value), settings.BufferCount, flags, false);

				// If we're coming back from full screen mode, then restore the form state.
				if ((settings.IsWindowed) && (!Settings.IsWindowed))
					_formState.Restore(false, false);

				// For some reason DXGI is giving the incorrect size information.
				_parentForm.Location = VideoOutput.OutputBounds.Location;
				_parentForm.ClientSize = new Size(Settings.VideoMode.Value.Width, Settings.VideoMode.Value.Height);

				Settings = settings;
				CreateResources();
			}
			finally
			{
				if (Settings.IsWindowed)
					Settings.Window.Resize += new EventHandler(Window_Resize);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSwapChain"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this swap chain.</param>
		/// <param name="name">The name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		internal GorgonSwapChain(GorgonGraphics graphics, string name, GorgonSwapChainSettings settings)
			: base(name)
		{
			if (graphics == null)
				throw new ArgumentNullException("graphics");

			Settings = settings;
			Graphics = graphics;			

			// Get the parent form for our window.
			_parentForm = Gorgon.GetTopLevelForm(settings.Window);
			_formState = new FormStateRecord(_parentForm);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Settings.Window.Resize -= new EventHandler(Window_Resize);
					_parentForm.Activated -= new EventHandler(_parentForm_Activated);
					_parentForm.Deactivate -= new EventHandler(_parentForm_Deactivate);

					ReleaseResources();

					Gorgon.Log.Print("GorgonSwapChain '{0}': Removing D3D11 swap chain...", Diagnostics.GorgonLoggingLevel.Simple, Name);
					if (GISwapChain != null)
					{
						if (GISwapChain.IsFullScreen)
							GISwapChain.SetFullScreenState(false, null);

						GISwapChain.Dispose();
					}
					if (Graphics != null)
						Graphics.RemoveTrackedObject(this);					
				}

				GISwapChain = null;
				Graphics = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
