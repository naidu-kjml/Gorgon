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
// Created: June 16, 2020 8:00:52 PM
// 
#endregion


namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// Arguments for the <see cref="IAnimationContent.GetTrackKeyCommand"/>.
    /// </summary>
    internal class GetTrackKeyArgs
    {
        #region Properties.
        /// <summary>
        /// Property to set or return whether there's key data in the track.
        /// </summary>
        public IKeyFrame Key
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the index of the key to look up.
        /// </summary>
        public int KeyIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the index of the track to look into.
        /// </summary>
        public int TrackIndex
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="GetTrackKeyArgs"/> class.</summary>
        /// <param name="keyIndex">The index of the key.</param>
        /// <param name="trackIndex">The index of the track.</param>
        public GetTrackKeyArgs(int keyIndex, int trackIndex)
        {
            KeyIndex = keyIndex;
            TrackIndex = trackIndex;
        }
        #endregion
    }
}
