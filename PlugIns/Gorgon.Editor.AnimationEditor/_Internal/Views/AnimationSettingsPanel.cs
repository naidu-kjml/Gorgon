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
// Created: April 20, 2019 2:19:56 PM
// 
#endregion

using System;
using System.ComponentModel;
using DX = SharpDX;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// The panel used to display settings for image codec support.
    /// </summary>
    internal partial class AnimationSettingsPanel
        : SettingsBaseControl, IDataContext<ISettings>
    {
        #region Properties.
        /// <summary>Property to return the ID of the panel.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string PanelID => DataContext?.ID.ToString() ?? Guid.Empty.ToString();

        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISettings DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the Click event of the CheckUnsupported control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckUnsupported_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.WarnUnsupportedTracks = CheckUnsupported.Checked;
        }

        /// <summary>Handles the Click event of the CheckAnimatePrimaryBg control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckAnimatePrimaryBg_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.AnimateNoPrimarySpriteBackground = CheckAnimatePrimaryBg.Checked;
        }

        /// <summary>Handles the Click event of the CheckOnionSkin control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckOnionSkin_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.UseOnionSkinning = CheckOnionSkin.Checked;
        }

        /// <summary>Handles the Click event of the CheckAddTextureTrack control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckAddTextureTrack_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.AddTextureTrackForPrimarySprite = CheckAddTextureTrack.Checked;
        }

        /// <summary>Handles the ValueChanged event of the NumericXRes control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericXRes_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.DefaultResolution = new DX.Size2((int)NumericXRes.Value, (int)NumericYRes.Value);
        }

        /// <summary>
        /// Function to restore the control to its default state.
        /// </summary>
        private void ResetDataContext()
        {
            CheckAnimatePrimaryBg.Checked = true;
            CheckOnionSkin.Checked = true;
            CheckAddTextureTrack.Checked = true;
            CheckUnsupported.Checked = true;
            NumericXRes.Value = 256;
            NumericYRes.Value = 256;
        }

        /// <summary>
        /// Function to initialize the control from the specified data context.
        /// </summary>
        /// <param name="dataContext">The data context to apply.</param>
        private void InitializeFromDataContext(ISettings dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            CheckAnimatePrimaryBg.Checked = dataContext.AnimateNoPrimarySpriteBackground;
            CheckOnionSkin.Checked = dataContext.UseOnionSkinning;
            CheckAddTextureTrack.Checked = dataContext.AddTextureTrackForPrimarySprite;
            CheckUnsupported.Checked = dataContext.WarnUnsupportedTracks;
            NumericXRes.Value = dataContext.DefaultResolution.Width;
            NumericYRes.Value = dataContext.DefaultResolution.Height;
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISettings dataContext)
        {
            InitializeFromDataContext(dataContext);
            DataContext = dataContext;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="AnimationSettingsPanel"/> class.</summary>
        public AnimationSettingsPanel() => InitializeComponent();
        #endregion
    }
}
