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
// Created: Wednesday, October 3, 2012 9:12:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SlimMath;

namespace GorgonLibrary.Animation
{
	/// <summary>
	/// A track that will animate properties with a Vector2 data type.
	/// </summary>
	/// <typeparam name="T">Type of object to be animated.</typeparam>
	class GorgonTrackVector2<T>
		: GorgonAnimationTrack<T>
		where T : class
	{
		#region Variables.
		private Func<T, Vector2> _getProperty = null;				// Get property method.
		private Action<T, Vector2> _setProperty = null;			// Set property method.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the supported interpolation modes for this track.
		/// </summary>
		public override TrackInterpolationMode SupportedInterpolation
		{
			get
			{
				return TrackInterpolationMode.Spline | TrackInterpolationMode.Linear;
			}
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the a key with the proper type for this track.
		/// </summary>
		/// <returns>The key with the proper type for this track.</returns>
		protected override IKeyFrame MakeKey()
		{
			return new GorgonKeyVector2(0, Vector2.Zero);
		}

		/// <summary>
		/// Function to set up the spline for the animation.
		/// </summary>
		protected internal override void SetupSpline()
		{
			base.SetupSpline();

			for (int i = 0; i < KeyFrames.Count; i++)
			{
				GorgonKeyVector2 key = (GorgonKeyVector2)KeyFrames[i];
				Spline.Points.Add(new Vector4(key.Value, 0.0f, 1.0f));
			}

			Spline.UpdateTangents();
		}

		/// <summary>
		/// Function to interpolate a new key frame from the nearest previous and next key frames.
		/// </summary>
		/// <param name="keyValues">Nearest previous and next key frames.</param>
		/// <param name="keyTime">The time to assign to the key.</param>
		/// <param name="unitTime">The time, expressed in unit time.</param>
		/// <returns>
		/// The interpolated key frame containing the interpolated values.
		/// </returns>
		protected override IKeyFrame GetTweenKey(ref GorgonAnimationTrack<T>.NearestKeys keyValues, float keyTime, float unitTime)
		{
			GorgonKeyVector2 next = (GorgonKeyVector2)keyValues.NextKey;
			GorgonKeyVector2 prev = (GorgonKeyVector2)keyValues.PreviousKey;

			switch (InterpolationMode)
			{
				case TrackInterpolationMode.Linear:
					return new GorgonKeyVector2(keyTime, Vector2.Lerp(prev.Value, next.Value, unitTime));
				case TrackInterpolationMode.Spline:
					return new GorgonKeyVector2(keyTime, (Vector2)Spline.GetInterpolatedValue(keyValues.PreviousKeyIndex, unitTime));					
				default:
					return prev;
			}
		}

		/// <summary>
		/// Function to apply the key value to the object properties.
		/// </summary>
		/// <param name="key">Key to apply to the properties.</param>
		protected internal override void ApplyKey(ref IKeyFrame key)
		{
			GorgonKeyVector2 value = (GorgonKeyVector2)key;
			_setProperty(Animation.AnimationController.AnimatedObject, value.Value);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTrackVector2{T}" /> class.
		/// </summary>
		/// <param name="property">Property information.</param>
		internal GorgonTrackVector2(GorgonAnimatedProperty property)
			: base(property)
		{
			_getProperty = BuildGetAccessor<Vector2>();
			_setProperty = BuildSetAccessor<Vector2>();

			InterpolationMode = TrackInterpolationMode.Linear;
		}
		#endregion
	}
}
