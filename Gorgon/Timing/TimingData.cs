#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Saturday, July 22, 2006 12:33:28 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities.Native.Win32;

namespace GorgonLibrary
{
	/// <summary>
	/// Object for timing data like FPS, # of frames, etc...
	/// </summary>
	public class TimingData
		: IDisposable
	{
		#region Constants.
		private const int AverageFpsFrameMax = 5;		// Maximum number of frames to skip before average is calculated.
		#endregion

		#region Variables.
		private double _lastFrameTime;								// Last frame time.
		private double _frameDrawTime;								// Time to draw a frame in milliseconds.
		private double _lastFPSFrameTime;							// Last FPS.
		private float _averageFps;									// Average FPS.
		private float _highestFps;									// Highest FPS.
		private float _lowestFps;									// Lowest FPS.
		private float _currentFps;									// Current FPS.
		private long _frameCount;									// Frame count.
        private long _totalFrameCount;                              // Total frame count.
		private PreciseTimer _timer;								// FPS timer.
		private int _frameAvgCounter;								// Counter for frame average.
		private double _frameAvgSum;								// Frame average sum.
		private static TimingData _empty = new TimingData(null);	// Empty timing data.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return an empty timing data structure.
		/// </summary>
		public static TimingData Empty
		{
			get
			{
				return _empty;
			}
		}

		/// <summary>
		/// Property to return the amount of time to draw a frame in milliseconds.
		/// </summary>
		public double FrameDrawTime
		{
			get
			{
				return _frameDrawTime;
			}
		}

		/// <summary>
		/// Property to return the average Frames Per Second.
		/// </summary>
		public float AverageFps
		{
			get
			{
				return _averageFps;
			}
		}

		/// <summary>
		/// Property to return the highest Frames Per Second.
		/// </summary>
		public float HighestFps
		{
			get
			{
				return _highestFps;
			}
		}

		/// <summary>
		/// Property to return the lowest Frames Per Second.
		/// </summary>
		public float LowestFps
		{
			get
			{
				return _lowestFps;
			}
		}

		/// <summary>
		/// Property to return the current Frames Per Second.
		/// </summary>
		public float CurrentFps
		{
			get
			{
				return _currentFps;
			}
		}

		/// <summary>
		/// Property to return the current number of frames drawn.
		/// </summary>
		public long FrameCount
		{
			get
			{
				return _totalFrameCount;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to calculate the frames per second.
		/// </summary>
		private void GetFps()
		{
			_frameCount++;
            _totalFrameCount++;

			if ((_lastFrameTime - _lastFPSFrameTime) >= 1000)
			{
				_currentFps = (float)(((double)_frameCount / (_lastFrameTime - _lastFPSFrameTime)) * 1000.0);

				if (_frameAvgCounter < AverageFpsFrameMax)
				{
					_averageFps = _currentFps;
					_frameAvgSum += _currentFps;
					_frameAvgCounter++;
				}
				else
				{
					_averageFps = (float)(_frameAvgSum / (double)AverageFpsFrameMax);

					_frameAvgCounter = 0;
					_frameAvgSum = 0.0;
				}

				if (_currentFps > _highestFps)
					_highestFps = _currentFps;

				if ((_currentFps < _lowestFps) || (_lowestFps == 0))
					_lowestFps = _currentFps;

				_lastFPSFrameTime = _lastFrameTime;
				_frameCount = 0;
			}
		}

		/// <summary>
		/// Function to reset the timing data.
		/// </summary>
		public void Reset()
		{
			if (_timer != null)
				_timer.Reset();
			_lastFPSFrameTime = 0.0;			
			_currentFps = 0.0f;
			_frameCount = 0;
			_lastFrameTime = 0.0;
			_frameDrawTime = 0.0;
			_frameAvgCounter = 0;
			_frameAvgSum = 0.0;
            _totalFrameCount = 0;
		}

		/// <summary>
		/// Function to end timing routine.
		/// </summary>
		/// <returns>TRUE to continue, FALSE to implement a delay.</returns>
		public bool Refresh()
		{
			/* This code has been pulled and modified from the HGE source */
			double theTime = 0.0;		// The current time.

			// If we have a roll-over, keep adjusting until we're positive.
			// This could cause a hiccup, but should happen so infrequently
			// that it shouldn't matter.
			do
			{
				theTime = _timer.Milliseconds - _lastFrameTime;
			} while (theTime < 0.001);

			if (theTime >= Gorgon.MinimumFrameTime)
			{
				_frameDrawTime = theTime;

				// Limit the frame time.
				if (_frameDrawTime > 200.0f)
				{
					if (Gorgon.MinimumFrameTime > 0.0)
						_frameDrawTime = Gorgon.MinimumFrameTime;
					else
						_frameDrawTime = 10.0f;
				}

				_lastFrameTime = _timer.Milliseconds;
				GetFps();

				return true;
			}
			else
			{
				// Sleep if we're outside of the minimum.
				if ((Gorgon.MinimumFrameTime > 0.0) && ((theTime + 3) < Gorgon.MinimumFrameTime))
					System.Threading.Thread.Sleep(1);

				return false;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="timer">Timer to use for data.</param>
		internal TimingData(PreciseTimer timer)
		{
			if (timer != null)
			{
				_timer = timer;
				if (!_timer.HighResolutionTimer)
					Win32API.timeBeginPeriod(1);
				Reset();
			}
			else
				_frameDrawTime = 10.0f;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~TimingData()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			Win32API.timeEndPeriod(1);
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
