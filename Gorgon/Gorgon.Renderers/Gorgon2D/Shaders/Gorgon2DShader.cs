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
// Created: June 13, 2018 4:13:14 PM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Collections;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A pixel shader for use with the <see cref="Gorgon2D"/> interface.
    /// </summary>
    public sealed class Gorgon2DShader<T>
        : IDisposable
        where T : GorgonShader
    {
        #region Variables.
        /// <summary>
        /// The constant buffers, with read/write access.
        /// </summary>
        internal GorgonArray<GorgonConstantBufferView> RwConstantBuffers = new GorgonArray<GorgonConstantBufferView>(GorgonConstantBuffers.MaximumConstantBufferCount);
        /// <summary>
        /// The texture samplers, with read/write access.
        /// </summary>
        internal GorgonArray<GorgonSamplerState> RwSamplers = new GorgonArray<GorgonSamplerState>(GorgonSamplerStates.MaximumSamplerStateCount);
        /// <summary>
        /// The shader resource views, with read/write access.
        /// </summary>
        internal GorgonArray<GorgonShaderResourceView> RwSrvs = new GorgonArray<GorgonShaderResourceView>(16);

        // The basic shader.
        private T _shader;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the shader.
        /// </summary>
        public T Shader
        {
            get => _shader;
            internal set => _shader = value;
        }

        /// <summary>
        /// Property to return the samplers for the shader.
        /// </summary>
        public IGorgonReadOnlyArray<GorgonSamplerState> Samplers => RwSamplers;

        /// <summary>
        /// Property to return the constant buffers for the shader.
        /// </summary>
        public IGorgonReadOnlyArray<GorgonConstantBufferView> ConstantBuffers => RwConstantBuffers;

        /// <summary>
        /// Property to return the list of shader resources for the shader.
        /// </summary>
        public IGorgonReadOnlyArray<GorgonShaderResourceView> ShaderResources => RwSrvs;
        #endregion

        #region Methods.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            T shader = Interlocked.Exchange(ref _shader, null);
            shader?.Dispose();
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderResources"/> class.
        /// </summary>
        internal Gorgon2DShader()
        {
        }
        #endregion
    }
}