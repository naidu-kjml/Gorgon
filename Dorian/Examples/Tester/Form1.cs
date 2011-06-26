﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Graphics;
using GorgonLibrary.Input;

namespace Tester
{
	public partial class Form1 : Form
	{
		GorgonInputFactory input = null;
		GorgonPointingDevice mouse = null;

		private bool Idle(GorgonFrameRate timing)
		{
			labelMouse.Text = mouse.Position.X.ToString() + "x" + mouse.Position.Y.ToString();
			return true;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				Gorgon.Initialize(this);				
				GorgonPlugInFactory.LoadPlugInAssembly(@"..\..\..\..\PlugIns\Gorgon.RawInput\bin\Debug\Gorgon.RawInput.dll");
				input = GorgonInputFactory.CreateFactory("GorgonLibrary.Input.GorgonRawInput");
				mouse = input.CreatePointingDevice();
				Gorgon.Go(Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			mouse.Dispose();
			Gorgon.Terminate();
		}

		public Form1()
		{
			InitializeComponent();
		}
	}
}
