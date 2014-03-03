﻿using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.FontEditorPlugIn.Controls
{
	partial class panelGradient
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panelBrushGradient = new System.Windows.Forms.Panel();
			this.panelGradientDisplay = new System.Windows.Forms.Panel();
			this.panelGradControls = new System.Windows.Forms.Panel();
			this.labelAngle = new System.Windows.Forms.Label();
			this.numericAngle = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.checkScaleAngle = new System.Windows.Forms.CheckBox();
			this.checkUseGamma = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label5 = new System.Windows.Forms.Label();
			this.panelSelectedColor = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.numericSelectedWeight = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.comboWrapMode = new System.Windows.Forms.ComboBox();
			this.panelPreview = new System.Windows.Forms.Panel();
			this.panelBrushGradient.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAngle)).BeginInit();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericSelectedWeight)).BeginInit();
			this.SuspendLayout();
			// 
			// panelBrushGradient
			// 
			this.panelBrushGradient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.panelBrushGradient.Controls.Add(this.panelGradientDisplay);
			this.panelBrushGradient.Controls.Add(this.panelGradControls);
			this.panelBrushGradient.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelBrushGradient.Location = new System.Drawing.Point(0, 0);
			this.panelBrushGradient.Name = "panelBrushGradient";
			this.panelBrushGradient.Size = new System.Drawing.Size(530, 86);
			this.panelBrushGradient.TabIndex = 0;
			// 
			// panelGradientDisplay
			// 
			this.panelGradientDisplay.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
			this.panelGradientDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelGradientDisplay.Location = new System.Drawing.Point(0, 0);
			this.panelGradientDisplay.Margin = new System.Windows.Forms.Padding(0);
			this.panelGradientDisplay.Name = "panelGradientDisplay";
			this.panelGradientDisplay.Size = new System.Drawing.Size(530, 70);
			this.panelGradientDisplay.TabIndex = 0;
			this.panelGradientDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.panelGradientDisplay_Paint);
			this.panelGradientDisplay.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelGradientDisplay_MouseDoubleClick);
			// 
			// panelGradControls
			// 
			this.panelGradControls.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelGradControls.Location = new System.Drawing.Point(0, 70);
			this.panelGradControls.Margin = new System.Windows.Forms.Padding(0);
			this.panelGradControls.Name = "panelGradControls";
			this.panelGradControls.Size = new System.Drawing.Size(530, 16);
			this.panelGradControls.TabIndex = 1;
			this.panelGradControls.Paint += new System.Windows.Forms.PaintEventHandler(this.panelBrushPreview_Paint);
			this.panelGradControls.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelBrushPreview_MouseDoubleClick);
			this.panelGradControls.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelBrushPreview_MouseDown);
			this.panelGradControls.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelBrushPreview_MouseMove);
			// 
			// labelAngle
			// 
			this.labelAngle.AutoSize = true;
			this.labelAngle.Location = new System.Drawing.Point(6, 9);
			this.labelAngle.Name = "labelAngle";
			this.labelAngle.Size = new System.Drawing.Size(37, 13);
			this.labelAngle.TabIndex = 1;
			this.labelAngle.Text = "Angle:";
			// 
			// numericAngle
			// 
			this.numericAngle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericAngle.DecimalPlaces = 1;
			this.numericAngle.Location = new System.Drawing.Point(49, 7);
			this.numericAngle.Maximum = new decimal(new int[] {
            3599,
            0,
            0,
            65536});
			this.numericAngle.Name = "numericAngle";
			this.numericAngle.Size = new System.Drawing.Size(52, 20);
			this.numericAngle.TabIndex = 0;
			this.numericAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericAngle.ValueChanged += new System.EventHandler(this.numericAngle_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(103, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(11, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "°";
			// 
			// checkScaleAngle
			// 
			this.checkScaleAngle.AutoSize = true;
			this.checkScaleAngle.Location = new System.Drawing.Point(9, 33);
			this.checkScaleAngle.Name = "checkScaleAngle";
			this.checkScaleAngle.Size = new System.Drawing.Size(88, 17);
			this.checkScaleAngle.TabIndex = 1;
			this.checkScaleAngle.Text = "Scale angle?";
			this.checkScaleAngle.UseVisualStyleBackColor = true;
			this.checkScaleAngle.Click += new System.EventHandler(this.checkScaleAngle_Click);
			// 
			// checkUseGamma
			// 
			this.checkUseGamma.AutoSize = true;
			this.checkUseGamma.Location = new System.Drawing.Point(9, 56);
			this.checkUseGamma.Name = "checkUseGamma";
			this.checkUseGamma.Size = new System.Drawing.Size(140, 17);
			this.checkUseGamma.TabIndex = 2;
			this.checkUseGamma.Text = "Use Gamma correction?";
			this.checkUseGamma.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.label5);
			this.panel1.Controls.Add(this.panelSelectedColor);
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.numericSelectedWeight);
			this.panel1.Controls.Add(this.label3);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.comboWrapMode);
			this.panel1.Controls.Add(this.labelAngle);
			this.panel1.Controls.Add(this.checkUseGamma);
			this.panel1.Controls.Add(this.numericAngle);
			this.panel1.Controls.Add(this.checkScaleAngle);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.panelPreview);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 86);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(530, 122);
			this.panel1.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(393, 8);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "Preview:";
			// 
			// panelSelectedColor
			// 
			this.panelSelectedColor.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
			this.panelSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSelectedColor.Location = new System.Drawing.Point(282, 32);
			this.panelSelectedColor.Name = "panelSelectedColor";
			this.panelSelectedColor.Size = new System.Drawing.Size(46, 20);
			this.panelSelectedColor.TabIndex = 5;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(187, 32);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(79, 13);
			this.label4.TabIndex = 10;
			this.label4.Text = "Selected Color:";
			// 
			// numericSelectedWeight
			// 
			this.numericSelectedWeight.DecimalPlaces = 1;
			this.numericSelectedWeight.Enabled = false;
			this.numericSelectedWeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericSelectedWeight.Location = new System.Drawing.Point(282, 6);
			this.numericSelectedWeight.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericSelectedWeight.Name = "numericSelectedWeight";
			this.numericSelectedWeight.Size = new System.Drawing.Size(46, 20);
			this.numericSelectedWeight.TabIndex = 4;
			this.numericSelectedWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(187, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(89, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Selected Weight:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 76);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Wrap Mode:";
			// 
			// comboWrapMode
			// 
			this.comboWrapMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboWrapMode.FormattingEnabled = true;
			this.comboWrapMode.Location = new System.Drawing.Point(9, 92);
			this.comboWrapMode.Name = "comboWrapMode";
			this.comboWrapMode.Size = new System.Drawing.Size(140, 21);
			this.comboWrapMode.TabIndex = 3;
			// 
			// panelPreview
			// 
			this.panelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.panelPreview.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
			this.panelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelPreview.Location = new System.Drawing.Point(396, 24);
			this.panelPreview.Name = "panelPreview";
			this.panelPreview.Size = new System.Drawing.Size(95, 95);
			this.panelPreview.TabIndex = 6;
			this.panelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.panelPreview_Paint);
			// 
			// panelGradient
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelBrushGradient);
			this.Controls.Add(this.panel1);
			this.Name = "panelGradient";
			this.Size = new System.Drawing.Size(530, 208);
			this.panelBrushGradient.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericAngle)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericSelectedWeight)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelBrushGradient;
		private System.Windows.Forms.Label labelAngle;
		private System.Windows.Forms.NumericUpDown numericAngle;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkScaleAngle;
		private System.Windows.Forms.CheckBox checkUseGamma;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboWrapMode;
		private System.Windows.Forms.Panel panelGradientDisplay;
		private System.Windows.Forms.Panel panelGradControls;
		private System.Windows.Forms.Panel panelSelectedColor;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numericSelectedWeight;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panelPreview;
		private System.Windows.Forms.Label label5;
	}
}
