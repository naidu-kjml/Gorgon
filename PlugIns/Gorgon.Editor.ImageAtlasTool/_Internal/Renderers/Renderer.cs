﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 24, 2019 10:14:49 PM
// 
#endregion

using Gorgon.Editor.Rendering;
using Gorgon.Editor.ImageAtlasTool.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ImageAtlasTool
{
    /// <summary>
    /// The renderer used to draw the texture and sprites.
    /// </summary>
    internal class Renderer
		: DefaultToolRenderer<IImageAtlas>
	{
		#region Variables.
		// The camera used to render.
		private IGorgon2DCamera _camera;
		// The texture to display.
		private GorgonTexture2DView _texture;
		// The sprite used to display the texture.
		private GorgonSprite _textureSprite;
		#endregion

		#region Methods.
		/// <summary>
        /// Function to build a texture for the current image.
        /// </summary>
		private void GetTexture()
		{
			_texture?.Dispose();

			if (DataContext?.CurrentImage.image == null)
			{
				return;
			}

			_texture = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo("ImagePreview")
			{
				Width = DataContext.CurrentImage.image.Width,
				Height = DataContext.CurrentImage.image.Height,
				Format = DataContext.CurrentImage.image.Format,
				IsCubeMap = false,
				Usage = ResourceUsage.Immutable,
				Binding = TextureBinding.ShaderResource
			}, DataContext.CurrentImage.image);
		}

		/// <summary>
		/// Function to draw a message on the screen when no atlas is loaded.
		/// </summary>
		private void DrawMessage()
		{
			DX.Size2F textSize = Renderer.DefaultFont.MeasureText(Resources.GORIAG_TEXT_NO_ATLAS, false);

			Renderer.Begin(camera: _camera);
			Renderer.DrawFilledRectangle(new DX.RectangleF(-MainRenderTarget.Width * 0.5f, -MainRenderTarget.Height * 0.5f, MainRenderTarget.Width, MainRenderTarget.Height), new GorgonColor(GorgonColor.White, 0.75f));
			Renderer.DrawString(Resources.GORIAG_TEXT_NO_ATLAS, new DX.Vector2((int)(-textSize.Width * 0.5f), (int)(-textSize.Height * 0.5f)), color: GorgonColor.Black);			
			Renderer.End();
		}

		/// <summary>
        /// Function to draw the selected image.
        /// </summary>
		private void DrawImage()
		{
			string text = DataContext.CurrentImage.file?.Name ?? string.Empty;
			DX.Size2F textSize = Renderer.DefaultFont.MeasureText(text, false);

			float scale = CalculateScaling(new DX.Size2F(_texture.Width + 8, _texture.Height + 8), new DX.Size2F(MainRenderTarget.Width, MainRenderTarget.Height));
            DX.Size2F size = new DX.Size2F(scale * _texture.Width, scale * _texture.Height).Truncate();
			DX.Vector2 position = new DX.Vector2(-size.Width * 0.5f, -size.Height * 0.5f).Truncate();

			Renderer.Begin(camera: _camera);
			Renderer.DrawFilledRectangle(new DX.RectangleF(position.X, position.Y, size.Width, size.Height),
										 GorgonColor.White,
										 _texture,
										 new DX.RectangleF(0, 0, 1, 1),
										 textureSampler: GorgonSamplerState.PointFiltering);
			Renderer.End();

			Renderer.Begin();
			Renderer.DrawFilledRectangle(new DX.RectangleF(0, ClientSize.Height - textSize.Height - 2, ClientSize.Width, textSize.Height + 4),
										 new GorgonColor(GorgonColor.Black, 0.80f));
			Renderer.DrawString(text, new DX.Vector2(ClientSize.Width * 0.5f - textSize.Width * 0.5f, ClientSize.Height - textSize.Height - 2), color: GorgonColor.White);
			Renderer.End();
		}

		/// <summary>Function called when a property on the <see cref="DefaultToolRenderer{T}.DataContext"/> is changing.</summary>
		/// <param name="propertyName">The name of the property that is changing.</param>
		/// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
		protected override void OnPropertyChanging(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(IImageAtlas.CurrentImage):
					_texture?.Dispose();
					_texture = null;
					break;
			}
		}

		/// <summary>Function called when a property on the <see cref="DefaultToolRenderer{T}.DataContext"/> has been changed.</summary>
		/// <param name="propertyName">The name of the property that was changed.</param>
		/// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
		protected override void OnPropertyChanged(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(IImageAtlas.CurrentImage):
					GetTexture();
					break;
			}
		}

        /// <summary>Function to render the content.</summary>
        /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
        protected override void OnRenderContent()
		{            
			OnRenderBackground();

			if ((DataContext.Atlas == null) || (DataContext.Atlas.Textures.Count == 0))
			{
				if (_texture != null)
				{
					DrawImage();
				}
				else
				{
					DrawMessage();
				}
				return;
			}

			GorgonTexture2DView texture = _textureSprite.Texture = DataContext.Atlas.Textures[DataContext.PreviewTextureIndex];

			float scale = CalculateScaling(new DX.Size2F(texture.Width + 8, texture.Height + 8), new DX.Size2F(MainRenderTarget.Width, MainRenderTarget.Height));
			_textureSprite.Size = new DX.Size2F(scale * texture.Width, scale * texture.Height).Truncate();
			_textureSprite.Position = new DX.Vector2(-_textureSprite.Size.Width * 0.5f, -_textureSprite.Size.Height * 0.5f).Truncate();

			Renderer.Begin(camera: _camera);
			_textureSprite.TextureArrayIndex = DataContext.PreviewArrayIndex;
			Renderer.DrawSprite(_textureSprite);

			Renderer.End();
		}

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_texture?.Dispose();
			}

			base.Dispose(disposing);
		}

        /// <summary>Function called when the renderer needs to load any resource data.</summary>
        /// <remarks>
        /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
        /// <see cref="DefaultToolRenderer{T}.OnUnload"/> method.
        /// </remarks>
        protected override void OnLoad() 
		{
			base.OnLoad();

			_camera = new Gorgon2DOrthoCamera(Renderer, new DX.Size2F(MainRenderTarget.Width, MainRenderTarget.Height))
			{
				Anchor = new DX.Vector2(0.5f, 0.5f)
			};

			GorgonTexture2DView texture = (DataContext.Atlas != null) && (DataContext.Atlas.Textures.Count > 0) ? DataContext.Atlas.Textures[0] : null;

			_textureSprite = new GorgonSprite
			{
				Texture = texture,
				TextureRegion = new DX.RectangleF(0, 0, 1, 1),
				Size = new DX.Size2F(texture != null ? texture.Width : 1, texture != null ? texture.Height : 1),
				TextureSampler = GorgonSamplerState.PointFiltering
			};
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>Initializes a new instance of the <see cref="Renderer"/> class.</summary>
		/// <param name="renderer">The 2D renderer for the application.</param>
		/// <param name="swapChain">The swap chain bound to the window.</param>
		/// <param name="dataContext">The data context for the renderer.</param>
		public Renderer(Gorgon2D renderer, GorgonSwapChain swapChain, IImageAtlas dataContext)
			: base("Atlas Renderer", renderer, swapChain, dataContext)
		{
		}
		#endregion
	}
}
