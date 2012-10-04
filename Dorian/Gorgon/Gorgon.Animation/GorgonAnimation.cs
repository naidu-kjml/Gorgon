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
// Created: Monday, September 3, 2012 8:29:35 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Animation
{
	/// <summary>
	/// An animation clip for an animated object.
	/// </summary>
	public class GorgonAnimation
		: GorgonNamedObject, ICloneable<GorgonAnimation>
	{
		#region Variables.
        private GorgonAnimationController _controller = null;       // Animation controller that owns this object.
		private float _length = 0;                                  // Length of the animation, in milliseconds.
		private float _time = 0;                                    // Current time for the animation, in milliseconds.
		private int _loopCount = 0;                                 // Number of loops for the animation.
		private int _looped = 0;                                    // Number of times the animation has currently looped.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return the animation controller that owns this clip.
        /// </summary>
        public GorgonAnimationController AnimationController
        {
            get
            {
                return _controller;
            }
            internal set
            {
                if (value == _controller)
                    return;

                _controller = value;
                EnumerateTracks(_controller.AnimatedObject);
            }
        }

		/// <summary>
		/// Property to set or return the number of times to loop an animation.
		/// </summary>
		/// <remarks></remarks>
		public int LoopCount
		{
			get
			{
				return _loopCount;
			}
			set
			{
				if (value < 0)
					value = 0;
				_loopCount = value;
			}
		}

		/// <summary>
		/// Property to set or return the speed of the animation.
		/// </summary>
		/// <remarks>Setting this value to a negative value will make the animation play backwards.</remarks>
		public float Speed
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether this animation is currently playing or not.
		/// </summary>
		public bool IsPlaying
		{
			get
			{
                if (AnimationController == null)
					return false;
                                
				return AnimationController.CurrentAnimation == this;
			}
		}

		/// <summary>
		/// Property to set or return the length of the animation (in milliseconds).
		/// </summary>
		public float Length
		{
			get
			{
				return _length;
			}
			set
			{
				if (value < 0)
					value = 0;

				_length = value;
			}
		}

		/// <summary>
		/// Property to set or return whether this animation should be looping or not.
		/// </summary>
		public bool IsLooped
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the current time, in milliseconds, for this animation.
		/// </summary>
		public float Time
		{
			get
			{
				return _time;
			}
			set
			{
				if ((_time == value) || (_length <= 0))
					return;

				_time = value;

				if ((IsLooped) && ((_time > _length) || (_time < 0)))
				{
					// Loop the animation.
					if ((_loopCount == 0) || (_looped != _loopCount))
					{
						_looped++;
						_time = _time % _length;
						if (_time < 0)
							_time += _length;
						return;
					}
				}

				if (_time < 0)
					_time = 0;
				if (_time > _length)
					_time = _length;
			}
		}
		
		/// <summary>
		/// Property to return the list of tracks for the animation.
		/// </summary>
		public GorgonAnimationTrackCollection Tracks
		{
			get;
			private set;
		}
		#endregion
		#region Methods.

		/// <summary>
		/// Function to update the owner of the animation.
		/// </summary>
		internal void UpdateOwner()
		{
			if (_owner == null)
				return;

			// Notify each track to update their animation to the current time.
			foreach (var track in Tracks)
			{
				if (track.KeyFrames.Count > 0)
				{
					IKeyFrame key = track.GetKeyAtTime(_time);
					track.ApplyKey(ref key);
				}
			}
		}

        /// <summary>
        /// Function to build the track list for the animated object.
        /// </summary>
        /// <param name="animatedObject">The object that contains the properties to animate.</param>
        /// <remarks>This method will enumerate the properties on an animated object and make a track for each animated property.  To mark a property as animated, assign a <see cref="GorgonLibrary.Animation.AnimatedPropertyAttribute">AnimatedPropertyAttribute</see> to the property.</remarks>
        public void EnumerateTracks(Object animatedObject)
        {
            if ((animatedObject == null) || (animatedObject.Animations == null))
                return;

            // Enumerate tracks from the owner object animated properties list.
            foreach (var item in animatedObject.Animations.AnimatedProperties)
            {
                if (Tracks.Contains(item.Value.DisplayName))		// Don't add tracks that are already here.
                    continue;

                switch (item.Value.DataType.FullName.ToLower())
                {
                    case "system.byte":
                        Tracks.Add(new GorgonTrackByte(item.Value));
                        break;
                    case "system.sbyte":
                        Tracks.Add(new GorgonTrackSByte(item.Value));
                        break;
                    case "system.int16":
                        Tracks.Add(new GorgonTrackInt16(item.Value));
                        break;
                    case "system.uint16":
                        Tracks.Add(new GorgonTrackUInt16(item.Value));
                        break;
                    case "system.int32":
                        Tracks.Add(new GorgonTrackInt32(item.Value));
                        break;
                    case "system.uint32":
                        Tracks.Add(new GorgonTrackUInt32(item.Value));
                        break;
                    case "system.int64":
                        Tracks.Add(new GorgonTrackInt64(item.Value));
                        break;
                    case "system.uint64":
                        Tracks.Add(new GorgonTrackUInt64(item.Value));
                        break;
                    case "system.single":
                        Tracks.Add(new GorgonTrackSingle(item.Value));
                        break;
                    case "slimmath.vector2":
                        Tracks.Add(new GorgonTrackVector2(item.Value));
                        break;
                    case "slimmath.vector3":
                        Tracks.Add(new GorgonTrackVector3(item.Value));
                        break;
                    case "slimmath.vector4":
                        Tracks.Add(new GorgonTrackVector4(item.Value));
                        break;
                    case "gorgonlibrary.graphics.gorgoncolor":
                        Tracks.Add(new GorgonTrackGorgonColor(item.Value));
                        break;
                }
            }
        }
        
        /// <summary>
		/// Function to reset the animation state.
		/// </summary>
		public void Reset()
		{
			_time = 0;
			_looped = 0;
			UpdateOwner();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonAnimation" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="length">The length of the animation, in milliseconds.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public GorgonAnimation(string name, float length)
			: base(name)
		{
			GorgonDebug.AssertParamString(name, "name");

			Tracks = new GorgonAnimationTrackCollection(this);
			Length = length;
			Speed = 1.0f;
		}
		#endregion		
	
		#region ICloneable<GorgonAnimation> Members
		/// <summary>
		/// Function to clone the animation.
		/// </summary>
		/// <returns>A clone of the animation.</returns>		
		public GorgonAnimation Clone()
		{
			GorgonAnimation clone = new GorgonAnimation(Name, Length);
			clone.IsLooped = IsLooped;
			clone.Length = Length;
			clone.LoopCount = LoopCount;
			clone.Speed = Speed;
			clone.Time = Time;
			clone.EnumerateTracks(Owner);

			foreach (var track in Tracks)
			{
				if (clone.Tracks.Contains(track.Name))
				{
					foreach (var key in track.KeyFrames)
						clone.Tracks[track.Name].KeyFrames.Add(key.Clone());
				}
			}

			return clone;
		}
		#endregion
	}
}