#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Tuesday, June 05, 2007 9:31:30 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ControlExtenders;

namespace GorgonLibrary.Graphics.Tools.Controls
{
	/// <summary>
	/// Base manager control.
	/// </summary>
	public partial class Manager 
		: UserControl
	{
		#region Variables.
		private formMain _owner = null;							// Owning form.
		private ToolStripItem _bindingItem = null;				// Toolstrip item bound to this control.
		private IFloaty _dockWindow = null;						// Docking window interface.
		private bool _isVisible = true;							// Flag to indicate that the sprite manager is visible.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the control is docking.
		/// </summary>
		[Category("Layout"), Description("Event fired when the control is docked in its host container.")]
		public event EventHandler Docking;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the main form this control is bound with.
		/// </summary>
		[Browsable(false)]
		protected formMain MainForm
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the toolstrip item this control is bound with.
		/// </summary>
		[Browsable(false)]
		protected ToolStripItem Toolitem
		{
			get
			{
				return _bindingItem;
			}
		}

		/// <summary>
		/// Property to return the docking window.
		/// </summary>
		[Browsable(false)]
		public IFloaty DockWindow
		{
			get
			{
				return _dockWindow;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite manager is visible or not.
		/// </summary>
		[Browsable(true), Category("Behaviour"), Description("Determines when the control is visible or hidden.")]
		public new bool Visible
		{
			get
			{
				return _isVisible;
			}
			set
			{
				_isVisible = value;

				if (_dockWindow != null)
				{
					if (!value)
						_dockWindow.Hide();
					else
						_dockWindow.Show();
				}

				OnVisibleChanged(EventArgs.Empty);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Docking event of the _dockWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _dockWindow_Docking(object sender, EventArgs e)
		{
			OnDockWindowDocking();

			panelManagerCaption.Visible = true;

			if (Docking != null)
				Docking(this, EventArgs.Empty);
		}

		/// <summary>
		/// Handles the WindowClosing event of the _dockWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void _dockWindow_WindowClosing(object sender, CancelEventArgs e)
		{
			OnDockWindowClosing();
		}

		/// <summary>
		/// Handles the VisibleChanged event of the labelManager control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void labelManager_VisibleChanged(object sender, EventArgs e)
		{
			panelManagerCaption.Visible = labelManagerClose.Visible = labelManager.Visible;
		}

		/// <summary>
		/// Handles the MouseEnter event of the labelManagerClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void labelManagerClose_MouseEnter(object sender, EventArgs e)
		{
			labelManagerClose.Image = Properties.Resources.CloseButtonGlow1;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelManagerClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void labelManagerClose_MouseLeave(object sender, EventArgs e)
		{
			labelManagerClose.Image = Properties.Resources.CloseButton1;
		}

		/// <summary>
		/// Handles the Click event of the labelManagerClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		private void labelManagerClose_Click(object sender, EventArgs e)
		{
			Visible = false;
		}

		/// <summary>
		/// Handles the Click event of the _menuItem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _bindingItem_Click(object sender, EventArgs e)
		{
			OnMenuItemClick();
		}

		/// <summary>
		/// Function called when the bound menu item is clicked.
		/// </summary>
		/// <remarks>This function needs to be overridden.</remarks>
		protected virtual void OnMenuItemClick()
		{
		}

		/// <summary>
		/// Function called when the dock window is docking to its host container.
		/// </summary>
		protected virtual void OnDockWindowDocking()
		{
		}

		/// <summary>
		/// Function called when the undocked dock window is closed.
		/// </summary>
		protected virtual void OnDockWindowClosing()
		{
		}

		/// <summary>
		/// Function to validate the form interface.
		/// </summary>
		protected virtual void ValidateForm()
		{
		}
		
		/// <summary>
		/// Function to bind this control to the main form and a specific toolstrip item.
		/// </summary>
		/// <param name="mainForm">Main form to bind with.</param>
		/// <param name="toolItem">Toolstrip item to bind with.</param>
		public virtual void BindToMainForm(formMain mainForm, ToolStripItem toolItem)
		{
			_owner = mainForm;
			_bindingItem = toolItem;

			if (_bindingItem != null)
				_bindingItem.Click += new EventHandler(_bindingItem_Click);
		}

		/// <summary>
		/// Function to bind this control to a docker.
		/// </summary>
		/// <param name="dock">Docking extender.</param>
		/// <param name="host">Host control.</param>
		/// <param name="splitter">Splitter to use when docked.</param>
		public void BindToDocker(DockExtender dock, ScrollableControl host, Splitter splitter)
		{
			_dockWindow = dock.Attach(host, labelManager, splitter);
			_dockWindow.Docking += new EventHandler(_dockWindow_Docking);
			_dockWindow.WindowClosing += new CancelEventHandler(_dockWindow_WindowClosing);
			panelManagerCaption.Visible = true;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public Manager()
		{
			InitializeComponent();
		}
		#endregion
	}
}
