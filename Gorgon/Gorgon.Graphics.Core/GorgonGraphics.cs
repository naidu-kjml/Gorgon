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
// Created: April 6, 2018 8:15:10 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.Mathematics.Interop;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI =  SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The primary object for the Gorgon Graphics system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used to initialize the functionality available for rendering hardware accelerated graphics for applications. It is also used in the initialization of other objects used to create graphics. 
    /// </para>
    /// <para>
    /// Typically, a graphics object is assigned to a single <see cref="IGorgonVideoAdapterInfo"/> to provide access to the functionality of that video adapter. If the system has more than once video adapter 
    /// installed then access to subsequent devices can be given by creating a new instance of this object with the appropriate <see cref="IGorgonVideoAdapterInfo"/>.
    /// </para>
    /// <para>
    /// <note type="tip">
    /// <para>
    /// To determine what devices are attached to the system, use a <see cref="EnumerateAdapters"/> method to retreive a list of applicable video adapters. This will contain a list of 
    /// <see cref="IGorgonVideoAdapterInfo"/> objects suitable for construction of the graphics object.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// When creating a graphics object, the user can choose which feature set they will support for a given <see cref="IGorgonVideoAdapterInfo"/> so that older devices may be used. The actual feature set 
    /// support is provided by the <see cref="IGorgonVideoAdapterInfo.FeatureSet"/> on the <see cref="IGorgonVideoAdapterInfo"/> interface.
    /// </para>
    /// <para>
    /// This object is quite simple in its functionality. It provides some state assignment, and a means to submit a <see cref="GorgonDrawCallCommon">draw call</see> so that graphics information can be 
    /// rendered.
    /// </para>
    /// <para><h3>Rendering</h3></para>
    /// <para>
    /// Through the use of <see cref="GorgonDrawCallCommon">draw call types</see>, this object will send data in a stateless fashion. This differs from Direct 3D and other traditional APIs where states are 
    /// set until they're changed (stateful). The approach provided by this object avoids a common problem called state-leakage where a state may have been set prior to drawing, but was forgotten about. 
    /// This can lead to artifacts or can disable rendering entirely and consequently can be quite difficult to track. 
    /// </para>
    /// <para>
    /// When a draw call is sent, it carries all of the required state information (with the exception of a view resource types). This ensures that if a draw call doesn't need a state at a specific time, 
    /// it will be reset to a sensible default (as defined by the developer). 
    /// </para>
    /// <para>
    /// When drawing, Gorgon will determine the minimum required state to send with the final draw call, ensuring no redundant states are set. This type of rendering provides a performance gain since it will 
    /// only set the absolute minimum unique state it needs when the draw call is actually sent to the GPU. This means the user can set the state for a draw call as much as they want without that state being 
    /// sent to the GPU.
    /// </para>
    /// <para>
    /// <h3>Debugging Support</h3>
    /// </para>
    /// <para>
    /// Applications can enable Direct 3D debugging by setting to the <see cref="IsDebugEnabled"/> property to <b>true</b>. This will allow developers to examine underlying failures when rendering using 
    /// Direct 3D. Gorgon also provides memory tracking for any underlying Direct 3D objects when the <see cref="IsObjectTrackingEnabled"/> is set to <b>true</b>. This is useful if a 
    /// <see cref="IDisposable.Dispose"/> call was forgotten by the developer.
    /// </para>
    /// <para>
    /// However, it is not enough to just set these flags to <b>true</b> to enable debugging. Users must also use the DirectX control panel (<c>Debug -> Graphics -> DirectX Control Panel</c>) provided by 
    /// Visual Studio in order to turn on debugging. Finally, the user must then turn on Native debugging in the Project properties of their application (under the <b>Debug</b> tab) so that any debug 
    /// output can be seen in the Output window while running the application.
    /// </para>
    /// <para>
    /// If using a <b>DEBUG</b> compiled version of Gorgon (recommended for development), then the <see cref="IsDebugEnabled"/> property will automatically be set to <b>true</b>.
    /// </para>
    /// <para>
    /// <h3>Requirements</h3>
    /// </para>
    /// <para>
    /// This object requires a minimum of:
    /// <list type="bullet">
    ///     <item>C# 7.3 (Visual Studio 2017 v15.7.2) or better - All libraries in Gorgon.</item>
    ///     <item>.NET 4.7.1 - All libraries in Gorgon.</item>
    ///     <item>Windows 10 v1703, Build 15603 (aka Creators Update).</item>
    ///     <item>Direct 3D 11.4 or better.</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonVideoAdapterInfo"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonDrawIndexCall"/>
    /// <seealso cref="GorgonDrawInstancedCall"/>
    /// <seealso cref="GorgonDrawIndexedInstancedCall"/>
    public sealed class GorgonGraphics
        : IDisposable
    {
        #region Constants.
        /// <summary>
        /// The minimum build number required for the Windows 10 operating system.
        /// </summary>
        internal const int MinWin10Build = 15603;

        /// <summary>
        /// The name of the shader file data used for include files that wish to use the include shader.
        /// </summary>
        public const string BlitterShaderIncludeFileName = "__Gorgon_TextureBlitter_Shader__";

        #endregion

        #region Variables.
        // The D3D 11.4 device context.
        private D3D11.DeviceContext4 _d3DDeviceContext;

        // The D3D 11.4 device.
        private D3D11.Device5 _d3DDevice;

        // The DXGI adapter.
        private DXGI.Adapter4 _dxgiAdapter;

        // The DXGI factory
        private DXGI.Factory5 _dxgiFactory;

        // The render targets currently bound to the pipeline.
        private readonly GorgonRenderTargetView[] _renderTargets = new GorgonRenderTargetView[D3D11.OutputMergerStage.SimultaneousRenderTargetCount];

        // The Native render targets.
        private readonly D3D11.RenderTargetView[] _d3DRtvs = new D3D11.RenderTargetView[D3D11.OutputMergerStage.SimultaneousRenderTargetCount];

        // The viewports used to define the area to render into.
        private readonly DX.ViewportF[] _viewports = new DX.ViewportF[16];

        // The viewports used to define the area to render into.
        private DX.Rectangle[] _scissors = new DX.Rectangle[1];

        // The flag used to determine if a render target and/or depth/stencil is updated.
        private (bool RtvsChanged, bool DepthViewChanged) _isTargetUpdated;

        // The state of the previous draw call.
        // We do a lot of copying between our draw call data to keep it immutable, but this thing will store references
        // directly (for a tiny performance gain, hopefully).
        private readonly D3DState _lastState = new D3DState
                                               {
                                                   PipelineState = new GorgonPipelineState
                                                                   {
                                                                       PrimitiveType = PrimitiveType.None
                                                                   }
                                               };

        // A cache for holding vertex bindings.
        private D3D11.VertexBufferBinding[] _vertexBindingCache = new D3D11.VertexBufferBinding[1];

        // A list of cached pipeline states.
        private readonly List<GorgonPipelineState> _cachedPipelineStates = new List<GorgonPipelineState>();

        // A list of cached pipeline states.
        private readonly List<GorgonSamplerState> _cachedSamplers = new List<GorgonSamplerState>();

        // A syncrhonization lock for multiple thread when dealing with the pipeline state cache.
        private readonly object _stateLock = new object();

        // A syncrhonization lock for multiple thread when dealing with the sampler cache.
        private readonly object _samplerLock = new object();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the Direct 3D 11.4 device context for this graphics instance.
        /// </summary>
        internal D3D11.DeviceContext4 D3DDeviceContext => _d3DDeviceContext;

        /// <summary>
        /// Property to return the Direct 3D 11.4 device for this graphics instance.
        /// </summary>
        internal D3D11.Device5 D3DDevice => _d3DDevice;

        /// <summary>
        /// Property to return the selected DXGI video adapter for this graphics instance.
        /// </summary>
        internal DXGI.Adapter4 DXGIAdapter => _dxgiAdapter;

        /// <summary>
        /// Property to return the DXGI factory used to create DXGI objects.
        /// </summary>
        internal DXGI.Factory5 DXGIFactory => _dxgiFactory;
        
        /// <summary>
        /// Property to return the logging interface used to write out debug messages.
        /// </summary>
        public IGorgonLog Log
        {
            get;
        }

        /// <summary>
        /// Property to return the current depth/stencil view.
        /// </summary>
        public GorgonDepthStencil2DView DepthStencilView
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the currently assigned render targets.
        /// </summary>
        public IReadOnlyList<GorgonRenderTargetView> RenderTargets => _renderTargets;

        /// <summary>
        /// Property to return the list of currently assigned viewports.
        /// </summary>
        public IReadOnlyList<DX.ViewportF> Viewports => _viewports;

        /// <summary>
        /// Property to set or return the video adapter to use for this graphics interface.
        /// </summary>
        public IGorgonVideoAdapterInfo VideoAdapter { get; }

        /// <summary>
        /// Property to return the support available to each format.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return the options available to a <see cref="BufferFormat"/>.
        /// </para>
        /// <para>
        /// The format support and compute shader/uav support value returned will be a bit mask of values from the <see cref="BufferFormatSupport"/> and the <see cref="ComputeShaderFormatSupport"/> 
        /// enumeration respectively.
        /// </para>
        /// </remarks>
        public IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> FormatSupport
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether object tracking is disabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will enable SharpDX's object tracking to ensure references are destroyed upon application exit.
        /// </para>
        /// <para>
        /// The default value for DEBUG mode is <b>true</b>, and for RELEASE it is set to <b>false</b>.  Disabling object tracking will give a slight performance increase.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This flag <i>must</i> be set prior to creating any <see cref="GorgonGraphics"/> object, or else the flag will not take effect.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public static bool IsObjectTrackingEnabled
        {
            get => DX.Configuration.EnableObjectTracking;
            set => DX.Configuration.EnableObjectTracking = value;
        }

        /// <summary>
        /// Property to set or return whether debug output is enabled for the underlying graphics API.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will enable debug output for the underlying graphics API that Gorgon uses to render (Direct 3D 11 at this time). When this is enabled, all functionality will have debugging information that will 
        /// output to the debug output console (Output window in Visual Studio) if the <c>Debug -> Enable Native Debugging</c> is turned on in the application project settings <i>and</i> the DirectX control panel 
        /// is set up to debug the application under Direct 3D 10/11(/12 for Windows 10) application list.
        /// </para>
        /// <para>
        /// When Gorgon is compiled in DEBUG mode, this flag defaults to <b>true</b>, otherwise it defaults to <b>false</b>.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This flag <i>must</i> be set prior to creating any <see cref="GorgonGraphics"/> object, or else the flag will not take effect.
        /// </para>
        /// <para>
        /// The D3D11 SDK Layers DLL must be installed in order for this flag to work. If it is not, then the application may crash.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public static bool IsDebugEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the actual supported <see cref="Core.FeatureSet"/> for this graphics instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A user may request a lower <see cref="Core.FeatureSet"/> than what is supported by the device to allow the application to run on older video adapters that lack support for newer functionality. 
        /// This requested feature set will be returned by this property if supported by the device. 
        /// </para>
        /// <para>
        /// If the user does not request a feature set, or has specified one higher than what the video adapter supports, then the highest feature set supported by the video adapter 
        /// (indicated by the <see cref="IGorgonVideoAdapterInfo.FeatureSet"/> property in the <see cref="IGorgonVideoAdapterInfo"/> class) will be returned.
        /// </para>
        /// </remarks>
        /// <seealso cref="Core.FeatureSet"/>
        public FeatureSet FeatureSet
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to bind a Direct3D index buffer to the pipeline.
        /// </summary>
        /// <param name="bindings">The vertex bindings.</param>
        private void BindVertexBuffers(GorgonVertexBufferBindings bindings)
        {
            (int start, int count) = bindings.GetDirtyItems();

            if (count == 0)
            {
                Array.Clear(_vertexBindingCache, 0, _vertexBindingCache.Length);
                D3DDeviceContext.InputAssembler.SetVertexBuffers(0, _vertexBindingCache);
                return;
            }

            if (_vertexBindingCache.Length != count)
            {
                Array.Resize(ref _vertexBindingCache, count);
            }

            for (int i = 0; i < count; ++i)
            {
                _vertexBindingCache[i] = bindings.Native[i];
            }

            D3DDeviceContext.InputAssembler.SetVertexBuffers(start, _vertexBindingCache);
        }

        /// <summary>
        /// Function to bind the index buffer to the pipeline.
        /// </summary>
        /// <param name="indexBuffer">The index buffer to bind.</param>
        private void BindIndexBuffer(GorgonIndexBuffer indexBuffer)
        {
            if (indexBuffer != null)
            {
                D3DDeviceContext.InputAssembler.SetIndexBuffer(indexBuffer.Native, indexBuffer.Use16BitIndices ? DXGI.Format.R16_UInt : DXGI.Format.R32_UInt, 0);
            }
            else
            {
                D3DDeviceContext.InputAssembler.SetIndexBuffer(null, DXGI.Format.Unknown, 0);
            }
        }

        /// <summary>
        /// Function to update a change enum to determine what states need setting.
        /// </summary>
        /// <param name="expressionResult">The result of an expression.</param>
        /// <param name="requestedChange">The change to make if the expression fails.</param>
        /// <param name="currentChanges">The current list of changes.</param>
        /// <returns><b>true</b> if a change was made, <b>false</b> if not.</returns>
        private static bool ChangeBuilder(bool expressionResult, DrawCallChanges requestedChange, ref DrawCallChanges currentChanges)
        {
            if (!expressionResult)
            {
                currentChanges |= requestedChange;
            }

            return !expressionResult;
        }

        /// <summary>
        /// Function to assign sampler states to a shader
        /// </summary>
        /// <param name="shaderStage">The shader stage to use.</param>
        /// <param name="samplers">The samplers to apply.</param>
        private static void BindSamplers(D3D11.CommonShaderStage shaderStage, GorgonSamplerStates samplers)
        {
            if (samplers == null)
            {
                shaderStage.SetSamplers(0);
                return;
            }

            (int start, int count) = samplers.GetDirtyItems();

            shaderStage.SetSamplers(start, count, samplers.Native);
        }


        /// <summary>
        /// Function to set the current list of scissor rectangles.
        /// </summary>
        /// <param name="rectangles">The list of scissor rectangles to apply.</param>
        private void SetScissors(IReadOnlyList<DX.Rectangle> rectangles)
        {
            if ((rectangles == null) || (rectangles.Count == 0))
            {
                D3DDeviceContext.Rasterizer.SetScissorRectangles((DX.Rectangle[])null);
                return;
            }

            if (_scissors.Length != rectangles.Count)
            {
                _scissors = new DX.Rectangle[rectangles.Count];
            }

            for (int i = 0; i < rectangles.Count; ++i)
            {
                _scissors[i] = rectangles[i];
            }

            D3DDeviceContext.Rasterizer.SetScissorRectangles(_scissors);
        }

        /// <summary>
        /// Function to compare the two list of scissor rectangle lists for equality.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        private bool CompareScissorRects(IReadOnlyList<DX.Rectangle> left, IReadOnlyList<DX.Rectangle> right)
        {
            if (left == right)
            {
                return true;
            }

            if (((left == null) && (right != null)) || (left == null))
            {
                return false;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; ++i)
            {
                if (!left[i].Equals(right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Function to apply the pipeline state.
        /// </summary>
        /// <param name="currentState">The state passed in from the draw call.</param>
        /// <param name="changes">The changes to make.</param>
        private void ApplyState(GorgonPipelineState currentState, DrawCallChanges changes)
        {
            if (changes == DrawCallChanges.None)
            {
                return;
            }

            if ((changes & DrawCallChanges.Topology) == DrawCallChanges.Topology)
            {
                D3DDeviceContext.InputAssembler.PrimitiveTopology = (D3D.PrimitiveTopology)_lastState.PipelineState.PrimitiveType;
            }

            if ((changes & DrawCallChanges.RasterState) == DrawCallChanges.RasterState)
            {
                D3DDeviceContext.Rasterizer.State = currentState.D3DRasterState;
            }

            if ((changes & DrawCallChanges.Scissors) == DrawCallChanges.Scissors)
            {
                SetScissors(currentState.RasterState.ScissorRectangles);
            }

            if ((changes & DrawCallChanges.PixelShader) == DrawCallChanges.PixelShader)
            {
                D3DDeviceContext.PixelShader.Set(currentState.PixelShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.VertexShader) == DrawCallChanges.VertexShader)
            {
                D3DDeviceContext.VertexShader.Set(currentState.VertexShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.GeometryShader) == DrawCallChanges.GeometryShader)
            {
                D3DDeviceContext.GeometryShader.Set(currentState.GeometryShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.DomainShader) == DrawCallChanges.DomainShader)
            {
                D3DDeviceContext.DomainShader.Set(currentState.DomainShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.HullShader) == DrawCallChanges.HullShader)
            {
                D3DDeviceContext.HullShader.Set(currentState.HullShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.ComputeShader) == DrawCallChanges.ComputeShader)
            {
                D3DDeviceContext.ComputeShader.Set(currentState.ComputeShader?.NativeShader);
            }
        }

        /// <summary>
        /// Function to build a merged render state.
        /// </summary>
        /// <param name="currentState">The current state.</param>
        /// <returns>A set of changes that need to be applied to the pipeline.</returns>
        private DrawCallChanges BuildStateChanges(GorgonPipelineState currentState)
        {
            GorgonPipelineState lastState = _lastState.PipelineState;
            DrawCallChanges changes = DrawCallChanges.None;


            if (ChangeBuilder(_lastState.PipelineState.PrimitiveType == currentState.PrimitiveType, DrawCallChanges.Topology, ref changes))
            {
                _lastState.PipelineState.PrimitiveType = currentState.PrimitiveType;
            }

            if (ChangeBuilder(lastState.D3DRasterState == currentState.D3DRasterState, DrawCallChanges.RasterState, ref changes))
            {
                _lastState.PipelineState.D3DRasterState = currentState.D3DRasterState;
            }

            if (ChangeBuilder(CompareScissorRects(lastState.RasterState?.ScissorRectangles, currentState.RasterState.ScissorRectangles), DrawCallChanges.Scissors, ref changes))
            {
                _lastState.PipelineState.RasterState = currentState.RasterState;
            }

            if (ChangeBuilder(lastState.VertexShader == currentState.VertexShader, DrawCallChanges.VertexShader, ref changes))
            {
                _lastState.PipelineState.VertexShader = currentState.VertexShader;
            }

            if (ChangeBuilder(lastState.PixelShader == currentState.PixelShader, DrawCallChanges.PixelShader, ref changes))
            {
                _lastState.PipelineState.PixelShader = currentState.PixelShader;
            }

            if (ChangeBuilder(lastState.GeometryShader == currentState.GeometryShader, DrawCallChanges.GeometryShader, ref changes))
            {
                _lastState.PipelineState.GeometryShader = currentState.GeometryShader;
            }

            if (ChangeBuilder(lastState.DomainShader == currentState.DomainShader, DrawCallChanges.DomainShader, ref changes))
            {
                _lastState.PipelineState.DomainShader = currentState.DomainShader;
            }

            if (ChangeBuilder(lastState.HullShader == currentState.HullShader, DrawCallChanges.HullShader, ref changes))
            {
                _lastState.PipelineState.HullShader = currentState.HullShader;
            }

            if (ChangeBuilder(lastState.ComputeShader == currentState.ComputeShader, DrawCallChanges.ComputeShader, ref changes))
            {
                _lastState.PipelineState.ComputeShader = currentState.ComputeShader;
            }

            return changes;
        }

        /// <summary>
        /// Function to bind the constant buffers to a specific shader stage.
        /// </summary>
        /// <param name="shaderType">The shader stage.</param>
        /// <param name="constantBuffers">The constant buffers to bind.</param>
        private void BindConstantBuffers(ShaderType shaderType, GorgonConstantBuffers constantBuffers)
        {
            if (constantBuffers == null)
            {
                switch (shaderType)
                {
                    case ShaderType.Vertex:
                        D3DDeviceContext.VSSetConstantBuffers1(0, 0, (D3D11.Buffer[])null, null, null);
                        break;
                    case ShaderType.Pixel:
                        D3DDeviceContext.PSSetConstantBuffers1(0, 0, (D3D11.Buffer[])null, null, null);
                        break;
                    case ShaderType.Geometry:
                        D3DDeviceContext.GSSetConstantBuffers1(0, 0, (D3D11.Buffer[])null, null, null);
                        break;
                    case ShaderType.Domain:
                        D3DDeviceContext.DSSetConstantBuffers1(0, 0, (D3D11.Buffer[])null, null, null);
                        break;
                    case ShaderType.Hull:
                        D3DDeviceContext.HSSetConstantBuffers1(0, 0, (D3D11.Buffer[])null, null, null);
                        break;
                    case ShaderType.Compute:
                        D3DDeviceContext.CSSetConstantBuffers1(0, 0, (D3D11.Buffer[])null, null, null);
                        break;
                }
                return;
            }

            (int start, int count) = constantBuffers.GetDirtyItems();

            // Ensure that we pick up changes to the constant buffers view range.
            for (int i = 0; i < count; ++i)
            {
                GorgonConstantBufferView view = constantBuffers[i + start];

                if (!view.ViewAdjusted)
                {
                    continue;
                }

                view.ViewAdjusted = false;
                constantBuffers.ViewStart[i] = view.StartElement * 16;
                constantBuffers.ViewCount[i] = (view.ElementCount + 15) & ~15;
            }
            
            switch (shaderType)
            {
                case ShaderType.Vertex:
                    D3DDeviceContext.VSSetConstantBuffers1(start, count, constantBuffers.Native, constantBuffers.ViewStart, constantBuffers.ViewCount);
                    break;
                case ShaderType.Pixel:
                    D3DDeviceContext.PSSetConstantBuffers1(start, count, constantBuffers.Native, constantBuffers.ViewStart, constantBuffers.ViewCount);
                    break;
                case ShaderType.Geometry:
                    D3DDeviceContext.GSSetConstantBuffers1(start, count, constantBuffers.Native, constantBuffers.ViewStart, constantBuffers.ViewCount);
                    break;
                case ShaderType.Domain:
                    D3DDeviceContext.DSSetConstantBuffers1(start, count, constantBuffers.Native, constantBuffers.ViewStart, constantBuffers.ViewCount);
                    break;
                case ShaderType.Hull:
                    D3DDeviceContext.HSSetConstantBuffers1(start, count, constantBuffers.Native, constantBuffers.ViewStart, constantBuffers.ViewCount);
                    break;
                case ShaderType.Compute:
                    D3DDeviceContext.CSSetConstantBuffers1(start, count, constantBuffers.Native, constantBuffers.ViewStart, constantBuffers.ViewCount);
                    break;
            }
        }

        /// <summary>
        /// Function to bind the shader resource views to a specific shader stage.
        /// </summary>
        /// <param name="shaderType">The shader stage.</param>
        /// <param name="srvs">The shader resource views to bind.</param>
        private void BindSrvs(ShaderType shaderType, GorgonShaderResourceViews srvs)
        {
            if (srvs == null)
            {
                switch (shaderType)
                {
                    case ShaderType.Vertex:
                        D3DDeviceContext.VertexShader.SetShaderResources(0, 0, (D3D11.ShaderResourceView[])null);
                        break;
                    case ShaderType.Pixel:
                        D3DDeviceContext.PixelShader.SetShaderResources(0, 0, (D3D11.ShaderResourceView[])null);
                        break;
                    case ShaderType.Geometry:
                        D3DDeviceContext.GeometryShader.SetShaderResources(0, 0, (D3D11.ShaderResourceView[])null);
                        break;
                    case ShaderType.Domain:
                        D3DDeviceContext.DomainShader.SetShaderResources(0, 0, (D3D11.ShaderResourceView[])null);
                        break;
                    case ShaderType.Hull:
                        D3DDeviceContext.HullShader.SetShaderResources(0, 0, (D3D11.ShaderResourceView[])null);
                        break;
                    case ShaderType.Compute:
                        D3DDeviceContext.ComputeShader.SetShaderResources(0, 0, (D3D11.ShaderResourceView[])null);
                        break;
                }
                return;
            }

            (int start, int count) = srvs.GetDirtyItems();
            
            switch (shaderType)
            {
                case ShaderType.Vertex:
                    D3DDeviceContext.VertexShader.SetShaderResources(start, count, srvs.Native);
                    break;
                case ShaderType.Pixel:
                    D3DDeviceContext.PixelShader.SetShaderResources(start, count, srvs.Native);
                    break;
                case ShaderType.Geometry:
                    D3DDeviceContext.GeometryShader.SetShaderResources(start, count, srvs.Native);
                    break;
                case ShaderType.Domain:
                    D3DDeviceContext.DomainShader.SetShaderResources(start, count, srvs.Native);
                    break;
                case ShaderType.Hull:
                    D3DDeviceContext.HullShader.SetShaderResources(start, count, srvs.Native);
                    break;
                case ShaderType.Compute:
                    D3DDeviceContext.ComputeShader.SetShaderResources(start, count, srvs.Native);
                    break;
            }
        }

        /// <summary>
        /// Function to bind resources to the pipeline.
        /// </summary>
        /// <param name="resourceChanges">The changes for the resources.</param>
        private void BindResources(DrawCallChanges resourceChanges)
        {
            if (resourceChanges == DrawCallChanges.None)
            {
                return;
            }

            if ((resourceChanges & DrawCallChanges.InputLayout) == DrawCallChanges.InputLayout)
            {
                D3DDeviceContext.InputAssembler.InputLayout = _lastState.InputLayout.D3DInputLayout;
            }

            if ((resourceChanges & DrawCallChanges.VertexBuffers) == DrawCallChanges.VertexBuffers)
            {
                BindVertexBuffers(_lastState.VertexBuffers);
            }
            
            if ((resourceChanges & DrawCallChanges.IndexBuffer) == DrawCallChanges.IndexBuffer)
            {
                BindIndexBuffer(_lastState.IndexBuffer);
            }

            if ((resourceChanges & DrawCallChanges.PsSamplers) == DrawCallChanges.PsSamplers)
            {
                BindSamplers(D3DDeviceContext.PixelShader, _lastState.PsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.VsSamplers) == DrawCallChanges.VsSamplers)
            {
                BindSamplers(D3DDeviceContext.VertexShader, _lastState.VsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.GsSamplers) == DrawCallChanges.GsSamplers)
            {
                BindSamplers(D3DDeviceContext.GeometryShader, _lastState.GsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.DsSamplers) == DrawCallChanges.DsSamplers)
            {
                BindSamplers(D3DDeviceContext.DomainShader, _lastState.DsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.HsSamplers) == DrawCallChanges.HsSamplers)
            {
                BindSamplers(D3DDeviceContext.HullShader, _lastState.HsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.CsSamplers) == DrawCallChanges.CsSamplers)
            {
                BindSamplers(D3DDeviceContext.ComputeShader, _lastState.CsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.VsConstants) == DrawCallChanges.VsConstants)
            {
                BindConstantBuffers(ShaderType.Vertex, _lastState.VsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.PsConstants) == DrawCallChanges.PsConstants)
            {
                BindConstantBuffers(ShaderType.Pixel, _lastState.PsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.GsConstants) == DrawCallChanges.GsConstants)
            {
                BindConstantBuffers(ShaderType.Geometry, _lastState.GsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.DsConstants) == DrawCallChanges.DsConstants)
            {
                BindConstantBuffers(ShaderType.Domain, _lastState.DsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.HsConstants) == DrawCallChanges.HsConstants)
            {
                BindConstantBuffers(ShaderType.Hull, _lastState.HsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.CsConstants) == DrawCallChanges.CsConstants)
            {
                BindConstantBuffers(ShaderType.Compute, _lastState.CsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.VsResourceViews) == DrawCallChanges.VsResourceViews)
            {
                BindSrvs(ShaderType.Vertex, _lastState.VsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.PsResourceViews) == DrawCallChanges.PsResourceViews)
            {
                BindSrvs(ShaderType.Pixel, _lastState.PsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.GsResourceViews) == DrawCallChanges.GsResourceViews)
            {
                BindSrvs(ShaderType.Geometry, _lastState.GsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.DsResourceViews) == DrawCallChanges.DsResourceViews)
            {
                BindSrvs(ShaderType.Domain, _lastState.DsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.HsResourceViews) == DrawCallChanges.HsResourceViews)
            {
                BindSrvs(ShaderType.Hull, _lastState.HsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.CsResourceViews) == DrawCallChanges.CsResourceViews)
            {
                BindSrvs(ShaderType.Compute, _lastState.CsSrvs);
            }
        }

        /// <summary>
        /// Function to build a merged draw state.
        /// </summary>
        /// <param name="currentState">The current state.</param>
        /// <returns>A set of changes that need to be applied to the pipeline.</returns>
        private DrawCallChanges BuildDrawCallResources(D3DState currentState)
        {
            DrawCallChanges changes = DrawCallChanges.None;

            // Ensure we have an input layout.
            ChangeBuilder(_lastState.InputLayout == currentState.InputLayout, DrawCallChanges.InputLayout, ref changes);

            if ((ChangeBuilder(currentState.VertexBuffers.DirtyEquals(_lastState.VertexBuffers), DrawCallChanges.VertexBuffers, ref changes))
                || ((changes & DrawCallChanges.InputLayout) == DrawCallChanges.InputLayout))
            {
                Debug.Assert(currentState.VertexBuffers != null, "VertexBuffers on current draw state is null - This is not allowed.");

                _lastState.VertexBuffers = currentState.VertexBuffers;
            }

            if (ChangeBuilder(_lastState.IndexBuffer == currentState.IndexBuffer, DrawCallChanges.IndexBuffer, ref changes))
            {
                _lastState.IndexBuffer = currentState.IndexBuffer;
            }

            if (ChangeBuilder(currentState.PsSamplers.DirtyEquals(_lastState.PsSamplers),
                              DrawCallChanges.PsSamplers,
                              ref changes))
            {
                Debug.Assert(currentState.PsSamplers != null, "PixelShader samplers is null - This is not allowed.");
                _lastState.PsSamplers = currentState.PsSamplers;
            }

            if (ChangeBuilder(currentState.VsSamplers.DirtyEquals(_lastState.VsSamplers),
                              DrawCallChanges.VsSamplers,
                              ref changes))
            {
                Debug.Assert(currentState.VsSamplers != null, "VertexShader samplers is null - This is not allowed.");
                _lastState.VsSamplers = currentState.VsSamplers;
            }

            if (ChangeBuilder(currentState.GsSamplers.DirtyEquals(_lastState.GsSamplers),
                              DrawCallChanges.GsSamplers,
                              ref changes))
            {
                Debug.Assert(currentState.GsSamplers != null, "GeometryShader samplers is null - This is not allowed.");
                _lastState.GsSamplers = currentState.GsSamplers;
            }

            if (ChangeBuilder(currentState.DsSamplers.DirtyEquals(_lastState.DsSamplers),
                              DrawCallChanges.DsSamplers,
                              ref changes))
            {
                Debug.Assert(currentState.DsSamplers != null, "DomainShader samplers is null - This is not allowed.");
                _lastState.DsSamplers = currentState.DsSamplers;
            }

            if (ChangeBuilder(currentState.HsSamplers.DirtyEquals(_lastState.HsSamplers),
                              DrawCallChanges.HsSamplers,
                              ref changes))
            {
                Debug.Assert(currentState.HsSamplers != null, "HullShader samplers is null - This is not allowed.");
                _lastState.HsSamplers = currentState.HsSamplers;
            }

            if (ChangeBuilder(currentState.CsSamplers.DirtyEquals(_lastState.CsSamplers),
                              DrawCallChanges.CsSamplers,
                              ref changes))
            {
                Debug.Assert(currentState.CsSamplers != null, "ComputeShader samplers is null - This is not allowed.");
                _lastState.CsSamplers = currentState.CsSamplers;
            }

            if (ChangeBuilder(currentState.VsConstantBuffers.DirtyEquals(_lastState.VsConstantBuffers),
                              DrawCallChanges.VsConstants,
                              ref changes))
            {
                Debug.Assert(currentState.VsConstantBuffers != null, "VertexShader constants is null - This is now allowed.");
                _lastState.VsConstantBuffers = currentState.VsConstantBuffers;
            }

            if (ChangeBuilder(currentState.PsConstantBuffers.DirtyEquals(_lastState.PsConstantBuffers),
                              DrawCallChanges.PsConstants,
                              ref changes))
            {
                Debug.Assert(currentState.PsConstantBuffers != null, "PixelShader constants is null - This is now allowed.");
                _lastState.PsConstantBuffers = currentState.PsConstantBuffers;
            }

            if (ChangeBuilder(currentState.GsConstantBuffers.DirtyEquals(_lastState.GsConstantBuffers),
                              DrawCallChanges.GsConstants,
                              ref changes))
            {
                Debug.Assert(currentState.GsConstantBuffers != null, "GeometryShader constants is null - This is now allowed.");
                _lastState.GsConstantBuffers = currentState.GsConstantBuffers;
            }

            if (ChangeBuilder(currentState.DsConstantBuffers.DirtyEquals(_lastState.DsConstantBuffers),
                              DrawCallChanges.DsConstants,
                              ref changes))
            {
                Debug.Assert(currentState.DsConstantBuffers != null, "DomainShader constants is null - This is now allowed.");
                _lastState.DsConstantBuffers = currentState.DsConstantBuffers;
            }

            if (ChangeBuilder(currentState.HsConstantBuffers.DirtyEquals(_lastState.HsConstantBuffers),
                              DrawCallChanges.HsConstants,
                              ref changes))
            {
                Debug.Assert(currentState.HsConstantBuffers != null, "HullShader constants is null - This is now allowed.");
                _lastState.HsConstantBuffers = currentState.HsConstantBuffers;
            }

            if (ChangeBuilder(currentState.HsConstantBuffers.DirtyEquals(_lastState.CsConstantBuffers),
                              DrawCallChanges.CsConstants,
                              ref changes))
            {
                Debug.Assert(currentState.CsConstantBuffers != null, "ComputeShader constants is null - This is now allowed.");
                _lastState.CsConstantBuffers = currentState.CsConstantBuffers;
            }

            if (ChangeBuilder(currentState.VsSrvs.DirtyEquals(_lastState.VsSrvs),
                              DrawCallChanges.VsResourceViews,
                              ref changes))
            {
                Debug.Assert(currentState.VsSrvs != null, "VertexShader srvs are null - This is now allowed.");
                _lastState.VsSrvs = currentState.VsSrvs;
            }

            if (ChangeBuilder(currentState.PsSrvs.DirtyEquals(_lastState.PsSrvs),
                              DrawCallChanges.PsResourceViews,
                              ref changes))
            {
                Debug.Assert(currentState.PsSrvs != null, "PixelShader srvs are null - This is now allowed.");
                _lastState.PsSrvs = currentState.PsSrvs;
            }

            if (ChangeBuilder(currentState.GsSrvs.DirtyEquals(_lastState.GsSrvs),
                              DrawCallChanges.GsResourceViews,
                              ref changes))
            {
                Debug.Assert(currentState.GsSrvs != null, "GeometryShader srvs are null - This is now allowed.");
                _lastState.GsSrvs = currentState.GsSrvs;
            }

            if (ChangeBuilder(currentState.DsSrvs.DirtyEquals(_lastState.DsSrvs),
                              DrawCallChanges.DsResourceViews,
                              ref changes))
            {
                Debug.Assert(currentState.DsSrvs != null, "DomainShader srvs are null - This is now allowed.");
                _lastState.DsSrvs = currentState.DsSrvs;
            }

            if (ChangeBuilder(currentState.HsSrvs.DirtyEquals(_lastState.HsSrvs),
                              DrawCallChanges.HsResourceViews,
                              ref changes))
            {
                Debug.Assert(currentState.HsSrvs != null, "HullShader srvs are null - This is now allowed.");
                _lastState.HsSrvs = currentState.HsSrvs;
            }

            if (ChangeBuilder(currentState.HsSrvs.DirtyEquals(_lastState.CsSrvs),
                              DrawCallChanges.CsResourceViews,
                              ref changes))
            {
                Debug.Assert(currentState.CsSrvs != null, "ComputeShader srvs are null - This is now allowed.");
                _lastState.CsSrvs = currentState.CsSrvs;
            }

            return changes;
        }

        /// <summary>
        /// Function to retrieve the multi sample maximum quality level support for a given format.
        /// </summary>
        /// <param name="device">The D3D 11 device to use.</param>
        /// <param name="format">The DXGI format support to evaluate.</param>
        /// <returns>A <see cref="GorgonMultisampleInfo"/> value containing the max count and max quality level.</returns>
        private GorgonMultisampleInfo GetMultisampleSupport(D3D11.Device5 device, DXGI.Format format)
        {
            try
            {
                for (int count = D3D11.Device.MultisampleCountMaximum; count >= 1; count = count / 2)
                {
                    int quality = device.CheckMultisampleQualityLevels1(format, count, D3D11.CheckMultisampleQualityLevelsFlags.None);

                    if ((quality < 1) || (count == 1))
                    {
                        continue;
                    }

                    return new GorgonMultisampleInfo(count, quality - 1);
                }
            }
            catch (DX.SharpDXException sdEx)
            {
                Log.Print($"ERROR: Could not retrieve a multisample quality level max for format: [{format}]. Exception: {sdEx.Message}", LoggingLevel.Verbose);
            }

            return GorgonMultisampleInfo.NoMultiSampling;
        }

        /// <summary>
        /// Function to create the Direct 3D device and Adapter for use with Gorgon.
        /// </summary>
        /// <param name="adapterInfo">The adapter to use.</param>
        /// <param name="requestedFeatureLevel">The requested feature set for the device.</param>
        /// <returns>A tuple containing the Direct3D device object, DXGI factory, DXGI video adapter, and actual feature set.</returns>
        private (D3D11.Device5, DXGI.Factory5, DXGI.Adapter4) CreateDevice(IGorgonVideoAdapterInfo adapterInfo, D3D.FeatureLevel requestedFeatureLevel)
        {
            D3D11.DeviceCreationFlags flags = IsDebugEnabled ? D3D11.DeviceCreationFlags.Debug : D3D11.DeviceCreationFlags.None;
            DXGI.Factory5 resultFactory;
            DXGI.Adapter4 resultAdapter;
            D3D11.Device5 resultDevice;

            using (DXGI.Factory2 factory2 = new DXGI.Factory2(IsDebugEnabled))
            {
                resultFactory = factory2.QueryInterface<DXGI.Factory5>();

                using (DXGI.Adapter adapter = (adapterInfo.VideoDeviceType == VideoDeviceType.Hardware ? resultFactory.GetAdapter1(adapterInfo.Index) : resultFactory.GetWarpAdapter()))
                {
                    resultAdapter = adapter.QueryInterface<DXGI.Adapter4>();

                    using (D3D11.Device device = new D3D11.Device(resultAdapter, flags, requestedFeatureLevel)
                                                 {
                                                     DebugName = $"'{adapterInfo.Name}' D3D11.4 {(adapterInfo.VideoDeviceType == VideoDeviceType.Software ? "Software Adapter" : "Adapter")}"
                                                 })
                    {
                        resultDevice = device.QueryInterface<D3D11.Device5>();

		                Log.Print($"Direct 3D 11.4 device created for video adapter '{adapterInfo.Name}' at feature set [{(FeatureSet)resultDevice.FeatureLevel}]", LoggingLevel.Simple);
                    }
                }
            }

            return (resultDevice, resultFactory, resultAdapter);
        }
        
        /// <summary>
        /// Function to enumerate format support for all <see cref="BufferFormat"/> values.
        /// </summary>
        /// <param name="device">The D3D11 device object.</param>
        /// <returns>The list of format support information objects for each <see cref="BufferFormat"/>.</returns>
        private IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> EnumerateFormatSupport(D3D11.Device5 device)
        {
            IEnumerable<BufferFormat> formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));
            var result = new Dictionary<BufferFormat, IGorgonFormatSupportInfo>();

            // Get support values for each format.
            foreach (BufferFormat format in formats)
            {
                DXGI.Format dxgiFormat = (DXGI.Format)format;

                // NOTE: NV12 seems to come back as value of -92093664, no idea what the extra flags might be, the documentation for D3D doesn't
                //       specify the flags.
                D3D11.FormatSupport formatSupport = device.CheckFormatSupport(dxgiFormat);
                D3D11.ComputeShaderFormatSupport computeSupport = device.CheckComputeShaderFormatSupport(dxgiFormat);

                result[format] = new FormatSupportInfo(format, formatSupport, computeSupport, GetMultisampleSupport(device, dxgiFormat));
            }

            return result;
        }

		/// <summary>
		/// Function to validate the depth/stencil view.
		/// </summary>
		/// <param name="view">The depth/stencil view to evaluate.</param>
		/// <param name="firstTarget">The first non-null target.</param>
		private void ValidateRtvAndDsv(GorgonDepthStencil2DView view, GorgonRenderTargetView firstTarget)
		{
			if ((firstTarget == null)
			    && (view == null))
			{
				return;
			}
            
            if (firstTarget != null)
		    {
		        // Ensure that we are only bound once to the pipeline.
		        if (RenderTargets.Count(item => item == firstTarget) > 1)
		        {
		            throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_ALREADY_BOUND);
		        }

                // Ensure our dimensions match, and multi-sample settings match.
		        foreach (GorgonRenderTargetView rtv in RenderTargets.Where(item => (item != null) && (item != firstTarget)))
		        {
		            if (rtv.Resource.ResourceType != firstTarget.Resource.ResourceType)
		            {
		                throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_NOT_SAME_TYPE, firstTarget.Resource.ResourceType));
		            }

		            switch (firstTarget.Resource.ResourceType)
		            {
		                case GraphicsResourceType.Texture2D:
		                    var left2D = (GorgonRenderTarget2DView)firstTarget;
		                    var right2D = (GorgonRenderTarget2DView)rtv;

		                    if ((left2D.Width != right2D.Width) && (left2D.Height != right2D.Height) && (left2D.ArrayCount != right2D.ArrayCount))
		                    {
                                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH);
		                    }

		                    if (!left2D.MultisampleInfo.Equals(right2D.MultisampleInfo))
		                    {
                                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_MULTISAMPLE_MISMATCH);
		                    }
		                    break;
		                case GraphicsResourceType.Texture3D:
		                    var left3D = (GorgonRenderTarget3DView)firstTarget;
		                    var right3D = (GorgonRenderTarget3DView)rtv;

		                    if ((left3D.Width != right3D.Width) && (left3D.Height != right3D.Height) && (left3D.Depth != right3D.Depth))
		                    {
		                        throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH);
		                    }
		                    break;
		                default:
		                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_UNSUPPORTED_RESOURCE, firstTarget.Resource.ResourceType));
		            }
		        }
		    }

		    if ((firstTarget == null)
		        || (view == null))
		    {
		        return;
		    }

            // Ensure all resources are the same type.
			if (view.Texture.ResourceType != firstTarget.Resource.ResourceType)
			{
				throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_TYPE_MISMATCH, view.Texture.ResourceType));
			}

		    var rtv2D = (GorgonRenderTarget2DView)firstTarget;
            
			// Ensure the depth stencil array/depth counts match for all resources.
			if (view.ArrayCount != rtv2D.ArrayCount)
			{
				throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_ARRAYCOUNT_MISMATCH, view.Texture.Name));
			}

			// Check to ensure that multisample info matches.
			if (!view.Texture.MultisampleInfo.Equals(rtv2D.MultisampleInfo))
			{
				throw new GorgonException(GorgonResult.CannotBind,
				                          string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_MULTISAMPLE_MISMATCH,
				                                        view.MultisampleInfo.Quality,
				                                        view.MultisampleInfo.Count));
			}

			if ((view.Width != rtv2D.Width)
			    || (view.Height != rtv2D.Height))
			{
				throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_RESOURCE_MISMATCH);
			}
		}

        /// <summary>
        /// Function to unbind a render target that is bound as a shader input.
        /// </summary>
        private void UnbindShaderInputs(int rtvCount)
        {
            // This can happen quite easily due to how we're handling draw calls (i.e. stateless).  So we won't log anything here and we'll just unbind for the time being.
            // This may have a small performance penalty.
            /*
            for (int i = 0; i < rtvCount; ++i)
            {
                GorgonRenderTargetView view = _renderTargets[i];

                if ((view == null) || (view.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource)
                {
                    continue;
                }
                
                UnbindFromShader(view.Resource, ref _lastDrawCall.VertexShaderResourceViews.GetDirtyItems());
                UnbindFromShader(view.Resource, ref _lastDrawCall.PixelShaderResourceViews.GetDirtyItems());
            }

            void UnbindFromShader(GorgonTexture renderTarget, ref (int Start, int Count, GorgonShaderResourceView[] Bindings) bindings)
            {
                for (int i = bindings.Start; i < bindings.Start + bindings.Count; ++i)
                {
                    GorgonShaderResourceView srv = bindings.Bindings[i];
                    
                    if ((srv == null) || (renderTarget != srv.Resource))
                    {
                        continue;
                    }
                    
                    _lastDrawCall.PixelShaderResourceViews[i] = null;
                    SetShaderResourceViews(ShaderType.Pixel, _lastDrawCall.PixelShaderResourceViews);
                }
            }
            */
        }

        /// <summary>
        /// Function to initialize a <see cref="GorgonPipelineState" /> object with Direct 3D 11 state objects by creating new objects for the unassigned values.
        /// </summary>
        /// <param name="pipelineState">The pipeline state.</param>
        /// <param name="blendState">An existing blend state to use.</param>
        /// <param name="depthStencilState">An existing depth/stencil state to use.</param>
        /// <param name="rasterState">An existing rasterizer state to use.</param>
        /// <returns>A new <see cref="GorgonPipelineState"/>.</returns>
        private void InitializePipelineState(GorgonPipelineState pipelineState,
                                                            D3D11.BlendState1 blendState,
                                                            D3D11.DepthStencilState depthStencilState,
                                                            D3D11.RasterizerState1 rasterState)
        {
            pipelineState.D3DRasterState = rasterState;
            pipelineState.D3DBlendState = blendState;
            pipelineState.D3DDepthStencilState = depthStencilState;
            
            if ((rasterState == null) && (pipelineState.RasterState != null))
            {
                pipelineState.D3DRasterState = pipelineState.RasterState.GetD3D11RasterState(_d3DDevice);
            }

            /*
            if ((depthStencilState == null) && (pipelineState.DepthStencilState != null))
            {
                result.D3DDepthStencilState = new D3D11.DepthStencilState(_d3DDevice, pipelineState.DepthStencilState.ToDepthStencilStateDesc())
                                              {
                                                  DebugName = "Gorgon D3D11DepthStencilState"
                                              };
            }

            
            if ((blendState != null) || (pipelineState.BlendStates == null) || (pipelineState.BlendStates.Count == 0))
            {
                return result;
            }

            int maxStates = pipelineState.BlendStates.Count.Min(D3D11.OutputMergerStage.SimultaneousRenderTargetCount);

            D3D11.BlendStateDescription1 desc = new D3D11.BlendStateDescription1
                       {
                           IndependentBlendEnable = pipelineState.IsIndependentBlendingEnabled,
                           AlphaToCoverageEnable = pipelineState.IsAlphaToCoverageEnabled
                       };

            for (int i = 0; i < maxStates; ++i)
            {
                desc.RenderTarget[i] = pipelineState.BlendStates[i].ToRenderTargetBlendStateDesc1();
            }

            result.D3DBlendState = new D3D11.BlendState1(_d3DDevice, desc)
                                   {
                                       DebugName = "Gorgon D3D11BlendState"
                                   };
                                   */
        }

        /// <summary>
        /// Function clear the binding state for render target resources.
        /// </summary>
        /// <param name="start">The starting index.</param>
        /// <param name="count">The number of items.</param>
        private void ClearRenderTargetBindings(int start, int count)
        {
            for (int i = start; i < count; ++i)
            {
                ref GorgonRenderTargetView view = ref _renderTargets[i];

                if (view != null)
                {
                    view.Resource.BindState = BindState.None;
                }

                view = null;
            }
        }

        /// <summary>
        /// Function to assign the render targets.
        /// </summary>
        /// <param name="rtvCount">The number of render targets to update.</param>
        private void SetRenderTargetAndDepthViews(int rtvCount)
        {
            if ((!_isTargetUpdated.RtvsChanged) && (!_isTargetUpdated.DepthViewChanged))
            {
                return;
            }

#if DEBUG
            ValidateRtvAndDsv(DepthStencilView, RenderTargets.FirstOrDefault(item => item != null));
#endif

            UnbindShaderInputs(rtvCount);

            if (rtvCount == 0)
            {
                Array.Clear(_d3DRtvs, 0, _d3DRtvs.Length);
                rtvCount = _d3DRtvs.Length;
            }
            else
            {
                for (int i = 0; i < rtvCount; ++i)
                {
                    GorgonRenderTargetView view = _renderTargets[i];

                    _d3DRtvs[i] = _renderTargets[i]?.Native;

                    if (view != null)
                    {
                        _renderTargets[i].Resource.BindState = BindState.Rtv;
                    }
                }
            }

            D3DDeviceContext.OutputMerger.SetTargets(DepthStencilView?.Native, rtvCount, _d3DRtvs);

            if (DepthStencilView != null)
            {
                DepthStencilView.Texture.BindState = BindState.Dsv;
            }

            _isTargetUpdated = (false, false);
        }

        /// <summary>
        /// Function to check for the minimum windows 10 build that Gorgon Graphics supports.
        /// </summary>
        internal static void CheckMinimumOperatingSystem()
        {
            if (!Win32API.IsWindows10OrGreater(MinWin10Build))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_OS, MinWin10Build));
            }
        }

        /// <summary>
        /// Function to build up a <see cref="GorgonSamplerState"/> object with its corresponding Direct3D 11 state object by either creating a new object, or inheriting a previous one.
        /// </summary>
        /// <param name="newState">The new state to initialize.</param>
        /// <returns>If the state matches a cached state, then the cached state is returned, otherwise a copy of the the <paramref name="newState"/> is returned.</returns>
        internal GorgonSamplerState CacheSamplerState(GorgonSamplerState newState)
        {
            lock (_samplerLock)
            {
                if (newState.ID != int.MinValue)
                {
                    return _cachedSamplers[newState.ID];
                }

                for (int i = 0; i < _cachedSamplers.Count; ++i)
                {
                    GorgonSamplerState cached = _cachedSamplers[i];

                    if (cached?.Equals(newState) ?? false)
                    {
                        return cached;
                    }
                }

                // We didn't find what we wanted, so create a new one.
                var resultState = new GorgonSamplerState(newState)
                                  {
                                      ID = _cachedSamplers.Count
                                  };
                resultState.BuildD3D11SamplerState(D3DDevice);
                _cachedSamplers.Add(resultState);
                
                return resultState;
            }
        }

        /// <summary>
        /// Function to build up a <see cref="GorgonPipelineState"/> object with Direct 3D 11 state objects by either creating new objects, or inheriting previous ones.
        /// </summary>
        /// <param name="newState">The new state to initialize.</param>
        /// <returns>If the pipeline state matches a cached pipeline state, then the cached state is returned, otherwise a copy of the <paramref name="newState"/> is returned.</returns>
        internal GorgonPipelineState CachePipelineState(GorgonPipelineState newState)
        {
            // Existing states.
            D3D11.DepthStencilState depthStencilState = null;
            D3D11.BlendState1 blendState = null;
            D3D11.RasterizerState1 rasterState = null;

            lock (_stateLock)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < _cachedPipelineStates.Count; ++i)
                {
                    int blendStateEqualCount = 0;
                    DrawCallChanges inheritedState = DrawCallChanges.None;
                    GorgonPipelineState cachedState = _cachedPipelineStates[i];

                    if (cachedState.PrimitiveType == newState.PrimitiveType)
                    {
                        inheritedState |= DrawCallChanges.Topology;
                    }

                    if (cachedState.VertexShader == newState.VertexShader)
                    {
                        inheritedState |= DrawCallChanges.VertexShader;
                    }

                    if (cachedState.PixelShader == newState.PixelShader)
                    {
                        inheritedState |= DrawCallChanges.PixelShader;
                    }

                    if (cachedState.GeometryShader == newState.GeometryShader)
                    {
                        inheritedState |= DrawCallChanges.GeometryShader;
                    }
    
                    if (cachedState.DomainShader == newState.DomainShader)
                    {
                        inheritedState |= DrawCallChanges.DomainShader;
                    }

                    if (cachedState.HullShader == newState.HullShader)
                    {
                        inheritedState |= DrawCallChanges.HullShader;
                    }

                    if (cachedState.ComputeShader == newState.ComputeShader)
                    {
                        inheritedState |= DrawCallChanges.PixelShader;
                    }
                    
                    if (cachedState.RasterState.Equals(newState.RasterState))
                    {
                        rasterState = cachedState.D3DRasterState;
                        inheritedState |= DrawCallChanges.RasterState;
                    }
                    
                    /*
                    if ((cachedStateInfo.DepthStencilState != null) &&
                        (cachedStateInfo.DepthStencilState.Equals(newStateInfo.DepthStencilState)))
                    {
                        depthStencilState = cachedState.D3DDepthStencilState;
                        inheritedState |= PipelineStateChange.DepthStencilState;
                    }
    
                    if (ReferenceEquals(newStateInfo.BlendStates, cachedStateInfo.BlendStates))
                    {
                        blendState = cachedState.D3DBlendState;
                        inheritedState |= PipelineStateChange.BlendState;
                    }
                    else
                    {
                        if ((newStateInfo.BlendStates != null)
                            && (cachedStateInfo.BlendStates != null)
                            && (newStateInfo.BlendStates.Count == cachedStateInfo.BlendStates.Count))
                        {
                            for (int j = 0; j < newStateInfo.BlendStates.Count; ++j)
                            {
                                if (cachedStateInfo.BlendStates[j]?.Equals(newStateInfo.BlendStates[j]) ?? false)
                                {
                                    blendStateEqualCount++;
                                }
                            }
    
                            if (blendStateEqualCount == newStateInfo.BlendStates.Count)
                            {
                                blendState = cachedState.D3DBlendState;
                                inheritedState |= PipelineStateChange.BlendState;
                            }
                        }
                    }
                    */
                    // We've copied all the states, so just return the existing pipeline state.
                    // ReSharper disable once InvertIf
                    if (inheritedState == DrawCallChanges.AllPipelineState)
                    {
                        return cachedState;
                    }
                }

                // Setup any uninitialized states.
                var resultState = new GorgonPipelineState(newState);
                InitializePipelineState(resultState, blendState, depthStencilState, rasterState);
                resultState.ID = _cachedPipelineStates.Count;
                _cachedPipelineStates.Add(resultState);
                return resultState;
            }
        }

        /// <summary>
        /// Function to clear the states for the graphics object.
        /// </summary>
        /// <param name="flush">[Optional] <b>true</b> to flush the queued graphics object commands, <b>false</b> to leave as is.</param>
        /// <remarks>
        /// <para>
        /// This method will reset all current states to an uninitialized state.
        /// </para>
        /// <para>
        /// If the <paramref name="flush"/> parameter is set to <b>true</b>, then any commands on the GPU that are pending will be flushed.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// This method will cause a significant performance hit if the <paramref name="flush"/> parameter is set to <b>true</b>, so its use is generally discouraged in performance sensitive situations.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void ClearState(bool flush = false)
        {
            // Reset state on the device context.
            D3DDeviceContext.ClearState();

            if (flush)
            {
                D3DDeviceContext.Flush();
            }

            Array.Clear(_renderTargets, 0, _renderTargets.Length);
            Array.Clear(_viewports, 0, _viewports.Length);
            Array.Clear(_vertexBindingCache, 0, _vertexBindingCache.Length);
            Array.Clear(_scissors, 0, _scissors.Length);
            
            _lastState.VertexBuffers = null;
            _lastState.IndexBuffer = null;
            _lastState.PipelineState.Clear();
            _lastState.PsSamplers = null;
            _lastState.VsSamplers = null;
            _lastState.GsSamplers = null;
            _lastState.DsSamplers = null;
            _lastState.HsSamplers = null;
            _lastState.CsSamplers = null;
            _lastState.VsConstantBuffers = null;
            _lastState.PsConstantBuffers = null;
            _lastState.GsConstantBuffers = null;
            _lastState.DsConstantBuffers = null;
            _lastState.HsConstantBuffers = null;
            _lastState.CsConstantBuffers = null;
        }

        /// <summary>
        /// Function to assign a single render target to the first slot.
        /// </summary>
        /// <param name="renderTarget">The render target view to assign.</param>
        /// <param name="depthStencil">[Optional] The depth/stencil to assign with the render target.</param>
        /// <remarks>
        /// <para>
        /// This will assign a render target in slot 0 of the <see cref="RenderTargets"/> list. All other render targets bound in other slots will be unbound. If multiple render targets need to be set,
        /// then call the <see cref="SetRenderTargets"/> method.
        /// </para>
        /// <para>
        /// If the <paramref name="depthStencil"/> parameter is used, then a <see cref="GorgonDepthStencil2DView"/> is assigned in conjunction with the render target. This depth/stencil have the same 
        /// dimensions, array size, and multisample values as the render target. When specifying a depth/stencil, the render targets must be a <see cref="GorgonTexture2D"/>.
        /// </para>
        /// <para>
        /// When a render target is set, the first viewport in the <see cref="Viewports"/> list will be reset to the size of the render target. The user is responsible for restoring these to their intended
        /// values after assigning the target if a different viewport region is required.
        /// </para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </remarks>
        /// <seealso cref="GorgonDepthStencil2DView"/>
        /// <seealso cref="GorgonTexture2D"/>
        public void SetRenderTarget(GorgonRenderTargetView renderTarget, GorgonDepthStencil2DView depthStencil = null)
        {
            if ((_renderTargets[0] == renderTarget) && (depthStencil == DepthStencilView))
            {
                return;
            }

            _isTargetUpdated = (_renderTargets[0] != renderTarget, DepthStencilView != depthStencil);

            if (_isTargetUpdated.RtvsChanged)
            {
                ClearRenderTargetBindings(1, _renderTargets.Length);
                _renderTargets[0] = renderTarget;

                DX.ViewportF viewport = default;
                if (_renderTargets[0] != null)
                {
                    viewport = new DX.ViewportF(0, 0, _renderTargets[0].Width, _renderTargets[0].Height);
                }
                SetViewport(ref viewport);
            }

            DepthStencilView = depthStencil;
            SetRenderTargetAndDepthViews(1);
        }

        /// <summary>
        /// Function to assign multiple render targets to the first slot and a custom depth/stencil view.
        /// </summary>
        /// <param name="renderTargets">The list of render target views to assign.</param>
        /// <param name="depthStencil">The depth/stencil view to assign.</param>
        /// <remarks>
        /// <para>
        /// This will assign multiple render targets to the corresponding slots in the <see cref="RenderTargets"/> list. 
        /// </para>
        /// <para>
        /// If the <paramref name="depthStencil"/> parameter is used, then a <see cref="GorgonDepthStencil2DView"/> is assigned in conjunction with the render target. This depth/stencil have the same 
        /// dimensions, array size, and multisample values as the render target. When specifying a depth/stencil, the render targets must be a <see cref="GorgonTexture2D"/>.
        /// </para>
        /// <para>
        /// If the <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> are attached to resources with multisampling enabled through <see cref="GorgonMultisampleInfo"/>, then the 
        /// <see cref="GorgonMultisampleInfo"/> of the resource attached to the <see cref="GorgonDepthStencil2DView"/> being assigned must match, or an exception will be thrown.
        /// </para>
        /// <para>
        /// The format for the <paramref name="renderTargets"/> and <paramref name="depthStencil"/> may differ from the formats of other views passed in.
        /// </para>
        /// <para>
        /// When a render target is set, the first viewport in the <see cref="Viewports"/> list will be reset to the size of the render target. The user is responsible for restoring these to their intended
        /// values after assigning the target if a different viewport region is required.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// The exceptions raised when validating a view against other views in this list are only thrown when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonDepthStencil2DView"/>
        /// <seealso cref="GorgonTexture2D"/>
        public void SetRenderTargets(GorgonRenderTargetView[] renderTargets, GorgonDepthStencil2DView depthStencil = null)
        {
            if ((renderTargets == null)
                || (renderTargets.Length == 0))
            {
                ClearRenderTargetBindings(0, _renderTargets.Length);

                if (DepthStencilView != null)
                {
                    DepthStencilView.Texture.BindState = BindState.None;
                }

                DepthStencilView = depthStencil;
                SetRenderTargetAndDepthViews(0);
                return;
            }

            int rtvCount = renderTargets.Length.Min(_renderTargets.Length);

            for (int i = 0; i < rtvCount; ++i)
            {
                ref GorgonRenderTargetView view = ref _renderTargets[i];

                if (view == renderTargets[i])
                {
                    continue;
                }

                if (view != null)
                {
                    view.Resource.BindState = BindState.None;
                }

                view = renderTargets[i];
                _isTargetUpdated = (true, depthStencil != DepthStencilView);
            }

            if ((!_isTargetUpdated.DepthViewChanged) && (!_isTargetUpdated.RtvsChanged))
            {
                return;
            }

            if (_isTargetUpdated.RtvsChanged)
            {
                if (rtvCount < _renderTargets.Length)
                {
                    ClearRenderTargetBindings(rtvCount, _renderTargets.Length);
                }

                DX.ViewportF viewport = default;

                if (_renderTargets[0] != null)
                {
                    viewport = new DX.ViewportF(0, 0, renderTargets[0].Width, renderTargets[0].Height);
                }

                SetViewport(ref viewport);
            }

            if (DepthStencilView != null)
            {
                DepthStencilView.Resource.BindState = BindState.None;
            }

            DepthStencilView = depthStencil;
            SetRenderTargetAndDepthViews(rtvCount);
        }

        /// <summary>
        /// Function to set a viewport to define the area to render on the <see cref="RenderTargets"/>.
        /// </summary>
        /// <param name="viewport">The viewport to assign.</param>
        /// <remarks>
        /// <para>
        /// This will define the area to render into on the current <see cref="RenderTargets"/>. This method will set the first viewport at index 0 only, any other viewports assigned will be unassigned.
        /// </para>
        /// </remarks>
        public void SetViewport(ref DX.ViewportF viewport)
        {
            ref DX.ViewportF firstViewport = ref _viewports[0];

            if (firstViewport.Equals(ref viewport))
            {
                return;
            }

            _viewports[0] = viewport;
            Array.Clear(_viewports, 1, _viewports.Length - 1);
            D3DDeviceContext.Rasterizer.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth);
        }

        /// <summary>
        /// Function to set multiple viewports to define the area to render on the <see cref="RenderTargets"/>.
        /// </summary>
        /// <param name="viewports">The viewports to assign.</param>
        /// <remarks>
        /// <para>
        /// This will define the area to render into on the current <see cref="RenderTargets"/>. This method will set the first viewport at index 0 only, any other viewports assigned will be unassigned.
        /// </para>
        /// </remarks>
        public void SetViewports(DX.ViewportF[] viewports)
        {
            if (viewports == null)
            {
                Array.Clear(_viewports, 0, _viewports.Length);
                D3DDeviceContext.Rasterizer.SetViewport(0, 0, 1, 1);
                return;
            }

            bool isChanged = false;
            int viewportCount = viewports.Length.Min(_viewports.Length);

            unsafe
            {
                RawViewportF* viewportPtr = stackalloc RawViewportF[viewportCount];

                if (viewportCount < _viewports.Length)
                {
                    Array.Clear(_viewports, viewportCount, _viewports.Length - viewportCount);
                }

                for (int i = 0; i < viewportCount; ++i)
                {
                    ref DX.ViewportF cachedViewport = ref _viewports[i];
                    ref DX.ViewportF newViewport = ref viewports[i];

                    if (cachedViewport.Equals(ref newViewport))
                    {
                        continue;
                    }

                    viewportPtr[i] = cachedViewport = newViewport;
                    isChanged = true;
                }

                if (!isChanged)
                {
                    return;
                }

                D3DDeviceContext.Rasterizer.SetViewports(viewportPtr, viewportCount);
            }
        }

        /// <summary>
        /// Function to retrieve information about the installed video adapters on the system.
        /// </summary>
        /// <param name="includeSoftwareDevice">[Optional] <b>true</b> to retrieve a software rendering device, or <b>false</b> to exclude it.</param>
        /// <param name="log">[Optional] The logging interface used to capture debug messages.</param>
        /// <returns>A list of installed adapters on the system.</returns>
        /// <remarks>
        /// <para>
        /// Use this to retrieve a list of video adapters available on the system. A video adapter may be a discreet video card, a device on the motherboard, or a software video adapter.
        /// </para>
        /// <para>
        /// This resulting list will contain <see cref="VideoAdapterInfo"/> objects which can then be passed to a <see cref="GorgonGraphics"/> instance. This allows applications or users to pick and choose which 
        /// adapter they wish to use for rendering.
        /// </para>
        /// <para>
        /// If the user specifies <b>true</b> for the <paramref name="includeSoftwareDevice"/> parameter, then the video adapter supplied will be much slower than an actual hardware video adapter. However, 
        /// this adapter can be helpful in debugging scenarios where issues with the hardware device driver may be causing incorrect rendering.
        /// </para>
        /// </remarks>
        public static IReadOnlyList<IGorgonVideoAdapterInfo> EnumerateAdapters(bool includeSoftwareDevice = false, IGorgonLog log = null) => VideoAdapterEnumerator.Enumerate(includeSoftwareDevice, log);

        /// <summary>
        /// Function to clear the cached pipeline states.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will destroy any previously cached pipeline states.
        /// </para>
        /// </remarks>
        public void ClearStateCache()
        {
            if (D3DDeviceContext != null)
            {
                ClearState();
            }

            lock (_samplerLock)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < _cachedSamplers.Count; ++i)
                {
                    _cachedSamplers[i].ID = int.MinValue;
                    _cachedSamplers[i].Native?.Dispose();
                }

                _cachedSamplers.Clear();
            }

            lock (_stateLock)
            {
                // Wipe out the state cache.
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < _cachedPipelineStates.Count; ++i)
                {
                    _cachedPipelineStates[i].ID = int.MinValue;
                    _cachedPipelineStates[i].D3DRasterState?.Dispose();
                    _cachedPipelineStates[i].D3DDepthStencilState?.Dispose();
                    _cachedPipelineStates[i].D3DBlendState?.Dispose();
                }

                _cachedPipelineStates.Clear();
            }
        }

        /// <summary>
        /// Function to submit a basic draw call to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        public void Submit(GorgonDrawCall drawCall)
        {
            drawCall.ValidateObject(nameof(drawCall));

            DrawCallChanges changes = BuildDrawCallResources(drawCall.D3DState);

            BindResources(changes);
            
            D3DDeviceContext.Draw(drawCall.VertexCount, drawCall.VertexStartIndex);
        }

        /// <summary>
        /// Function to submit a draw call with indices to the GPU.
        /// </summary>
        /// <param name="drawIndexCall">The draw call to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawIndexCall"/> parameter is <b>null</b>.</exception>
        public void Submit(GorgonDrawIndexCall drawIndexCall)
        {
            drawIndexCall.ValidateObject(nameof(drawIndexCall));

            DrawCallChanges changes = BuildDrawCallResources(drawIndexCall.D3DState);
            DrawCallChanges stateChanges = BuildStateChanges(drawIndexCall.D3DState.PipelineState);

            BindResources(changes);
            ApplyState(_lastState.PipelineState, stateChanges);

            D3DDeviceContext.DrawIndexed(drawIndexCall.IndexCount, drawIndexCall.IndexStart, drawIndexCall.BaseVertexIndex);
        }

        #region Crap Code.
        private D3D11.DepthStencilState dState;
        private D3D11.BlendState1 bState;

        [Obsolete("This method is here to set up native functionality not yet available to the core library.")]
        private void DoDispose()
        {
            dState?.Dispose();
            bState?.Dispose();
        }

        [Obsolete("This method is here to set up native functionality not yet available to the core library.")]
        public void DoInit()
        {
            dState = new D3D11.DepthStencilState(D3DDevice,
                                                 new D3D11.DepthStencilStateDescription
                                                 {
                                                     IsDepthEnabled = false,
                                                     DepthWriteMask = D3D11.DepthWriteMask.All,
                                                     DepthComparison = D3D11.Comparison.Less,
                                                     IsStencilEnabled = false,
                                                     StencilReadMask = 0xff,
                                                     StencilWriteMask = 0xff,
                                                     FrontFace = new D3D11.DepthStencilOperationDescription
                                                                 {
                                                                     Comparison = D3D11.Comparison.Always,
                                                                     DepthFailOperation = D3D11.StencilOperation.Keep,
                                                                     FailOperation = D3D11.StencilOperation.Keep,
                                                                     PassOperation = D3D11.StencilOperation.Keep
                                                                 },
                                                     BackFace = new D3D11.DepthStencilOperationDescription
                                                                {
                                                                    Comparison = D3D11.Comparison.Always,
                                                                    DepthFailOperation = D3D11.StencilOperation.Keep,
                                                                    PassOperation = D3D11.StencilOperation.Keep,
                                                                    FailOperation = D3D11.StencilOperation.Keep
                                                                }
                                                 });

            bState = new D3D11.BlendState1(D3DDevice,
                                           new D3D11.BlendStateDescription1
                                           {
                                               AlphaToCoverageEnable = false,
                                               IndependentBlendEnable = false,
                                               RenderTarget =
                                               {
                                                   [0] = new D3D11.RenderTargetBlendDescription1
                                                         {
                                                             IsBlendEnabled = true,
                                                             IsLogicOperationEnabled = false,
                                                             SourceBlend = D3D11.BlendOption.SourceAlpha,
                                                             DestinationBlend = D3D11.BlendOption.InverseSourceAlpha,
                                                             AlphaBlendOperation = D3D11.BlendOperation.Add,
                                                             BlendOperation = D3D11.BlendOperation.Add,
                                                             SourceAlphaBlend = D3D11.BlendOption.One,
                                                             DestinationAlphaBlend = D3D11.BlendOption.Zero,
                                                             LogicOperation = D3D11.LogicOperation.Noop,
                                                             RenderTargetWriteMask = D3D11.ColorWriteMaskFlags.All
                                                         }
                                               }
                                           });
        }

        private bool _stateSet;

        [Obsolete("This method is here to set up native functionality not yet available to the core library.")]
        public void DoStuff()
        {
            if (!_stateSet)
            {
                D3DDeviceContext.OutputMerger.DepthStencilState = dState;
                D3DDeviceContext.OutputMerger.BlendState = bState;

                _stateSet = true;
            }
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DoDispose();
            D3D11.DeviceContext4 context = Interlocked.Exchange(ref _d3DDeviceContext, null);
            D3D11.Device5 device = Interlocked.Exchange(ref _d3DDevice, null);
            DXGI.Adapter4 adapter = Interlocked.Exchange(ref _dxgiAdapter, null);
            DXGI.Factory5 factory = Interlocked.Exchange(ref _dxgiFactory, null);

            // If these are all gone, then we've already disposed.
            if ((factory == null)
                && (adapter == null)
                && (device == null)
                && (context == null))
            {
                return;
            }

            ClearStateCache();

            // Dispose all objects created from this interface.
            this.DisposeAll();

            // Disconnect from the context.
            Log.Print($"Destroying GorgonGraphics interface for device '{VideoAdapter.Name}'...", LoggingLevel.Simple);

            // Reset the state for the context. This will ensure we don't have anything bound to the pipeline when we shut down.
            context?.ClearState();
            context?.Dispose();
            device?.Dispose();
            adapter?.Dispose();
            factory?.Dispose();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
        /// </summary>
        /// <param name="videoAdapterInfo">A <see cref="IGorgonVideoAdapterInfo"/> to specify the video adapter to use for this instance.</param>
        /// <param name="featureSet">[Optional] The requested feature set for the video adapter used with this object.</param>
        /// <param name="log">[Optional] The log to use for debugging.</param>
        /// <exception cref="GorgonException">Thrown when the <paramref name="featureSet"/> is unsupported.</exception>
        /// <remarks>
        /// <para>
        /// When the <paramref name="videoAdapterInfo"/> is set to <b>null</b>, Gorgon will use the first video adapter with feature level specified by <paramref name="featureSet"/>  
        /// will be used. If the feature level requested is higher than what any device in the system can support, then the first device with the highest feature level will be used.
        /// </para>
        /// <para>
        /// When specifying a feature set, the device with the closest matching feature set will be used. If the <paramref name="videoAdapterInfo"/> is specified, then that device will be used at the 
        /// requested <paramref name="featureSet"/>. If the requested <paramref name="featureSet"/> is higher than what the <paramref name="videoAdapterInfo"/> will support, then Gorgon will use the 
        /// highest feature of the specified <paramref name="videoAdapterInfo"/>. 
        /// </para>
        /// <para>
        /// If Gorgon is compiled in DEBUG mode, and <see cref="VideoAdapterInfo"/> is <b>null</b>, then it will attempt to find the most appropriate hardware video adapter, and failing that, will fall 
        /// back to a software device.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// The Gorgon Graphics library only works on Windows 10 v1703 Build 15603 (Creators Update) or better. No lesser operating system version is supported.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// The following examples show the various ways the object can be configured:
        /// </para>
        /// <code lang="csharp">
        /// <![CDATA[
        /// // Create using a specific video adapter and use the highest feature set supported by that device:
        /// // Get a list of available video adapters.
        /// IReadOnlyList<IGorgonVideoAdapterInfo> videoAdapters = GorgonGraphics.EnumerateAdapters(false, log);
        ///
        /// // In real code, you should always check for more than 0 devices in the resulting list.
        /// GorgonGraphics graphics = GorgonGraphics.Create(videoAdapters[0]);
        /// 
        /// // Create using the requested feature set and the first adapter that supports the nearest feature set requested:
        /// // If the device does not support 12.1, then the device with the nearest feature set (e.g. 12.0) will be used instead.
        /// GorgonGraphics graphics = GorgonGraphics.Create(videoAdapters[0], FeatureSet.Level_12_1);
        /// 
        /// // Create using the requested device and the requested feature set:
        /// // If the device does not support 12.0, then the highest feature set supported by the device will be used (e.g. 10.1).
        /// IReadOnlyList<IGorgonVideoAdapterInfo> videoAdapters = GorgonGraphics.EnumerateAdapters(false, log);
        ///
        /// GorgonGraphics graphics = GorgonGraphics.Create(videoAdapters[0], FeatureSet.Level_12_0); 
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="IGorgonVideoAdapterInfo"/>
        public GorgonGraphics(IGorgonVideoAdapterInfo videoAdapterInfo,
                               FeatureSet? featureSet = null,
                               IGorgonLog log = null)
        {
            VideoAdapter = videoAdapterInfo ?? throw new ArgumentNullException(nameof(videoAdapterInfo));
            Log = log ?? GorgonLog.NullLog;

            // If we've not specified a feature level, or the feature level exceeds the requested device feature level, then 
            // fall back to the device feature level.
            if ((featureSet == null) || (videoAdapterInfo.FeatureSet < featureSet.Value))
            {
                featureSet = videoAdapterInfo.FeatureSet;
            }

            // We only support feature set 12 and greater.
            if (!Enum.IsDefined(typeof(FeatureSet), featureSet.Value))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, featureSet));
            }

            FeatureSet = featureSet.Value;

            Log.Print("Gorgon Graphics initializing...", LoggingLevel.Simple);
            Log.Print($"Using video adapter '{videoAdapterInfo.Name}' at feature set [{featureSet.Value}] for Direct 3D 11.4.", LoggingLevel.Simple);

            // Build up the required device objects to pass in to the constructor.
            (D3D11.Device5 device, DXGI.Factory5 factory, DXGI.Adapter4 adapter) = CreateDevice(videoAdapterInfo, (D3D.FeatureLevel)featureSet.Value);
            _dxgiFactory = factory;
            _dxgiAdapter = adapter;
            _d3DDevice = device;
            _d3DDeviceContext = device.ImmediateContext.QueryInterface<D3D11.DeviceContext4>();
            
            FormatSupport = EnumerateFormatSupport(_d3DDevice);
            
            Log.Print("Gorgon Graphics initialized.", LoggingLevel.Simple);
        }

        /// <summary>
        /// Initializes the <see cref="GorgonGraphics"/> class.
        /// </summary>
        static GorgonGraphics()
        {
            CheckMinimumOperatingSystem();

            DX.Configuration.ThrowOnShaderCompileError = false;

#if DEBUG
            IsDebugEnabled = true;
#endif
        }
        #endregion
    }
}