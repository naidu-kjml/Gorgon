﻿namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// States used by shaders when rendering a draw call.
    /// </summary>
    public interface IGorgonShaderStates
    {
        /// <summary>
        /// Property to return the constant buffers for the vertex shader.
        /// </summary>
        GorgonConstantBuffers VertexShaderConstantBuffers
        {
            get;
        }

        /// <summary>
        /// Property to return the constant buffers for the pixel shader.
        /// </summary>
        GorgonConstantBuffers PixelShaderConstantBuffers
        {
            get;
        }

        /// <summary>
        /// Property to return the vertex shader resources to bind to the pipeline.
        /// </summary>
        GorgonShaderResourceViews VertexShaderResourceViews
        {
            get;
        }

        /// <summary>
        /// Property to set or return the list of pixel shader resource views.
        /// </summary>
        GorgonShaderResourceViews PixelShaderResourceViews
        {
            get;
        }

        /// <summary>
        /// Property to return the pixel shader samplers to bind to the pipeline.
        /// </summary>
        GorgonSamplerStates PixelShaderSamplers
        {
            get;
        }

        /// <summary>
        /// Property to return the vertex shader samplers to bind to the pipeline.
        /// </summary>
        GorgonSamplerStates VertexShaderSamplers
        {
            get;
        }

        /// <summary>
        /// Property to set or return the current pipeline state.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, then the previous state will remain set.
        /// </remarks>
        GorgonPipelineState PipelineState
        {
            get;
            set;
        }
    }
}