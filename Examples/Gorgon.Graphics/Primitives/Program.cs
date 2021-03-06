﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 9:38:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;
using GI = Gorgon.Input;

namespace Gorgon.Examples
{
    /// <summary>
    /// Our application entry point.
    /// </summary>
    internal static class Program
    {
        #region Variables.
        // The main application window.
        private static FormMain _window;
        // Camera.
        private static Camera _camera;
        // Rotation value.
        private static float _objRotation;
        // Rotation value.
        private static float _cloudRotation;
        // Input factory.
        private static GI.GorgonRawInput _input;
        // Keyboard interface.
        private static GI.GorgonRawKeyboard _keyboard;
        // Mouse interface.
        private static GI.GorgonRawMouse _mouse;
        // Camera rotation amount.
        private static DX.Vector3 _cameraRotation;
        // Lock to sphere.
        private static bool _lock;
        // Mouse sensitivity.
        private static float _sensitivity = 1.5f;
        // Graphics interface.
        private static GorgonGraphics _graphics;
        // Primary swap chain.
        private static GorgonSwapChain _swapChain;
        // The depth buffer.
        private static GorgonDepthStencil2DView _depthBuffer;
        // Triangle primitive.
        private static Triangle _triangle;
        // Plane primitive.
        private static Plane _plane;
        // Cube primitive.
        private static Cube _cube;
        // Sphere primitive.
        private static Sphere _sphere;
        // Sphere primitive.
        private static Sphere _clouds;
        // Icosphere primitive.
        private static IcoSphere _icoSphere;
        // A simple application specific renderer.
        private static SimpleRenderer _renderer;
        // Our 2D renderer for rendering text.
        private static Gorgon2D _2DRenderer;
        // The font used to render our text.
        private static GorgonFont _font;
        // The text sprite used to display our info.
        private static GorgonTextSprite _textSprite;
        #endregion

        /// <summary>
        /// Main application loop.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
        private static bool Idle()
        {
            ProcessKeys();

            _swapChain.RenderTargetView.Clear(Color.CornflowerBlue);
            _depthBuffer.Clear(1.0f, 0);

            _cloudRotation += 2.0f * GorgonTiming.Delta;
            _objRotation += 50.0f * GorgonTiming.Delta;

            if (_cloudRotation > 359.9f)
            {
                _cloudRotation -= 359.9f;
            }

            if (_objRotation > 359.9f)
            {
                _objRotation -= 359.9f;
            }

            _triangle.Material.TextureOffset = new DX.Vector2(0, _triangle.Material.TextureOffset.Y - (0.125f * GorgonTiming.Delta));

            if (_triangle.Material.TextureOffset.Y < 0.0f)
            {
                _triangle.Material.TextureOffset = new DX.Vector2(0, 1.0f + _triangle.Material.TextureOffset.Y);
            }

            _plane.Material.TextureOffset = _triangle.Material.TextureOffset;

            _icoSphere.Rotation = new DX.Vector3(0, _icoSphere.Rotation.Y + (4.0f * GorgonTiming.Delta), 0);
            _cube.Rotation = new DX.Vector3(_objRotation, _objRotation, _objRotation);
            _sphere.Position = new DX.Vector3(-2.0f, (_objRotation.ToRadians().Sin().Abs() * 2.0f) - 1.10f, 0.75f);
            _sphere.Rotation = new DX.Vector3(_objRotation, _objRotation, 0);
            _clouds.Rotation = new DX.Vector3(0, _cloudRotation, 0);

            _renderer.Render();

            ref readonly GorgonGraphicsStatistics stats = ref _graphics.Statistics;

            _2DRenderer.Begin();
            _textSprite.Text = $@"FPS: {GorgonTiming.FPS:0.0}, Delta: {(GorgonTiming.Delta * 1000):0.000} msec. " +
                               $@"Tris: {stats.TriangleCount} " +
                               $@"Draw Calls: {stats.DrawCallCount} " +
                               $@"CamRot: {_cameraRotation} Mouse: {_mouse?.Position.X:0}x{_mouse?.Position.Y:0} Sensitivity: {_sensitivity:0.0##}";
            _2DRenderer.DrawTextSprite(_textSprite);
            _2DRenderer.End();

            GorgonExample.DrawStatsAndLogo(_2DRenderer);

            _swapChain.Present();

            return true;
        }

