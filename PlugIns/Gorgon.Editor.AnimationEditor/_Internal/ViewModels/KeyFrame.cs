﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: June 16, 2020 4:24:54 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Animation;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// A view model for a key frame.
    /// </summary>
    internal class KeyFrame
        : ViewModelBase<KeyFrameParameters, IHostContentServices>, IKeyFrame
    {
        #region Variables.
        // The key frame time index.
        private float _time;
        // The texture value for the key.
        private TextureValue _texture;
        // The floating point values for the key.
        private DX.Vector4 _floatValues;
        #endregion

        #region Properties.
        /// <summary>Property to set or return the time index for the key frame.</summary>
        public float Time
        {
            get => _time;
            set
            {
                if (value.EqualsEpsilon(_time))
                {
                    return;
                }

                OnPropertyChanging();
                _time = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the type of data for this key frame.</summary>
        public AnimationTrackKeyType DataType
        {
            get;
            private set;
        }

        /// <summary>Property to set or return a texture value for the key frame.</summary>
        public TextureValue TextureValue
        {
            get => _texture;
            set
            {
                if (_texture.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                _texture = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return one to four floating point values for the key frame.</summary>
        public DX.Vector4 FloatValue
        {
            get => _floatValues;
            set
            {
                if (_floatValues.Equals(ref value))
                {
                    return;
                }

                OnPropertyChanging();
                _floatValues = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        ///   <para>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </para>
        ///   <para>
        /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
        /// </para>
        /// </remarks>
        protected override void OnInitialize(KeyFrameParameters injectionParameters)
        {
            _time = injectionParameters.Time;
            DataType = injectionParameters.KeyType;

            if ((DataType == AnimationTrackKeyType.Texture2D) && (injectionParameters.TextureValue != null))
            {
                _texture = injectionParameters.TextureValue.Value;
            }
            else if (injectionParameters.FloatValues != null)
            {
                _floatValues = injectionParameters.FloatValues.Value;
            }
        }
        #endregion
    }
}
