﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 26, 2018 8:51:04 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.ViewModels;
using Gorgon.Timing;

namespace Gorgon.Editor
{
    /// <summary>
    /// The main application form.
    /// </summary>
    internal partial class FormMain 
        : KryptonForm, IDataContext<IMain>
    {
        #region Variables.
        // A registry of actions to execute for a ribbon button.
        private readonly Dictionary<KryptonRibbonGroupButton, Action> _ribbonButtonRegistry = new Dictionary<KryptonRibbonGroupButton, Action>();
        // The current action to use when cancelling an operation in progress.
        private Action _progressCancelAction;
        // The timer used to indicate how fast messages can be sent to the progress meter.
        private IGorgonTimer _progressTimer;
        // The current synchronization context.
        private SynchronizationContext _syncContext;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the data context assigned to this view.
        /// </summary>
        [Browsable(false)]
        public IMain DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the RenameBegin event of the PanelProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelProject_RenameBegin(object sender, EventArgs e) =>ButtonFileSystemRename.Enabled = false;

        /// <summary>
        /// Handles the RenameEnd event of the PanelProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PanelProject_RenameEnd(object sender, EventArgs e) => ValidateRibbonButtons();

        /// <summary>
        /// Handles the BackClicked event of the StageLauncher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void StageLauncher_BackClicked(object sender, EventArgs e) => NavigateToProjectView(DataContext);

        /// <summary>
        /// Handles the AppButtonMenuOpening event of the RibbonMain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void RibbonMain_AppButtonMenuOpening(object sender, CancelEventArgs e)
        {
            NavigateToStagingView();
            e.Cancel = true;
        }

        /// <summary>
        /// Function called when a project has been created.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void StageNewProject_ProjectCreated(object sender, ProjectCreateArgs e)
        {
            if ((DataContext?.AssignProjectCommand == null)
                || (!DataContext.AssignProjectCommand.CanExecute(e.Project)))
            {
                return;
            }

            DataContext.AssignProjectCommand.Execute(e.Project);
        }

        /// <summary>
        /// Function to validate the state of the ribbon buttons.
        /// </summary>
        private void ValidateRibbonButtons()
        {
            IProjectVm project = DataContext?.CurrentProject;
            IFileExplorerVm fileExplorer = project?.FileExplorer;

            if ((project == null) || (fileExplorer == null))
            {
                TabFileSystem.Visible = false;
                CheckShowAllFiles.Enabled = CheckShowAllFiles.Checked = false;
                return;
            }

            TabFileSystem.Visible = true;

            ButtonFileSystemNewDirectory.Enabled = (fileExplorer.CreateNodeCommand != null)
                                                && (fileExplorer.CreateNodeCommand.CanExecute(null));

            ButtonFileSystemRename.Enabled = (fileExplorer.RenameNodeCommand != null)
                                            && (fileExplorer.SelectedNode != null);

            ButtonExpand.Enabled = (fileExplorer.SelectedNode != null) && (fileExplorer.SelectedNode.Children.Count > 0);
            ButtonCollapse.Enabled = (fileExplorer.SelectedNode != null) && (fileExplorer.SelectedNode.Children.Count > 0);

            ButtonFileSystemDelete.Enabled = (fileExplorer.DeleteNodeCommand != null)
                                            && (fileExplorer.DeleteNodeCommand.CanExecute(null));

            CheckShowAllFiles.Enabled = true;
            CheckShowAllFiles.Checked = project.ShowExternalItems;
        }

        /// <summary>
        /// Function to ensure that the wait panel stays on top if it is active.
        /// </summary>
        private void KeepWaitPanelOnTop()
        {
            if (!WaitScreen.Visible)
            {
                return;
            }

            WaitScreen.BringToFront();
        }

        /// <summary>
        /// Function to navigate to the project view control.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void NavigateToProjectView(IMain dataContext)
        {
            StageLauncher.Visible = false;
            RibbonMain.Visible = true;
            PanelWorkSpace.Visible = true;

            PanelWorkSpace.BringToFront();
            KeepWaitPanelOnTop();

            PanelProject.SetDataContext(dataContext?.CurrentProject);
        }

        /// <summary>
        /// Function to navigate to the project view control.
        /// </summary>
        private void NavigateToStagingView()
        {
            StageLauncher.Visible = true;
            RibbonMain.Visible = false;
            PanelWorkSpace.Visible = false;

            StageLauncher.BringToFront();

            KeepWaitPanelOnTop();
        }

        /// <summary>
        /// Handles the ProgressDeactivated event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DataContext_ProgressDeactivated(object sender, EventArgs e)
        {
            _progressCancelAction = null;
            ProgressScreen.Visible = false;
            ProgressScreen.OperationCancelled -= ProgressScreen_OperationCancelled;
            _progressTimer = null;
        }

        /// <summary>
        /// Datas the context progress updated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DataContext_ProgressUpdated(object sender, ProgressPanelUpdateArgs e)
        {
            // Drop the message if it's been less than 10 milliseconds since our last call.
            // This will keep us from flooding the window message queue with very fast operations.
            if ((_progressTimer != null) && (_progressTimer.Milliseconds < 10))
            {
                return;
            }

            // The actual method to execute on the main thread.
            void UpdateProgressPanel(ProgressPanelUpdateArgs args)
            {
                ProgressScreen.ProgressTitle = e.Title ?? string.Empty;
                ProgressScreen.ProgressMessage = e.Message ?? string.Empty;
                ProgressScreen.CurrentValue = e.PercentageComplete;

                if (ProgressScreen.Visible)
                {
                    _progressTimer?.Reset();
                    return;
                }

                _progressTimer = GorgonTimerQpc.SupportsQpc() ? (IGorgonTimer)new GorgonTimerQpc() : new GorgonTimerMultimedia();
                _progressCancelAction = e.CancelAction;
                ProgressScreen.OperationCancelled += ProgressScreen_OperationCancelled;
                ProgressScreen.MeterStyle = e.IsMarquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                ProgressScreen.Visible = true;
                ProgressScreen.BringToFront();
                ProgressScreen.Invalidate();
            }

            if (InvokeRequired)
            {
                _syncContext.Send(args => UpdateProgressPanel((ProgressPanelUpdateArgs)args), e);
                return;
            }

            UpdateProgressPanel(e);
        }

        /// <summary>
        /// Handles the OperationCancelled event of the ProgressScreen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ProgressScreen_OperationCancelled(object sender, EventArgs e) => _progressCancelAction?.Invoke();

        /// <summary>
        /// Handles the WaitPanelDeactivated event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DataContext_WaitPanelDeactivated(object sender, EventArgs e) => WaitScreen.Visible = false;

        /// <summary>
        /// Handles the WaitPanelActivated event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="WaitPanelActivateArgs"/> instance containing the event data.</param>
        private void DataContext_WaitPanelActivated(object sender, WaitPanelActivateArgs e)
        {
            WaitScreen.WaitMessage = e.Message;

            if (!string.IsNullOrWhiteSpace(e.Title))
            {
                WaitScreen.WaitTitle = e.Title;
            }

            if (WaitScreen.Visible)
            {
                return;
            }

            WaitScreen.Visible = true;
            WaitScreen.BringToFront();
            WaitScreen.Invalidate();
        }

        /// <summary>
        /// Handles the PropertyChanging event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IMain.CurrentProject):
                    if (DataContext.CurrentProject?.FileExplorer != null)
                    {
                        DataContext.CurrentProject.FileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the FileExplorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void FileExplorer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorerVm.SelectedNode):                    
                    ValidateRibbonButtons();
                    break;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IMain.Text):
                    Text = DataContext.Text;
                    break;
                case nameof(IMain.CurrentProject):
                    NavigateToProjectView(DataContext);
                    StageLauncher.ShowBackButton = DataContext.CurrentProject != null;