        /// <summary>
        /// Function to process the keyboard commands.
        /// </summary>
		private static void ProcessKeys()
        {
            DX.Vector3 cameraDir = DX.Vector3.Zero;
            DX.Vector3 lookAt = _camera.LookAt;
            lookAt.Normalize();

            if (_keyboard.KeyStates[Keys.Left] == GI.KeyState.Down)
            {
                _cameraRotation.Y -= 40.0f * GorgonTiming.Delta;
            }
            else if (_keyboard.KeyStates[Keys.Right] == GI.KeyState.Down)
            {
                _cameraRotation.Y += 40.0f * GorgonTiming.Delta;
            }
            if (_keyboard.KeyStates[Keys.Up] == GI.KeyState.Down)
            {
                _cameraRotation.X -= 40.0f * GorgonTiming.Delta;
            }
            else if (_keyboard.KeyStates[Keys.Down] == GI.KeyState.Down)
            {
                _cameraRotation.X += 40.0f * GorgonTiming.Delta;
            }
            if (_keyboard.KeyStates[Keys.PageUp] == GI.KeyState.Down)
            {
                _cameraRotation.Z -= 40.0f * GorgonTiming.Delta;
            }
            else if (_keyboard.KeyStates[Keys.PageDown] == GI.KeyState.Down)
            {
                _cameraRotation.Z += 40.0f * GorgonTiming.Delta;
            }
            if (_keyboard.KeyStates[Keys.D] == GI.KeyState.Down)
            {
                cameraDir.X = 2.0f * GorgonTiming.Delta;
            }
            else if (_keyboard.KeyStates[Keys.A] == GI.KeyState.Down)
            {
                cameraDir.X = -2.0f * GorgonTiming.Delta;
            }
            if (_keyboard.KeyStates[Keys.W] == GI.KeyState.Down)
            {
                cameraDir.Z = 2.0f * GorgonTiming.Delta;
            }
            else if (_keyboard.KeyStates[Keys.S] == GI.KeyState.Down)
            {
                cameraDir.Z = -2.0f * GorgonTiming.Delta;
            }
            if (_keyboard.KeyStates[Keys.Q] == GI.KeyState.Down)
            {
                cameraDir.Y = 2.0f * GorgonTiming.Delta;
            }
            else if (_keyboard.KeyStates[Keys.E] == GI.KeyState.Down)
            {
                cameraDir.Y = -2.0f * GorgonTiming.Delta;
            }

            if (_lock)
            {
                _camera.Target(_sphere.Position);
            }

            _camera.Rotation = _cameraRotation;
            _camera.Move(ref cameraDir);
        }

        /// <summary>
        /// Function to handle the mouse wheel when it is moved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private static void Mouse_Wheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                _sensitivity -= 0.05f;

