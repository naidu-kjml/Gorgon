﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.FileSystem;

namespace Tester_Graphics
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//GorgonDialogs.InfoBox(null, "Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=");
			//GorgonDialogs.WarningBox(null, "Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=", "Test 123\nTest456");
			//GorgonDialogs.ErrorBox(null, "Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=", "Test 123\nTest456");
			//GorgonDialogs.ConfirmBox(null, "Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=", false, false);
			//GorgonDialogs.ConfirmBox(null, "Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=", true, false);
			//GorgonDialogs.ConfirmBox(null, "Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=Fuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=\nFuck you ABCDEFGHIJKLMNOPQSTUVWXYZ 1234567890 !@#$%^&*()_+-=", true, true);

			GorgonFileSystem fs = new GorgonFileSystem();
			GorgonFileSystemOpenFile dialog = null;

			fs.Mount(@"x:\unpak\");

			dialog = new GorgonFileSystemOpenFile(fs);
			dialog.Filter = "WinZip files (*.zip)|*.zip;*.zeep|All the files (*.*)|*.*";
			dialog.FilterIndex = 1;
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				GorgonDialogs.InfoBox(null, string.Join("\n", dialog.Filenames));
			}

			return;
			Gorgon.Run(new Form1());
		}
	}
}