                    if (DataContext.CurrentProject?.FileExplorer != null)
                    {
                        DataContext.CurrentProject.FileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
                    }

                    break;
            }

            ValidateRibbonButtons();
        }

        /// <summary>
        /// Function to initialize the form from the data context.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        private void InitializeFromDataContext(IMain dataContext)
        {
            if (dataContext == null)
            {
                Text = Resources.GOREDIT_CAPTION_NO_FILE;
                StageLauncher.ShowBackButton = false;
                return;
            }

            Text = dataContext.Text;
            StageLauncher.ShowBackButton = dataContext.CurrentProject != null;
        }

        /// <summary>
        /// Function to unassign the events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            _progressCancelAction = null;
            ProgressScreen.OperationCancelled -= ProgressScreen_OperationCancelled;

            if (DataContext == null)
            {
                return;
            }

            if (DataContext.CurrentProject?.FileExplorer != null)
            {
                DataContext.CurrentProject.FileExplorer.PropertyChanged -= FileExplorer_PropertyChanged;
            }

            DataContext.ProgressUpdated -= DataContext_ProgressUpdated;
            DataContext.ProgressDeactivated -= DataContext_ProgressDeactivated;
            DataContext.WaitPanelActivated -= DataContext_WaitPanelActivated;
            DataContext.WaitPanelDeactivated -= DataContext_WaitPanelDeactivated;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemNewDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemNewDirectory_Click(object sender, EventArgs e)
        {
            try
            {
                PanelProject.FileExplorer.CreateDirectory();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemRename control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemRename_Click(object sender, EventArgs e)
        {
            try
            {
                PanelProject.FileExplorer.RenameNode();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonFileSystemDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFileSystemDelete_Click(object sender, EventArgs e)
        {
            try
            {
                PanelProject.FileExplorer.Delete();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonExpand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonExpand_Click(object sender, EventArgs e)
        {
            try
            {
                PanelProject.FileExplorer.Expand();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the ButtonCollapse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonCollapse_Click(object sender, EventArgs e)
        {
            try
            {
                PanelProject.FileExplorer.Collapse();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Handles the Click event of the CheckShowAllFiles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckShowAllFiles_Click(object sender, EventArgs e)
        {
            try
            {
                if (DataContext?.CurrentProject == null)
                {
                    return;
                }

                DataContext.CurrentProject.ShowExternalItems = CheckShowAllFiles.Checked;
                PanelProject.FileExplorer.RefreshFileSystem();
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DataContext?.OnLoad();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data. </param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            DataContext?.OnUnload();
        }

        /// <summary>
        /// Function to assign a data context to the view.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        public void SetDataContext(IMain dataContext)
        {
            try
            {
                UnassignEvents();

                // Assign the data context to the new view.
                StageLauncher.StageNewProject.SetDataContext(dataContext.NewProject);

                InitializeFromDataContext(dataContext);
                DataContext = dataContext;

                if (DataContext == null)
                {
                    return;
                }

                if (DataContext.CurrentProject != null)
                {
                    DataContext.CurrentProject.FileExplorer.PropertyChanged += FileExplorer_PropertyChanged;
                }

                DataContext.ProgressUpdated += DataContext_ProgressUpdated;
                DataContext.ProgressDeactivated += DataContext_ProgressDeactivated;
                DataContext.WaitPanelActivated += DataContext_WaitPanelActivated;
                DataContext.WaitPanelDeactivated += DataContext_WaitPanelDeactivated;
                DataContext.PropertyChanged += DataContext_PropertyChanged;
                DataContext.PropertyChanging += DataContext_PropertyChanging;
            }
            finally
            {
                ValidateRibbonButtons();
            }
        }

        /// <summary>
        /// Function to load the theme for the window.
        /// </summary>
        public void LoadTheme()
        {
            // Default to our dark theme. We may have more later.
            /*byte[] theme = Encoding.UTF8.GetBytes(Resources.Krypton_DarkO2k10Theme);
            AppPalette.SuspendUpdates();
            AppPalette.Import(theme);
            AppPalette.ResumeUpdates(true);*/
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormMain"/> class.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            StageLauncher.StageNewProject.ProjectCreated += StageNewProject_ProjectCreated;
            _syncContext = SynchronizationContext.Current;
        }
        #endregion
    }
}
