﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, January 09, 2012 7:06:42 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D11 = SharpDX.Direct3D11;
using GorgonLibrary.Native;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Buffer usage types.
	/// </summary>
	public enum BufferUsage
	{
		/// <summary>
		/// Allows read/write access to the buffer from the GPU.
		/// </summary>
		Default = 0,
		/// <summary>
		/// Allows read access by the GPU and write access by the CPU.
		/// </summary>
		Dynamic = 1,
		/// <summary>
		/// Can only be read by the GPU, cannot be written to or read from by the CPU, and cannot be written to by the GPU.
		/// </summary>
		/// <remarks>Pre-initialize any buffer created with this usage, or else you will not be able to after it's been created.</remarks>
		Immutable = 2,
		/// <summary>
		/// Allows reading/writing by the CPU and can be copied to a GPU compatiable buffer (but not used directly by the GPU).
		/// </summary>
		Staging = 3
	}

	/// <summary>
	/// Flags used when locking the buffer for reading/writing.
	/// </summary>
	[Flags()]
	public enum BufferLockFlags
	{
		/// <summary>
		/// Lock the buffer for reading.
		/// </summary>
		/// <remarks>This flag is mutually exclusive.</remarks>
		Read = 0,
		/// <summary>
		/// Lock the buffer for writing.
		/// </summary>
		Write = 1,
		/// <summary>
		/// Lock the buffer for writing, but guarantee that we will not overwrite a part of the buffer that's already in use.
		/// </summary>
		NoOverwrite = 2,
		/// <summary>
		/// Lock the buffer for writing, but mark its contents as invalid.
		/// </summary>
		Discard = 4
	}

	/// <summary>
	/// A base buffer object.
	/// </summary>
	public abstract class GorgonBaseBuffer
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D CPU access flags.
		/// </summary>
		internal D3D11.CpuAccessFlags D3DCPUAccessFlags
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the D3D usages.
		/// </summary>
		internal D3D11.ResourceUsage D3DUsage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the graphics interface that created this buffer.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the buffer is locked or not.
		/// </summary>
		public bool IsLocked
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the size of the buffer, in bytes.
		/// </summary>
		public int Size
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the usage for this buffer.
		/// </summary>
		public BufferUsage BufferUsage
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		protected internal abstract void Initialize(GorgonDataStream data);

		/// <summary>
		/// Function used to lock the underlying buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">Flags used when locking the buffer.</param>
		/// <returns>A data stream containing the buffer data.</returns>		
		protected abstract void LockBuffer(BufferLockFlags lockFlags);

		/// <summary>
		/// Function called to unlock the underlying data buffer.
		/// </summary>
		protected internal abstract void UnlockBuffer();

		/// <summary>
		/// Function to update the buffer.
		/// </summary>
		/// <param name="stream">Stream containing the data used to update the buffer.</param>
		/// <param name="destIndex">Index of the sub data to use.</param>
		/// <param name="range2D">2D contraints for the buffer.</param>
		/// <param name="front">3D front face constraint for the buffer.</param>
		/// <param name="back">3D back face constraint for the buffer.</param>
		protected abstract void UpdateBuffer(GorgonDataStream stream, int destIndex, System.Drawing.Rectangle range2D, int front, int back);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBaseBuffer"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create this object.</param>
		/// <param name="usage">Usage for this buffer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is NULL (Nothing in VB.Net).</exception>
		protected GorgonBaseBuffer(GorgonGraphics graphics, BufferUsage usage)			
		{
			GorgonDebug.AssertNull<GorgonGraphics>(graphics, "graphics");

			Graphics = graphics;
			BufferUsage = usage;

			switch (usage)
			{
				case BufferUsage.Dynamic:
					D3DCPUAccessFlags = D3D11.CpuAccessFlags.Write;
					D3DUsage = D3D11.ResourceUsage.Dynamic;
					break;
				case BufferUsage.Immutable:
					D3DCPUAccessFlags = D3D11.CpuAccessFlags.None;
					D3DUsage = D3D11.ResourceUsage.Immutable;
					break;
				case BufferUsage.Staging:
					D3DCPUAccessFlags = D3D11.CpuAccessFlags.Write | D3D11.CpuAccessFlags.Read;
					D3DUsage = D3D11.ResourceUsage.Staging;
					break;
				default:
					D3DCPUAccessFlags = D3D11.CpuAccessFlags.None;
					D3DUsage = D3D11.ResourceUsage.Default;
					break;
			}			

		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					Graphics.RemoveTrackedObject(this);

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
