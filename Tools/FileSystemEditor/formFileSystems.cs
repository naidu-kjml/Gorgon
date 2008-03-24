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
// Created: Wednesday, April 11, 2007 4:56:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Utility;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Form for the file system plug-in editor.
	/// </summary>
	public partial class formFileSystems : Form
	{
		#region Methods.
		/// <summary>
		/// Function to validate the form interface.
		/// </summary>
		private void ValidateForm()
		{
			buttonRemovePlugIn.Enabled = false;

			// Check for selected plug-ins.
			if ((listPlugIns.Items.Count > 0) && (listPlugIns.SelectedItems.Count > 0))
				buttonRemovePlugIn.Enabled = true;
		}

		/// <summary>
		/// Function to fill the plug-in list.
		/// </summary>
		private void FillList()
		{
			ListViewItem newItem = null;		// List view item.

			listPlugIns.Items.Clear();

			// Add each type.
			foreach (FileSystemProvider fsType in FileSystemProviderCache.Providers)
			{
				// Only add plug-ins.
				if (fsType.IsPlugIn)
				{
					newItem = new ListViewItem(fsType.Description + " (" + fsType.Name + ")");
					newItem.SubItems.Add(fsType.PlugInPath);
					newItem.Name = fsType.Name;
					listPlugIns.Items.Add(newItem);
				}
			}

			ValidateForm();
			listPlugIns.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		/// <summary>
		/// Handles the Click event of the buttonAddPlugIn control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonAddPlugIn_Click(object sender, EventArgs e)
		{
			try
			{
				// Begin loading.
				if (dialogPlugInPath.ShowDialog(this) == DialogResult.OK)
				{
					foreach (string plugInPath in dialogPlugInPath.FileNames)
						FileSystemProvider.Load(plugInPath);
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				FillList();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonRemovePlugIn control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonRemovePlugIn_Click(object sender, EventArgs e)
		{
			formMain owner = null;		// Owner window.

			try
			{
				owner = (formMain)Owner;

				// Remove each plug-in.
				foreach (ListViewItem item in listPlugIns.SelectedItems)
				{
					// Begin closing any associated file systems.
					foreach (formFileSystemWindow window in owner.MdiChildren)
					{
                        if (string.Compare(window.FileSystem.Provider.Name, item.Name, true) == 0)
							window.Close();
					}

					// Reset the last used if we've closed this plug-in.
					if (owner.LastUsed == FileSystemProviderCache.Providers[item.Name])
						owner.LastUsed = FileSystemProviderCache.Providers[typeof(FolderFileSystem)];

					FileSystemProviderCache.Providers[item.Name].Dispose();
				}
				
				FillList();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the ItemSelectionChanged event of the listPlugIns control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.ListViewItemSelectionChangedEventArgs"/> instance containing the event data.</param>
		private void listPlugIns_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			ValidateForm();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			FillList();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formFileSystems()
		{
			InitializeComponent();
		}
		#endregion
	}
}