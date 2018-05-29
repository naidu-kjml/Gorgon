﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, December 14, 2015 8:41:48 PM
// 
#endregion

using Gorgon.Math;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Extension methods for SharpDX object conversion.
	/// </summary>
	internal static class SharpDXExtensions
	{
        /// <summary>
        /// Function to convert a DXGI swap chain description to a <see cref="GorgonSwapChainInfo"/>.
        /// </summary>
        /// <param name="desc">The description to convert.</param>
        /// <param name="name">The name of the swap chain.</param>
        /// <returns>A new <see cref="GorgonSwapChainInfo"/>.</returns>
	    public static GorgonSwapChainInfo ToSwapChainInfo(this DXGI.SwapChainDescription1 desc, string name)
        {
            return new GorgonSwapChainInfo(name)
                   {
                       Width = desc.Width,
                       Height = desc.Height,
                       Format = (BufferFormat)desc.Format,
                       UseFlipMode = desc.SwapEffect == DXGI.SwapEffect.FlipSequential,
                       StretchBackBuffer = desc.Scaling != DXGI.Scaling.None
                   };
        }

	    /// <summary>
	    /// Function to convert a <see cref="IGorgonSwapChainInfo"/> to a DXGI swap chain description value.
	    /// </summary>
	    /// <param name="swapChainInfo">The swap chain info to convert.</param>
	    /// <returns>A DXGI swap chain description.</returns>
	    public static DXGI.SwapChainDescription1 ToSwapChainDesc(this IGorgonSwapChainInfo swapChainInfo)
	    {
	        return new DXGI.SwapChainDescription1
	               {
	                   BufferCount = 2,
	                   AlphaMode = DXGI.AlphaMode.Unspecified,
	                   Flags = DXGI.SwapChainFlags.AllowModeSwitch,
	                   Format = (DXGI.Format)swapChainInfo.Format,
	                   Width = swapChainInfo.Width,
	                   Height = swapChainInfo.Height,
	                   Scaling = swapChainInfo.StretchBackBuffer ? DXGI.Scaling.Stretch : DXGI.Scaling.None,
	                   SampleDescription = ToSampleDesc(GorgonMultisampleInfo.NoMultiSampling),
	                   SwapEffect = swapChainInfo.UseFlipMode ? DXGI.SwapEffect.FlipSequential : DXGI.SwapEffect.Discard,
	                   Usage = DXGI.Usage.RenderTargetOutput
	               };
	    }

		/// <summary>
		/// Function to convert a <see cref="GorgonBox"/> to a D3D11 resource region.
		/// </summary>
		/// <param name="box">The box value to convert.</param>
		/// <returns>The D3D11 resource region.</returns>
		public static D3D11.ResourceRegion ToResourceRegion(this GorgonBox box)
		{
			return new D3D11.ResourceRegion(box.Left, box.Top, box.Front, box.Right, box.Bottom, box.Back);
		}

		/// <summary>
		/// Function to convert a DXGI rational number to a Gorgon rational number.
		/// </summary>
		/// <param name="rational">The rational number to convert.</param>
		/// <returns>A Gorgon rational number.</returns>
		public static GorgonRationalNumber ToGorgonRational(this DXGI.Rational rational)
		{
		    return rational.Denominator == 0 ? GorgonRationalNumber.Empty : new GorgonRationalNumber(rational.Numerator, rational.Denominator);
		}

		/// <summary>
		/// Function to convert a Gorgon rational number to a DXGI rational number.
		/// </summary>
		/// <param name="rational">The rational number to convert.</param>
		/// <returns>The DXGI ration number.</returns>
		public static DXGI.Rational ToDxGiRational(this GorgonRationalNumber rational)
		{
			return new DXGI.Rational(rational.Numerator, rational.Denominator);
		}

	    /// <summary>
	    /// Function to convert a GorgonVideoMode to a ModeDescription.
	    /// </summary>
	    /// <param name="mode">ModeDescription1 to convert.</param>
	    /// <returns>The new mode description.</returns>
	    public static DXGI.ModeDescription ToModeDesc(this GorgonVideoMode mode)
	    {
	        return new DXGI.ModeDescription
	               {
	                   Format = (DXGI.Format)mode.Format,
	                   Height = mode.Height,
	                   Scaling = (DXGI.DisplayModeScaling)mode.Scaling,
	                   Width = mode.Width,
	                   ScanlineOrdering = (DXGI.DisplayModeScanlineOrder)mode.ScanlineOrder,
	                   RefreshRate = mode.RefreshRate.ToDxGiRational()
	               };
	    }

	    /// <summary>
	    /// Function to convert a GorgonVideoMode to a ModeDescription1.
	    /// </summary>
	    /// <param name="mode">ModeDescription to convert.</param>
	    /// <returns>The new mode description.</returns>
	    public static DXGI.ModeDescription1 ToModeDesc1(this GorgonVideoMode mode)
	    {
	        return new DXGI.ModeDescription1
	               {
	                   Format = (DXGI.Format)mode.Format,
	                   Height = mode.Height,
	                   Scaling = (DXGI.DisplayModeScaling)mode.Scaling,
	                   Width = mode.Width,
	                   ScanlineOrdering = (DXGI.DisplayModeScanlineOrder)mode.ScanlineOrder,
	                   RefreshRate = mode.RefreshRate.ToDxGiRational(),
                       Stereo = mode.SupportsStereo
	               };
	    }

	    /// <summary>
	    /// Function to convert a DXGI ModeDescription1 to a <see cref="GorgonVideoMode"/>.
	    /// </summary>
	    /// <param name="mode">ModeDescription1 to convert.</param>
	    /// <param name="gorgonMode">The converted video mode.</param>
	    public static void ToGorgonVideoMode(this DXGI.ModeDescription1 mode, out GorgonVideoMode gorgonMode)
	    {
	        gorgonMode = new GorgonVideoMode(mode);
	    }

        /// <summary>
        /// Function to convert a DXGI ModeDescription to a <see cref="GorgonVideoMode"/>.
        /// </summary>
        /// <param name="mode">ModeDescription to convert.</param>
        /// <returns>The new mode description.</returns>
        public static GorgonVideoMode ToGorgonVideoMode(this DXGI.ModeDescription mode)
	    {
	        return new GorgonVideoMode(mode.ToModeDesc1());
	    }

        /// <summary>
        /// Function to convert a ModeDescription1 to a ModeDescription.
        /// </summary>
        /// <param name="mode">ModeDescription1 to convert.</param>
        /// <returns>The new mode description.</returns>
        public static DXGI.ModeDescription ToModeDesc(this DXGI.ModeDescription1 mode)
		{
			return new DXGI.ModeDescription
			       {
				       Format = mode.Format,
				       Height = mode.Height,
				       Scaling = mode.Scaling,
				       Width = mode.Width,
				       ScanlineOrdering = mode.ScanlineOrdering,
				       RefreshRate = mode.RefreshRate
			       };
		}

		/// <summary>
		/// Function to convert a ModeDescription to a ModeDescription1.
		/// </summary>
		/// <param name="mode">ModeDescription to convert.</param>
		/// <returns>The new mode description.</returns>
		public static DXGI.ModeDescription1 ToModeDesc1(this DXGI.ModeDescription mode)
		{
			return new DXGI.ModeDescription1
			       {
				       Format = mode.Format,
				       Height = mode.Height,
				       Scaling = mode.Scaling,
				       Width = mode.Width,
				       ScanlineOrdering = mode.ScanlineOrdering,
				       RefreshRate = mode.RefreshRate,
				       Stereo = false
			       };
		}

        /// <summary>
        /// Function to convert a GorgonVertexBufferBinding to a D3D11 vertex buffer binding.
        /// </summary>
        /// <param name="binding">The binding to convert.</param>
        /// <returns>The D3D11 vertex buffer binding.</returns>
	    public static D3D11.VertexBufferBinding ToVertexBufferBinding(this GorgonVertexBufferBinding binding)
	    {
            return new D3D11.VertexBufferBinding(binding.VertexBuffer?.Native, binding.Stride, binding.Offset);
	    }

		/// <summary>
		/// Function to convert a <see cref="GorgonMultisampleInfo"/> to a DXGI multi sample description.
		/// </summary>
		/// <param name="samplingInfo">The Gorgon multi sample info to convert.</param>
		/// <returns>The DXGI multi sample description.</returns>
		public static DXGI.SampleDescription ToSampleDesc(this GorgonMultisampleInfo samplingInfo)
		{
			return new DXGI.SampleDescription(samplingInfo.Count, samplingInfo.Quality);
		}
	}
}