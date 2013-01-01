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
// Created: Sunday, December 30, 2012 9:40:28 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Graphics.Example
{
	/// <summary>
	/// A vertex for our boinger objects.
	/// </summary>
	public struct BoingerVertex
	{
		/// <summary>
		/// Property to return the size of the vertex, in bytes.
		/// </summary>
		public static int Size
		{
			get
			{
				return GorgonLibrary.Native.DirectAccess.SizeOf<BoingerVertex>();
			}
		}

		/// <summary>
		/// Vertex position.
		/// </summary>
		[InputElement(0, "SV_POSITION")]
		public Vector4 Position;
		/// <summary>
		/// Texture coordinate.
		/// </summary>
		[InputElement(1, "TEXCOORD")]
		public Vector2 UV;

		/// <summary>
		/// Initializes a new instance of the <see cref="BoingerVertex" /> struct.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="uv">The texture coordinate.</param>
		public BoingerVertex(Vector3 position, Vector2 uv)
		{
			Position = new Vector4(position, 1.0f);
			UV = uv;
		}
	}

	/// <summary>
	/// This is an example of using the base graphics API.  It's very similar to how Direct 3D 11 works, but with some enhancements
	/// to deal with poor error support and other "gotchas" that tend to pop up.  It also has some time saving functionality to
	/// deal with mundane tasks like setting up a swap chain, pixel shaders, etc...
	/// 
	/// This example is a recreation of the Amiga "Boing" demo (http://www.youtube.com/watch?v=-ga41edXw3A).
	/// 
	/// 
	/// Before I go any further: This example is NOT, and I stress this "NOT!" a good example of how to write a 3D application.  
	/// A good 3D renderer is a monster to write, this example just shows a user the flexibility of Gorgon and that it's capable 
	/// of rendering 3D with the lower API level.  Any funky 3D only tricks or complicated scene graph mechanisms that you might 
	/// expect are up to the developer to figure out and write.
	/// 
	/// Anyway, on with the show....
	/// 
	/// In this example we create a swap chain, and set up the application for 3D rendering andbuild 2 types of objects:  
	/// A plane (2 of them, 1 for the floor and another for the rear wall), and a sphere.  
	/// 
	/// Once the initialization is done, we render the objects.  We transform the sphere using a world matrix for its rotation,
	/// translation and scaling.  Note that there's a shadow under the sphere, this is just the same sphere drawn again without
	/// a texture and a diffuse hardcoded shader (see shader.hlsl).  To get the shadow in there, we turn off depth-writing, which
	/// enables us to render the shadow without it interferring with any geometry but still respecting the depth buffer.
	/// 
	/// One thing to note is the use of the 2D renderer for drawing text.  I had 2 options here:
	/// 1. Draw the text manually myself.  And, there was no way in hell I was doing that.
	/// 2. Use the 2D renderer.
	/// 
	/// You'll note that we call _2D.End2D(); after we create the 2D interface.  This is so we can disable the 2D stuff up front or
	/// it may interfere with our 3D stuff.  And in the render loop, before we render the text, we call _2D.Begin2D().  This takes
	/// a copy of the current state and then overwrites that state with the 2D stuff and then we render the text.  Finally, we call
	/// _2D.End2D() and that restores the previous state (the states are stored in a stack in a LIFO order) so the 3D renderer
	/// doesn't get all messed up.  When mixing the renderers like this, it's crucial to ensure that state doesn't get clobbered
	/// or else things won't show up properly.
	/// 
	/// This example is considered advanced, and a firm understanding of a graphics API like Direct 3D 11 is recommended.
	/// It's also very "low level", in that there's not a whole lot that's done for you by the API.  It's a very manual process to 
	/// get everything initialized and thus there's a lot of set up code.  This is unlike the 2D renderer, which takes very little
	/// effort to get up and running (technically, you barely have to touch the base graphics library to get the 2D renderer doing
	/// something useful).
	/// </summary>
	static class Program
	{
		#region Variables.
		private static formMain _mainForm = null;						// Main application form.
		private static GorgonGraphics _graphics = null;					// Graphics interface.		
		private static GorgonSwapChain _swap = null;					// Our primary swap chain.
		private static GorgonVertexShader _vertexShader = null;			// Our primary vertex shader.
		private static GorgonPixelShader _pixelShader = null;			// Our primary pixel shader.	
		private static GorgonPixelShader _pixelShaderDiffuse = null;	// Our diffuse only pixel shader.	
		private static GorgonInputLayout _inputLayout = null;			// Input layout.
		private static GorgonTexture2D _texture = null;					// Our texture.
		private static GorgonConstantBuffer _wvpBuffer = null;			// Our world/view/project matrix buffer.
		private static GorgonDataStream _wvpBufferStream = null;		// Stream to our buffer.
		private static Gorgon2D _2D = null;								// 2D interface.
		private static Matrix _viewMatrix = Matrix.Identity;			// Our view matrix.
		private static Matrix _projMatrix = Matrix.Identity;			// Our projection matrix.
		private static Plane[] _planes = null;							// Our walls.
		private static Sphere _sphere = null;							// Our sphere.
		private static GorgonDepthStencilStates _depth;					// Depth enabled.
		private static GorgonDepthStencilStates _noDepth;				// No depth enabled.
		private static bool _bounceH = false;							// Horizontal bounce.
		private static bool _bounceV = false;							// Vertical bounce.
		private static float _rotate = -45.0f;							// Ball rotation.
		private static float _rotateSpeed = 1.0f;						// Rotation speed.
		private static float _dropSpeed = 0.01f;						// Drop speed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface for the application.
		/// </summary>
		public static GorgonGraphics Graphics
		{
			get
			{
				return _graphics;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the ball position.
		/// </summary>
		private static void UpdateBall()
		{
			Vector3 position = _sphere.Position;

			if (!_bounceH)
			{
				position.X -= 4.0f * GorgonTiming.Delta;
			}
			else
			{
				position.X += 4.0f * GorgonTiming.Delta;
			}

			if (!_bounceV)
			{
				position.Y += 4.0f * GorgonTiming.Delta * (_dropSpeed / 27f);
			}
			else
			{
				position.Y -= 4.0f * GorgonTiming.Delta * (_dropSpeed / 27f);
			}

			if ((position.X < -2.3f) || (position.X > 2.3f))
			{
				if (position.X < -2.3f)
				{
					position.X = -2.3f;
				}

				if (position.X > 2.3f)
				{
					position.X = 2.3f;
				}

				_bounceH = !_bounceH;				
			}

			if ((position.Y > 2.0f) || (position.Y < -2.5f))
			{
				_bounceV = !_bounceV;
				if (position.Y > 2.0f)
				{
					position.Y = 2.0f;
					_dropSpeed = 27f;
				}
				else
				{
					position.Y = -2.5f;
					_dropSpeed = 27f;
				}
			}

			if (_bounceV)
			{
				_dropSpeed += 9.8f * GorgonTiming.Delta;
			}
			else
			{
				_dropSpeed -= 9.8f * GorgonTiming.Delta;
			}

			_sphere.Rotation = new Vector3(0, _rotate, -12.0f);
			_sphere.Position = position;

			_rotate += 90.0f * GorgonTiming.Delta * _rotateSpeed.Sin();
			_rotateSpeed += GorgonTiming.Delta / 2.0f;
		}

		/// <summary>
		/// Function to handle idle time for the application.
		/// </summary>
		/// <returns>TRUE to continue processing, FALSE to stop.</returns>
		private static bool Idle()
		{
			// Clear to our gray color.
			_swap.Clear(Color.FromArgb(173, 173, 173), 1.0f);

			// Animate the ball.
			UpdateBall();

			// Draw our planes.
			foreach (var plane in _planes)
			{
				plane.Draw();
			}

			// Draw the main sphere.
			_sphere.Draw();

			// Draw the sphere shadow first.
			// This will 
			var spherePosition = _sphere.Position;
			_graphics.Output.DepthStencilState.States = _noDepth;
			_graphics.Shaders.PixelShader.Current = _pixelShaderDiffuse;
			_sphere.Position = new Vector3(spherePosition.X + 0.25f, spherePosition.Y - 0.125f, spherePosition.Z);
			_sphere.Draw();

			// Reset our sphere position, pixel shader and depth writing state.
			_sphere.Position = spherePosition;
			_graphics.Output.DepthStencilState.States = _depth;
			_graphics.Shaders.PixelShader.Current = _pixelShader;

			// Draw our text.
			// Use this to show how incredibly slow and terrible my 3D code is.
			_2D.Begin2D();
			_2D.Drawing.FilledRectangle(new RectangleF(0, 0, _mainForm.ClientSize.Width - 1.0f, 38.0f), Color.FromArgb(128, 0, 0, 0));
			_2D.Drawing.DrawRectangle(new RectangleF(0, 0, _mainForm.ClientSize.Width, 38.0f), Color.White);
			_2D.Drawing.DrawString(_graphics.Fonts.DefaultFont, 
				"FPS: " + GorgonTiming.AverageFPS.ToString("0.0")
				+ "\nDelta: " + (GorgonTiming.AverageDelta * 1000.0f).ToString("0.0##") + " milliseconds", 
				new Vector2(3.0f, 0.0f), GorgonColor.White);
			// Draw our logo because I'm insecure.
			_2D.Drawing.Blit(_graphics.Textures.GorgonLogo,
					new RectangleF(
						_mainForm.ClientSize.Width - _graphics.Textures.GorgonLogo.Settings.Width,
						_mainForm.ClientSize.Height - _graphics.Textures.GorgonLogo.Settings.Height,
						_graphics.Textures.GorgonLogo.Settings.Width,
						_graphics.Textures.GorgonLogo.Settings.Height));

			// Note that we're rendering here but not flipping the buffer (the 'false' parameter).  This just delays the page
			// flipping until later.  Technically, we don't need to do this here because it's the last thing we're doing, but
			// if we had more rendering to do after, we'd have to flip manually.
			_2D.Render(false);
			_2D.End2D();

			// Now we flip our buffers.
			// We need to this or we won't see anything.
			_swap.Flip();

			return true;
		}

		/// <summary>
		/// Function to update the world/view/projection matrix.
		/// </summary>
		/// <param name="world">The world matrix to update.</param>
		public static void UpdateWVP(ref Matrix world)
		{
			Matrix temp = Matrix.Identity;
			Matrix wvp = Matrix.Identity;

			// Build our world/view/projection matrix to send to
			// the shader.
			Matrix.Multiply(ref world, ref _viewMatrix, out temp);
			Matrix.Multiply(ref temp, ref _projMatrix, out wvp);

			// Direct 3D 11 requires that we transpose our matrix 
			// before sending it to the shader.
			Matrix.Transpose(ref wvp, out wvp);

			// Update the constant buffer.
			_wvpBufferStream.Write<Matrix>(wvp);
			_wvpBufferStream.Position = 0;
			_wvpBuffer.Update(_wvpBufferStream);
		}

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private static void Initialize()
		{
			GorgonViewport view;													// Our rendering viewport.
			BufferFormat depthFormat = BufferFormat.D24_UIntNormal_S8_UInt;			// Depth buffer format.

			// Create our form.
			_mainForm = new formMain();			

			// Create the main graphics interface.
			_graphics = new GorgonGraphics();
			
			// Validate depth buffer for this device.
			// Odds are good that if this fails, you should probably invest in a
			// better video card.  Preferably something created after 2005.
			if (!_graphics.VideoDevice.SupportsDepthFormat(depthFormat))
			{
				depthFormat = BufferFormat.D16_UIntNormal;
				if (!_graphics.VideoDevice.SupportsDepthFormat(depthFormat))
				{
					GorgonDialogs.ErrorBox(_mainForm, "Video device does not support a 24 or 16 bit depth buffer.");
					return;
				}
			}

			// Create a 1280x800 window with a depth buffer.
			// We can modify the resolution in the config file for the application, but
			// like other Gorgon examples, the default is 1280x800.
			_swap = _graphics.Output.CreateSwapChain("Main", new GorgonSwapChainSettings()
			{
				Window = _mainForm,										// Assign to our form.
				Format = BufferFormat.R8G8B8A8_UIntNormal,				// Set up for 32 bit RGBA normalized display.
				Size = Properties.Settings.Default.Resolution,			// Get the resolution from the config file.
				DepthStencilFormat = depthFormat,						// Get our depth format.
				IsWindowed = Properties.Settings.Default.IsWindowed		// Set up for windowed or full screen (depending on config file).
			});

			// Center on the primary monitor.
			// This is necessary because we already created the window, so it'll be off center at this point.
			_mainForm.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - _mainForm.Width / 2, 
											Screen.PrimaryScreen.WorkingArea.Height / 2 - _mainForm.Height / 2);

			// Handle any resizing.
			// This is here because the base graphics library will NOT handle state loss due to resizing.
			// This is up to the developer to handle.
			_swap.Resized += _swap_Resized;

			// Create the 2D interface for our text.
			_2D = _graphics.Output.Create2DRenderer(_swap);									

			// Turn off the 2D stuff for now.
			// We do this so we can have a clean slate for our state... *facepalm*.
			// Anyway, the 2D renderer initially sets up a bunch of state values for
			// us and these may interfere with our 3D display, so we'll just restore
			// the defaults (i.e. the previous states before the renderer was created)
			// by calling End2D.
			_2D.End2D();

			// Create our shaders.
			// Our vertex shader.  This is a simple shader, it just processes a vertex by multiplying it against
			// the world/view/projection matrix and spits it back out.
			_vertexShader = _graphics.Shaders.CreateShader<GorgonVertexShader>("VertexShader", "BoingerVS", Properties.Resources.Shader, true);
			// Our main pixel shader.  This is a very simple shader, it just reads a texture and spits it back out.  Has no
			// diffuse capability.
			_pixelShader = _graphics.Shaders.CreateShader<GorgonPixelShader>("PixelShader", "BoingerPS", Properties.Resources.Shader, true);
			// Our diffuse shader for our ball "shadow".  This is hard coded to send back black (R:0, G:0, B:0) at 50% opacity (A: 0.5).
			_pixelShaderDiffuse = _graphics.Shaders.CreateShader<GorgonPixelShader>("DiffuseShader", "BoingerDiffusePS", Properties.Resources.Shader, true);

			// Create the vertex input layout.
			// We need to create a layout for our vertex type because the shader won't know
			// how to interpret the data we're sending it otherwise.  This is why we need a 
			// vertex shader before we even create the layout.
			_inputLayout = _graphics.Input.CreateInputLayout("InputLayout", typeof(BoingerVertex), _vertexShader);

			// Create the view port.
			// This just tells the renderer how big our display is.
			view = new GorgonViewport(0, 0, _mainForm.ClientSize.Width, _mainForm.ClientSize.Height, 0.0f, 1.0f);

			// Load our textures from the resources.
			// This contains our textures for the walls and ball.  
			_texture = _graphics.Textures.FromGDIBitmap("PlaneTexture", Properties.Resources.Texture);

			// Set up our view matrix.
			// Move the camera (view matrix) back 2.2 units.  This will give us enough room to see what's
			// going on.
			Matrix.Translation(0, 0, 2.2f, out _viewMatrix);			

			// Set up our projection matrix.
			// This matrix is probably the cause of almost EVERY problem you'll ever run into in 3D programming.
			// Basically we're telling the renderer that we want to have a vertical FOV of 75 degrees, with the aspect ratio
			// based on our form width and height.  The final values indicate how to distribute Z values across depth (tip: 
			// it's not linear).
			_projMatrix = Matrix.PerspectiveFovLH((75.0f).Radians(), (float)_mainForm.Width / (float)_mainForm.Height, 0.125f, 500.0f);

			// Create our constant buffer and backing store.			
			// Our constant buffers are how we send data to our shaders.  This one in particular will be responsible
			// for sending our world/view/projection matrix to the vertex shader.  The stream we're creating after
			// the constant buffer is our system memory store for the data.  Basically we write to the system 
			// memory and then upload that data to the video card.  This is very different from how things used to
			// work, but allows a lot more flexibility.  
			_wvpBuffer = _graphics.Shaders.CreateConstantBuffer(Matrix.SizeInBytes, false);
			_wvpBufferStream = new GorgonDataStream(_wvpBuffer.SizeInBytes);

			// Create our planes.
			// Here's where we create the 2 planes for our rear wall and floor.  We set the texture size to texel units
			// because that's how the video card expects them.  However, it's a little hard to eyeball 0.67798223f by looking
			// at the texture image display, so we use the ToTexel function to determine our texel size.
			var textureSize = _texture.ToTexel(new Vector2(500, 500));
			_planes = new[] {
				new Plane(new Vector2(3.5f), new RectangleF(Vector2.Zero, textureSize)),
				new Plane(new Vector2(3.5f), new RectangleF(Vector2.Zero, textureSize))
			};

			// Set up default positions and orientations.
			_planes[0].Position = new Vector3(0, 0, 3.0f);
			_planes[1].Position = new Vector3(0, -3.5f, 3.5f);
			_planes[1].Rotation = new Vector3(90.0f, 0, 0);			

			// Create our sphere.
			// Again, here we're using texels to align the texture coordinates to the other image
			// packed into the texture (atlasing).  
			var textureOffset = _texture.ToTexel(new Vector2(516, 0));
			// This is to scale our texture coordinates because the actual image is much smaller
			// (256x256) than the full texture (1024x512).
			textureSize.X = 0.245f;
			textureSize.Y = 0.5f;
			_sphere = new Sphere(1.0f, textureOffset, textureSize);
			// Give the sphere a place to live.
			_sphere.Position = new Vector3(2.2f, 1.5f, 2.5f);

			// Bind our objects to the pipeline and set default states.
			// At this point we need to give the graphics card a bunch of things
			// it needs to do its job.  

			// Give our current input layout.
			_graphics.Input.Layout = _inputLayout;
			// We're drawing individual triangles for this (and this is usyally the case).
			_graphics.Input.PrimitiveType = PrimitiveType.TriangleList;

			// Bind our current vertex shader and send over our world/view/projection matrix
			// constant buffer.
			_graphics.Shaders.VertexShader.Current = _vertexShader;
			_graphics.Shaders.VertexShader.ConstantBuffers[0] = _wvpBuffer;

			// Do the same with the pixel shader, only we're binding our texture to it as well.
			// We also need to bind a sampler to the texture because without it, the shader won't
			// know how to interpret the texture data (e.g. how will the shader know if the texture
			// is supposed to be bilinear filtered or point filtered?)
			// 
			// Note that we're sending our world/view/projection matrix constant buffer to the pixel 
			// shader.  We should NOT have to do this, but I'm getting weird warnings when I don't send
			// it.  So, I'm doing this to shut it up.
			_graphics.Shaders.PixelShader.Current = _pixelShader;
			_graphics.Shaders.PixelShader.ConstantBuffers[0] = _wvpBuffer;
			_graphics.Shaders.PixelShader.Resources.SetTexture(0, _texture);
			_graphics.Shaders.PixelShader.TextureSamplers[0] = GorgonTextureSamplerStates.DefaultStates;

			// Turn on alpha blending.
			// Pretty much what this is.  Turning on alpha blending (for our shadow).
			_graphics.Output.BlendingState.States = new GorgonBlendStates()
			{
				IsAlphaCoverageEnabled = false,
				RenderTarget0 = new GorgonRenderTargetBlendState()
				{
					AlphaOperation = BlendOperation.Add,
					DestinationAlphaBlend = BlendType.Zero,
					BlendingOperation = BlendOperation.Add,
					DestinationBlend = BlendType.InverseSourceAlpha,
					IsBlendingEnabled = true,
					SourceAlphaBlend = BlendType.One,
					SourceBlend = BlendType.SourceAlpha,
					WriteMask = ColorWriteMaskFlags.All
				}
			};

			// Turn on depth writing.
			// This is our depth writing state.  When this is on, all polygon data sent to the card
			// will write to our depth buffer.  Normally we want this, but for translucent objects, it's
			// problematic....
			_depth = new GorgonDepthStencilStates()
			{
				DepthComparison = ComparisonOperators.LessEqual,
				IsDepthEnabled = true,
				IsDepthWriteEnabled = true,
				IsStencilEnabled = false
			};

			// Turn off depth writing.
			// So, we copy the depth state and turn off depth writing so that translucent objects 
			// won't write to the depth buffer but can still read it.
			_noDepth = _depth;
			_noDepth.IsDepthWriteEnabled = false;
			_graphics.Output.DepthStencilState.States = _depth;

			// Finally bind our swap chain and set up the default
			// rasterizer states.
			_graphics.Output.RenderTargets[0] = _swap;
			_graphics.Rasterizer.States = GorgonRasterizerStates.DefaultStates;
			_graphics.Rasterizer.SetViewport(view);

			// I know, there's a lot in here.  Thing is, if this were Direct 3D 11 code, it'd probably MUCH 
			// more code and that's even before creating our planes and sphere.
		}

		/// <summary>
		/// Handles the Resized event of the _swap control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		static void _swap_Resized(object sender, EventArgs e)
		{
			// Reset our projection matrix to match our new size.
			_projMatrix = Matrix.PerspectiveFovLH((75.0f).Radians(), (float)_mainForm.Width / (float)_mainForm.Height, 0.125f, 500.0f);
			// Update our viewport to reflect the new size.
			_graphics.Rasterizer.SetViewport(
				new GorgonViewport(0, 0, _mainForm.ClientSize.Width, _mainForm.ClientSize.Height, 0.0f, 1.0f)
			);
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				Initialize();
				Gorgon.Run(_mainForm, Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(null, ex));
			}
			finally
			{
				if (_wvpBufferStream != null)				
				{
					_wvpBufferStream.Dispose();
				}

				if (_graphics != null)
				{
					_graphics.Dispose();
				}
			}
		}
	}
}