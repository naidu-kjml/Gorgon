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
// Created: Sunday, February 26, 2012 9:31:44 AM
// 
#endregion

// Portions of this code were adapted from the Vortex 2D graphics library by Alex Khomich.
// Vortex2D.Net is available from http://www.vortex2d.net/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using GorgonLibrary.Math;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// A renderable object for drawing an ellipse on the screen.
	/// </summary>
	public class GorgonEllipse
		: GorgonMoveable
	{
		#region Variables.
		private int _quality = 0;					// Quality for the ellipse rendering.
		private Vector2 _center = Vector2.Zero;		// Center point for the ellipse.
		private Vector2[] _offsets = null;			// Offsets for the ellipse points.
		private Vector2[] _points = null;			// List of points for the ellipse.
		private GorgonColor[] _colors = null;		// Colors for points.
		private Vector2[] _uv = null;				// Texture coordinates for ellipse.
		private bool _isFilled = false;				// Flag to indicate whether to draw the ellipse as filled or as an outline.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal override PrimitiveType PrimitiveType
		{
			get
			{
				if (IsFilled)
					return Graphics.PrimitiveType.TriangleList;
				else
					return Graphics.PrimitiveType.LineList;
			}
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		protected internal override int IndexCount
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		protected internal override GorgonIndexBuffer IndexBuffer
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Property to set or return whether the ellipse should be drawn as filled or as an outline.
		/// </summary>
		public bool IsFilled
		{
			get
			{
				return _isFilled;
			}
			set
			{
				if (value != _isFilled)
				{
					_isFilled = value;

					if (value)
						BaseVertexCount = VertexCount = _points.Length * 3;
					else
						BaseVertexCount = VertexCount = _points.Length * 2;

					NeedsTextureUpdate = true;
					NeedsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the quality level for the ellipse.
		/// </summary>
		/// <remarks>The quality level cannot be less than 4 or greater than 256.</remarks>
		public int Quality
		{
			get
			{
				return _quality;
			}
			set
			{
				if ((value < 4) || (value > 256))
					return;

				if (_quality != value)
				{
					_quality = value;
					BaseVertexCount = _quality * 3;
					VertexCount = _quality * 3;
					InitializeVertices(_quality * 3);
				}
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		public override GorgonColor Color
		{
			get
			{
				return _colors[0];
			}
			set
			{
				for (int i = 0; i < _colors.Length; i++)
					_colors[i] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the texture coordinates for the unfilled ellipse.
		/// </summary>
		private void UpdateUnfilledTextureCoordinates()
		{
			Vector2 scaleUV = Vector2.Zero;
			Vector2 offsetUV = Vector2.Zero;
			Vector2 scaledPos = Vector2.Zero;
			Vector2 scaledTexture = Vector2.Zero;
			int vertexIndex = 0;

			if (Texture == null)
			{
				for (int i = 0; i < Vertices.Length; i++)
					Vertices[i].UV = Vector2.Zero;
				return;
			}

			scaledPos = new Vector2(TextureOffset.X / Texture.Settings.Width, TextureOffset.Y / Texture.Settings.Height);
			scaledTexture = new Vector2(TextureRegion.Width / Texture.Settings.Width, TextureRegion.Height / Texture.Settings.Height);
			for (int i = 0; i < _offsets.Length; i++)
			{
				Vector2.Modulate(ref _offsets[i], ref scaledTexture, out offsetUV);

				if (i + 1 < _offsets.Length)
					Vector2.Modulate(ref _offsets[i + 1], ref scaledTexture, out scaleUV);
				else
					Vector2.Modulate(ref _offsets[0], ref scaledTexture, out scaleUV);

				Vector2.Add(ref scaleUV, ref scaledPos, out scaleUV);

				// Set center coordinate.
				Vertices[vertexIndex].UV = offsetUV;
				Vertices[vertexIndex + 1].UV = scaleUV;
				vertexIndex += 2;
			}
		}

		/// <summary>
		/// Function to update the texture coordinates for the filled ellipse.
		/// </summary>
		private void UpdateFilledTextureCoordinates()
		{
			// Calculate texture coordinates.
			Vector2 scaleUV = Vector2.Zero;
			Vector2 offsetUV = Vector2.Zero;
			Vector2 midPoint = Vector2.Zero;
			Vector2 scaledPos = Vector2.Zero;
			Vector2 scaledTexture = Vector2.Zero;
			int vertexIndex = 0;

			if (Texture == null)
			{
				for (int i = 0; i < Vertices.Length; i++)
					Vertices[i].UV = Vector2.Zero;
				return;
			}

			midPoint = new Vector2(TextureRegion.Width / 2.0f, TextureRegion.Height / 2.0f);
			scaledPos = new Vector2(TextureOffset.X / Texture.Settings.Width, TextureOffset.Y / Texture.Settings.Height);
			scaledTexture = new Vector2(TextureRegion.Width / Texture.Settings.Width, TextureRegion.Height / Texture.Settings.Height);
			for (int i = 0; i < _offsets.Length; i++)
			{
				Vector2.Modulate(ref _offsets[i], ref scaledTexture, out offsetUV);

				if (i + 1 < _offsets.Length)
					Vector2.Modulate(ref _offsets[i + 1], ref scaledTexture, out scaleUV);
				else
					Vector2.Modulate(ref _offsets[0], ref scaledTexture, out scaleUV);

				Vector2.Add(ref scaleUV, ref scaledPos, out scaleUV);
				
				// Set center coordinate.
				Vertices[vertexIndex].UV.X = (TextureOffset.X + midPoint.X) / Texture.Settings.Width;
				Vertices[vertexIndex].UV.Y = (TextureOffset.Y + midPoint.Y) / Texture.Settings.Height;
				Vertices[vertexIndex + 1].UV = offsetUV;
				Vertices[vertexIndex + 2].UV = scaleUV;
				vertexIndex += 3;
			}
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected override void UpdateTextureCoordinates()
		{
			if (!IsFilled)
				UpdateUnfilledTextureCoordinates();
			else
				UpdateFilledTextureCoordinates();
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected override void UpdateVertices()
		{
			// Set center point.
			_center.X = (0.5f * Size.X - Anchor.X) / 2.0f;
			_center.Y = (0.5f * Size.Y - Anchor.Y) / 2.0f;

			for (int i = 0; i < _points.Length; i++)
			{
				_points[i].X = (_offsets[i].X * Size.X - Anchor.X) / 2.0f;
				_points[i].Y = (_offsets[i].Y * Size.Y - Anchor.Y) / 2.0f;
			}
		}

		/// <summary>
		/// Function to transform the unfilled vertices.
		/// </summary>
		private void TransformUnfilled()
		{
			int vertexIndex = 0;
			for (int i = 0; i < _points.Length; i++)
			{
				Vector2 startPosition = _points[i];
				Vector2 endPosition = Vector2.Zero;
				if (i + 1 < _points.Length)
					endPosition = _points[i + 1];
				else
					endPosition = _points[0];

				if (Scale.X != 1.0f)
				{
					startPosition.X *= Scale.X;
					endPosition.X *= Scale.X;
				}

				if (Scale.Y != 1.0f)
				{
					startPosition.Y *= Scale.Y;
					endPosition.Y *= Scale.Y;
				}

				if (Angle != 0.0f)
				{
					float angle = GorgonMathUtility.Radians(Angle);		// Angle in radians.
					float cosVal = (float)System.Math.Cos(angle);		// Cached cosine.
					float sinVal = (float)System.Math.Sin(angle);		// Cached sine.

					Vertices[vertexIndex].Position.X = (startPosition.X * cosVal - startPosition.Y * sinVal);
					Vertices[vertexIndex].Position.Y = (startPosition.X * sinVal + startPosition.Y * cosVal);

					Vertices[vertexIndex + 1].Position.X = (endPosition.X * cosVal - endPosition.Y * sinVal);
					Vertices[vertexIndex + 1].Position.Y = (endPosition.X * sinVal + endPosition.Y * cosVal);
				}
				else
				{
					Vertices[vertexIndex].Position.X = startPosition.X;
					Vertices[vertexIndex].Position.Y = startPosition.Y;
					Vertices[vertexIndex + 1].Position.X = endPosition.X;
					Vertices[vertexIndex + 1].Position.Y = endPosition.Y;
				}

				if (Position.X != 0.0f)
				{
					Vertices[vertexIndex].Position.X += Position.X;
					Vertices[vertexIndex + 1].Position.X += Position.X;
				}

				if (Position.Y != 0.0f)
				{
					Vertices[vertexIndex].Position.Y += Position.Y;
					Vertices[vertexIndex + 1].Position.Y += Position.Y;
				}

				if (Depth != 0.0f)
				{
					Vertices[vertexIndex].Position.Z = Depth;
					Vertices[vertexIndex + 1].Position.Z = Depth;
				}

				Vertices[vertexIndex].Color = _colors[i];
				if (i + 1 < _points.Length)
					Vertices[vertexIndex + 1].Color = _colors[i + 1];
				else
					Vertices[vertexIndex + 1].Color = _colors[0];

				vertexIndex += 2;
			}
		}

		/// <summary>
		/// Function to transform the filled vertices.
		/// </summary>
		private void TransformFilled()
		{
			int vertexIndex = 0;
			Vector2 center = _center;

			for (int i = 0; i < _points.Length; i++)
			{
				Vector2 startPosition = _points[i];
				Vector2 endPosition = Vector2.Zero;
				if (i + 1 < _points.Length)
					endPosition = _points[i + 1];
				else
					endPosition = _points[0];
				
				if (Scale.X != 1.0f)
				{
					startPosition.X *= Scale.X;
					endPosition.X *= Scale.X;
					center.X *= Scale.X;
				}

				if (Scale.Y != 1.0f)
				{
					startPosition.Y *= Scale.Y;
					endPosition.Y *= Scale.Y;
					center.Y *= Scale.Y;
				}

				if (Angle != 0.0f)
				{
					float angle = GorgonMathUtility.Radians(Angle);		// Angle in radians.
					float cosVal = (float)System.Math.Cos(angle);		// Cached cosine.
					float sinVal = (float)System.Math.Sin(angle);		// Cached sine.

					Vertices[vertexIndex].Position.X = (center.X * cosVal - center.Y * sinVal);
					Vertices[vertexIndex].Position.Y = (center.X * sinVal + center.Y * cosVal);

					Vertices[vertexIndex + 1].Position.X = (startPosition.X * cosVal - startPosition.Y * sinVal);
					Vertices[vertexIndex + 1].Position.Y = (startPosition.X * sinVal + startPosition.Y * cosVal);

					Vertices[vertexIndex + 2].Position.X = (endPosition.X * cosVal - endPosition.Y * sinVal);
					Vertices[vertexIndex + 2].Position.Y = (endPosition.X * sinVal + endPosition.Y * cosVal);
				}
				else
				{
					Vertices[vertexIndex].Position.X = center.X;
					Vertices[vertexIndex].Position.Y = center.Y;
					Vertices[vertexIndex + 1].Position.X = startPosition.X;
					Vertices[vertexIndex + 1].Position.Y = startPosition.Y;
					Vertices[vertexIndex + 2].Position.X = endPosition.X;
					Vertices[vertexIndex + 2].Position.Y = endPosition.Y;
				}

				if (Position.X != 0.0f)
				{
					Vertices[vertexIndex].Position.X += Position.X;
					Vertices[vertexIndex + 1].Position.X += Position.X;
					Vertices[vertexIndex + 2].Position.X += Position.X;
				}

				if (Position.Y != 0.0f)
				{
					Vertices[vertexIndex].Position.Y += Position.Y;
					Vertices[vertexIndex + 1].Position.Y += Position.Y;
					Vertices[vertexIndex + 2].Position.Y += Position.Y;
				}

				if (Depth != 0.0f)
				{
					Vertices[vertexIndex].Position.Z = Depth;
					Vertices[vertexIndex + 1].Position.Z = Depth;
					Vertices[vertexIndex + 2].Position.Z = Depth;
				}

				Vertices[vertexIndex + 2].Color = Vertices[vertexIndex + 1].Color = Vertices[vertexIndex].Color = _colors[i];

				vertexIndex += 3;
			}
		}

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			if (!IsFilled)
				TransformUnfilled();
			else
				TransformFilled();
		}

		/// <summary>
		/// Function to set up any additional information for the renderable.
		/// </summary>
		protected override void InitializeCustomVertexInformation()
		{
			float angle = 0.0f;
			float step = GorgonMathUtility.PI * 2 / (_quality);

			_offsets = new Vector2[_quality];
			_points = new Vector2[_offsets.Length];
			_colors = new GorgonColor[_offsets.Length];
			_uv = new Vector2[_offsets.Length];

			for (int i = 0; i < _offsets.Length; i++)
			{
				_colors[i] = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
				_offsets[i] = new Vector2(GorgonMathUtility.Cos(angle), GorgonMathUtility.Sin(angle)) * 0.5f;
				_offsets[i] += new Vector2(0.5f, 0.5f);
				angle += step;
			}

			UpdateVertices();
			UpdateTextureCoordinates();

			NeedsTextureUpdate = false;
			NeedsVertexUpdate = false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEllipse"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="position">The position of the ellipse.</param>
		/// <param name="size">The size of the ellipse.</param>
		/// <param name="quality">Quality of the ellipse.</param>
		/// <param name="isFilled">TRUE if the ellipse should be filled.</param>
		internal GorgonEllipse(Gorgon2D gorgon2D, string name, Vector2 position, Vector2 size, int quality, bool isFilled)
			: base(gorgon2D, name)
		{
			TextureRegion = new System.Drawing.RectangleF(0, 0, size.X, size.Y);
			Position = position;
			Size = size;
			Quality = quality;
			IsFilled = isFilled;
		}
		#endregion
	}
}