                if (_sensitivity < 0.05f)
                {
                    _sensitivity = 0.05f;
                }
            }
            else if (e.Delta > 0)
            {
                _sensitivity += 0.05f;

                if (_sensitivity > 3.0f)
                {
                    _sensitivity = 3.0f;
                }
            }
        }

        /// <summary>
        /// Function called when a key is held down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GI.GorgonKeyboardEventArgs" /> instance containing the event data.</param>
        private static void Keyboard_KeyDown(object sender, GI.GorgonKeyboardEventArgs e)
        {
            if (e.Key != Keys.L)
            {
                return;
            }

            _lock = !_lock;
            _camera.Target(_lock ? (DX.Vector3?)_sphere.Position : null);
        }

        /// <summary>
        /// Handles the Down event of the Mouse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private static void Mouse_Down(object sender, MouseEventArgs args)
        {
            if ((args.Button != MouseButtons.Right)
                || (_mouse != null))
            {
                return;
            }

            GI.GorgonRawMouse.CursorVisible = false;

            // Capture the cursor so that we can't move it outside the client area.
            Cursor.Clip = _window.RectangleToScreen(new Rectangle(_window.ClientSize.Width / 2, _window.ClientSize.Height / 2, 1, 1));

            _mouse = new GI.GorgonRawMouse();
            _input.RegisterDevice(_mouse);

            _mouse.MouseButtonUp += Mouse_Up;
            _mouse.MouseMove += RawMouse_MouseMove;
        }

        /// <summary>
        /// Function called when the mouse is moved while in raw input mode.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GI.GorgonMouseEventArgs" /> instance containing the event data.</param>
        private static void RawMouse_MouseMove(object sender, GI.GorgonMouseEventArgs e)
        {
            Point delta = e.RelativePosition;
            _cameraRotation.X += delta.Y.Sign() * (_sensitivity);
            _cameraRotation.Y += delta.X.Sign() * (_sensitivity);
        }

        /// <summary>
        /// Handles the Up event of the Mouse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="GI.GorgonMouseEventArgs"/> instance containing the event data.</param>
        private static void Mouse_Up(object sender, GI.GorgonMouseEventArgs args)
        {
            if (((args.Buttons & GI.MouseButtons.Right) != GI.MouseButtons.Right)
                || (_mouse == null))
            {
                return;
            }

            try
            {
                _mouse.MouseButtonUp -= Mouse_Up;
                _mouse.MouseMove -= RawMouse_MouseMove;

                _input.UnregisterDevice(_mouse);
                _mouse = null;

                Cursor.Clip = Rectangle.Empty;
                GI.GorgonRawMouse.CursorVisible = true;
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(_window, ex);
            }
        }

        /// <summary>
        /// Function to build or rebuild the depth buffer.
        /// </summary>
        /// <param name="width">The width of the depth buffer.</param>
        /// <param name="height">The height of the depth buffer.</param>
	    private static void BuildDepthBuffer(int width, int height)
        {
            _depthBuffer?.Dispose();
            _depthBuffer = GorgonDepthStencil2DView.CreateDepthStencil(_graphics,
                                                                       new GorgonTexture2DInfo
                                                                       {
                                                                           Usage = ResourceUsage.Default,
                                                                           Width = width,
                                                                           Height = height,
                                                                           Format = BufferFormat.D24_UNorm_S8_UInt,
                                                                           Binding = TextureBinding.DepthStencil
                                                                       });
        }

        /// <summary>
        /// Function to build the shaders required for the application.
        /// </summary>
	    private static void LoadShaders()
        {
            _renderer.ShaderCache["VertexShader"] = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Resources.Shaders, "PrimVS", true);
            _renderer.ShaderCache["PixelShader"] = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.Shaders, "PrimPS", true);
            _renderer.ShaderCache["BumpMapShader"] = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.Shaders, "PrimPSBump", true);
            _renderer.ShaderCache["WaterShader"] = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.Shaders, "PrimPSWaterBump", true);
        }

        /// <summary>
        /// Function to load textures from application resources.
        /// </summary>
	    private static void LoadTextures()
        {
            // Load standard images from the resource section.
            _renderer.TextureCache["UV"] = Resources.UV.ToTexture2D(_graphics,
                                                                    new GorgonTexture2DLoadOptions
                                                                    {
                                                                        Name = "UV"
                                                                    }).GetShaderResourceView();

            _renderer.TextureCache["Earth"] = Resources.earthmap1k.ToTexture2D(_graphics,
                                                                               new GorgonTexture2DLoadOptions
                                                                               {
                                                                                   Name = "Earth"
                                                                               }).GetShaderResourceView();

            _renderer.TextureCache["Earth_Specular"] = Resources.earthspec1k.ToTexture2D(_graphics,
                                                                                         new GorgonTexture2DLoadOptions
                                                                                         {
                                                                                             Name = "Earth_Specular"
                                                                                         }).GetShaderResourceView();

            _renderer.TextureCache["Clouds"] = Resources.earthcloudmap.ToTexture2D(_graphics,
                                                                                   new GorgonTexture2DLoadOptions
                                                                                   {
                                                                                       Name = "Clouds"
                                                                                   }).GetShaderResourceView();

            _renderer.TextureCache["GorgonNormalMap"] = Resources.normalmap.ToTexture2D(_graphics,
                                                                                        new GorgonTexture2DLoadOptions
                                                                                        {
                                                                                            Name = "GorgonNormalMap"
                                                                                        }).GetShaderResourceView();

            // The following images are DDS encoded and require an encoder to read them from the resources.
            var dds = new GorgonCodecDds();

            using (var stream = new MemoryStream(Resources.Rain_Height_NRM))
            using (IGorgonImage image = dds.FromStream(stream))
            {
                _renderer.TextureCache["Water_Normal"] = image.ToTexture2D(_graphics,
                                                                           new GorgonTexture2DLoadOptions
                                                                           {
                                                                               Name = "Water_Normal"
                                                                           }).GetShaderResourceView();
            }

            using (var stream = new MemoryStream(Resources.Rain_Height_SPEC))
            using (IGorgonImage image = dds.FromStream(stream))
            {
                _renderer.TextureCache["Water_Specular"] = image.ToTexture2D(_graphics,
                                                                             new GorgonTexture2DLoadOptions
                                                                             {
                                                                                 Name = "Water_Specular"
                                                                             }).GetShaderResourceView();
            }

            using (var stream = new MemoryStream(Resources.earthbump1k_NRM))
            using (IGorgonImage image = dds.FromStream(stream))
            {
                _renderer.TextureCache["Earth_Normal"] = image.ToTexture2D(_graphics,
                                                                           new GorgonTexture2DLoadOptions
                                                                           {
                                                                               Name = "Earth_Normal"
                                                                           }).GetShaderResourceView();
            }
        }

        /// <summary>
        /// Function to build the meshes.
        /// </summary>
	    private static void BuildMeshes()
        {
            var fnU = new DX.Vector3(0.5f, 1.0f, 0);
            var fnV = new DX.Vector3(1.0f, 1.0f, 0);
            DX.Vector3.Cross(ref fnU, ref fnV, out DX.Vector3 faceNormal);
            faceNormal.Normalize();

            _triangle = new Triangle(_graphics,
                                     new Vertex3D
                                     {
                                         Position = new DX.Vector4(-12.5f, -1.5f, 12.5f, 1),
                                         Normal = faceNormal,
                                         UV = new DX.Vector2(0, 1.0f)
                                     },
                                     new Vertex3D
                                     {
                                         Position = new DX.Vector4(0, 24.5f, 12.5f, 1),
                                         Normal = faceNormal,
                                         UV = new DX.Vector2(0.5f, 0.0f)
                                     },
                                     new Vertex3D
                                     {
                                         Position = new DX.Vector4(12.5f, -1.5f, 12.5f, 1),
                                         Normal = faceNormal,
                                         UV = new DX.Vector2(1.0f, 1.0f)
                                     })
            {
                Material =
                            {
                                PixelShader = "WaterShader",
                                VertexShader = "VertexShader",
                                Textures =
                                {
                                    [0] = "UV",
                                    [1] = "Water_Normal",
                                    [2] = "Water_Specular"
                                }
                            },
                Position = new DX.Vector3(0, 0, 1.0f)
            };
            _renderer.Meshes.Add(_triangle);

            _plane = new Plane(_graphics, new DX.Vector2(25.0f, 25.0f), new RectangleF(0, 0, 1.0f, 1.0f), new DX.Vector3(90, 0, 0), 32, 32)
            {
                Position = new DX.Vector3(0, -1.5f, 1.0f),
                Material =
                         {
                             PixelShader = "WaterShader",
                             VertexShader = "VertexShader",
                             Textures =
                             {
                                 [0] = "UV",
                                 [1] = "Water_Normal",
                                 [2] = "Water_Specular"
                             }
                         }
            };
            _renderer.Meshes.Add(_plane);

            _cube = new Cube(_graphics, new DX.Vector3(1, 1, 1), new RectangleF(0, 0, 1.0f, 1.0f), new DX.Vector3(45.0f, 0, 0))
            {
                Position = new DX.Vector3(0, 0, 1.5f),
                Material =
                        {
                            SpecularPower = 0,
                            PixelShader = "BumpMapShader",
                            VertexShader = "VertexShader",
                            Textures =
                            {
                                [0] = "UV",
                                [1] = "GorgonNormalMap"
                            }
                        }
            };
            _renderer.Meshes.Add(_cube);

            _sphere = new Sphere(_graphics, 1.0f, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), DX.Vector3.Zero, 64, 64)
            {
                Position = new DX.Vector3(-2.0f, 1.0f, 0.75f),
                Material =
                          {
                              PixelShader = "PixelShader",
                              VertexShader = "VertexShader",
                              SpecularPower = 0.75f,
                              Textures =
                              {
                                  [0] = "Earth"
                              }
                          }
            };
            _renderer.Meshes.Add(_sphere);

            _icoSphere = new IcoSphere(_graphics, 5.0f, new DX.RectangleF(0, 0, 1, 1), DX.Vector3.Zero, 3)
            {
                Rotation = new DX.Vector3(0, -45.0f, 0),
                Position = new DX.Vector3(10, 2, 9.5f),
                Material =
                             {
                                 PixelShader = "BumpMapShader",
                                 VertexShader = "VertexShader",
                                 Textures =
                                 {
                                     [0] = "Earth",
                                     [1] = "Earth_Normal",
                                     [2] = "Earth_Specular"
                                 }
                             }
            };
            _renderer.Meshes.Add(_icoSphere);

            _clouds = new Sphere(_graphics, 5.125f, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), DX.Vector3.Zero, 16)
            {
                Position = new DX.Vector3(10, 2, 9.5f),
                Material =
                          {
                              PixelShader = "PixelShader",
                              VertexShader = "VertexShader",
                              BlendState = GorgonBlendState.Additive,
                              Textures =
                              {
                                  [0] = "Clouds"
                              }
                          }
            };
            _renderer.Meshes.Add(_clouds);
        }

        /// <summary>
        /// Function to initialize the lights.
        /// </summary>
	    private static void BuildLights()
        {
            _renderer.Lights[0] = new Light
            {
                LightPosition = new DX.Vector3(1.0f, 1.0f, -1.0f),
                SpecularPower = 256.0f
            };

            _renderer.Lights[1] = new Light
            {
                LightPosition = new DX.Vector3(-5.0f, 5.0f, 8.0f),
                LightColor = Color.Yellow,
                SpecularColor = Color.Yellow,
                SpecularPower = 2048.0f,
                Attenuation = 10.0f
            };

            _renderer.Lights[2] = new Light
            {
                LightPosition = new DX.Vector3(5.0f, 3.0f, 10.0f),
                LightColor = Color.Red,
                SpecularColor = Color.Red,
                SpecularPower = 0.0f,
                Attenuation = 16.0f
            };
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        private static void Initialize()
        {
            GorgonExample.ShowStatistics = false;
            _window = GorgonExample.Initialize(new DX.Size2(Settings.Default.Resolution.Width, Settings.Default.Resolution.Height), "Primitives");

            try
            {
                // Find out which devices we have installed in the system.
                IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();

                if (deviceList.Count == 0)
                {
                    throw new
                        NotSupportedException("There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
                }

                _graphics = new GorgonGraphics(deviceList[0]);
                _renderer = new SimpleRenderer(_graphics);

                _swapChain = new GorgonSwapChain(_graphics,
                                                 _window,
                                                 new GorgonSwapChainInfo("Swap")
                                                 {
                                                     Width = _window.ClientSize.Width,
                                                     Height = _window.ClientSize.Height,
                                                     Format = BufferFormat.R8G8B8A8_UNorm
                                                 });

                BuildDepthBuffer(_swapChain.Width, _swapChain.Height);

                _graphics.SetRenderTarget(_swapChain.RenderTargetView, _depthBuffer);

                LoadShaders();

                LoadTextures();

                BuildLights();

                BuildMeshes();

                _renderer.Camera = _camera = new Camera
                {
                    Fov = 75.0f,
                    ViewWidth = _swapChain.Width,
                    ViewHeight = _swapChain.Height
                };

                _input = new GI.GorgonRawInput(_window);
                _keyboard = new GI.GorgonRawKeyboard();

                _input.RegisterDevice(_keyboard);

                _keyboard.KeyDown += Keyboard_KeyDown;
                _window.MouseDown += Mouse_Down;
                _window.MouseWheel += Mouse_Wheel;

                _swapChain.SwapChainResizing += (sender, args) =>
                                                     {
                                                         _graphics.SetDepthStencil(null);
                                                     };

                // When we resize, update the projection and viewport to match our client size.
                _swapChain.SwapChainResized += (sender, args) =>
                                                    {
                                                        _camera.ViewWidth = args.Size.Width;
                                                        _camera.ViewHeight = args.Size.Height;

                                                        BuildDepthBuffer(args.Size.Width, args.Size.Height);

                                                        _graphics.SetDepthStencil(_depthBuffer);
                                                    };

                _2DRenderer = new Gorgon2D(_graphics);

                GorgonExample.LoadResources(_graphics);

                // Create a font so we can render some text.
                _font = GorgonExample.Fonts.GetFont(new GorgonFontInfo("Segoe UI", 14.0f, FontHeightMode.Points, "Segoe UI 14pt")
                {
                    OutlineSize = 2,
                    OutlineColor1 = GorgonColor.Black,
                    OutlineColor2 = GorgonColor.Black
                });

                _textSprite = new GorgonTextSprite(_font)
                {
                    DrawMode = TextDrawMode.OutlinedGlyphs
                };
            }
            finally
            {
                GorgonExample.EndInit();
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Initialize();

                GorgonApplication.Run(_window, Idle);
            }
            catch (Exception ex)
            {
                GorgonExample.HandleException(ex);
            }
            finally
            {
                GorgonExample.UnloadResources();

                _2DRenderer.Dispose();

                _input?.Dispose();

                if (_renderer != null)
                {
                    foreach (Mesh mesh in _renderer.Meshes)
                    {
                        mesh.Dispose();
                    }

                    _renderer.Dispose();
                }

                _icoSphere?.Dispose();
                _clouds?.Dispose();
                _sphere?.Dispose();
                _cube?.Dispose();
                _plane?.Dispose();
                _triangle?.Dispose();

                _swapChain?.Dispose();
                _graphics?.Dispose();
            }
        }
    }
}
