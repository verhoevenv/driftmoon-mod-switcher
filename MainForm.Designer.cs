/*
Copyright 2010 Vincent Verhoeven

This file is part of driftmoon-mod-switcher.

driftmoon-mod-switcher is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

driftmoon-mod-switcher is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with driftmoon-mod-switcher.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace driftmoon_mod_switcher
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.InstalledLB = new System.Windows.Forms.ListBox();
            this.InstalledL = new System.Windows.Forms.Label();
            this.InstallDirL = new System.Windows.Forms.Label();
            this.InstallDirT = new System.Windows.Forms.TextBox();
            this.InstallDirB = new System.Windows.Forms.Button();
            this.CurrentModL = new System.Windows.Forms.Label();
            this.CurrentModT = new System.Windows.Forms.TextBox();
            this.CurrentModB = new System.Windows.Forms.Button();
            this.InstallFromZipB = new System.Windows.Forms.Button();
            this.LogT = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // InstalledLB
            // 
            this.InstalledLB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.InstalledLB.ColumnWidth = 20;
            this.InstalledLB.FormattingEnabled = true;
            this.InstalledLB.Location = new System.Drawing.Point(12, 50);
            this.InstalledLB.Name = "InstalledLB";
            this.InstalledLB.Size = new System.Drawing.Size(439, 134);
            this.InstalledLB.TabIndex = 0;
            // 
            // InstalledL
            // 
            this.InstalledL.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.InstalledL.AutoSize = true;
            this.InstalledL.Location = new System.Drawing.Point(177, 34);
            this.InstalledL.Name = "InstalledL";
            this.InstalledL.Size = new System.Drawing.Size(104, 13);
            this.InstalledL.TabIndex = 1;
            this.InstalledL.Text = "Installed mods found";
            // 
            // InstallDirL
            // 
            this.InstallDirL.AutoSize = true;
            this.InstallDirL.Location = new System.Drawing.Point(9, 14);
            this.InstallDirL.Name = "InstallDirL";
            this.InstallDirL.Size = new System.Drawing.Size(124, 13);
            this.InstallDirL.TabIndex = 2;
            this.InstallDirL.Text = "Driftmoon install directory";
            // 
            // InstallDirT
            // 
            this.InstallDirT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.InstallDirT.Enabled = false;
            this.InstallDirT.Location = new System.Drawing.Point(139, 11);
            this.InstallDirT.Name = "InstallDirT";
            this.InstallDirT.Size = new System.Drawing.Size(231, 20);
            this.InstallDirT.TabIndex = 3;
            // 
            // InstallDirB
            // 
            this.InstallDirB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InstallDirB.Location = new System.Drawing.Point(376, 9);
            this.InstallDirB.Name = "InstallDirB";
            this.InstallDirB.Size = new System.Drawing.Size(75, 23);
            this.InstallDirB.TabIndex = 4;
            this.InstallDirB.Text = "Change";
            this.InstallDirB.UseVisualStyleBackColor = true;
            this.InstallDirB.Click += new System.EventHandler(this.InstallDirB_Click);
            // 
            // CurrentModL
            // 
            this.CurrentModL.AutoSize = true;
            this.CurrentModL.Location = new System.Drawing.Point(12, 207);
            this.CurrentModL.Name = "CurrentModL";
            this.CurrentModL.Size = new System.Drawing.Size(67, 13);
            this.CurrentModL.TabIndex = 5;
            this.CurrentModL.Text = "Current mod:";
            // 
            // CurrentModT
            // 
            this.CurrentModT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.CurrentModT.Enabled = false;
            this.CurrentModT.Location = new System.Drawing.Point(85, 204);
            this.CurrentModT.Name = "CurrentModT";
            this.CurrentModT.Size = new System.Drawing.Size(243, 20);
            this.CurrentModT.TabIndex = 6;
            // 
            // CurrentModB
            // 
            this.CurrentModB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CurrentModB.Location = new System.Drawing.Point(334, 202);
            this.CurrentModB.Name = "CurrentModB";
            this.CurrentModB.Size = new System.Drawing.Size(117, 23);
            this.CurrentModB.TabIndex = 7;
            this.CurrentModB.Text = "Use selected";
            this.CurrentModB.UseVisualStyleBackColor = true;
            this.CurrentModB.Click += new System.EventHandler(this.CurrentModB_Click);
            // 
            // InstallFromZipB
            // 
            this.InstallFromZipB.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.InstallFromZipB.Location = new System.Drawing.Point(132, 230);
            this.InstallFromZipB.Name = "InstallFromZipB";
            this.InstallFromZipB.Size = new System.Drawing.Size(194, 23);
            this.InstallFromZipB.TabIndex = 8;
            this.InstallFromZipB.Text = "Install downloaded mod from directory";
            this.InstallFromZipB.UseVisualStyleBackColor = true;
            this.InstallFromZipB.Click += new System.EventHandler(this.InstallModB_Click);
            // 
            // LogT
            // 
            this.LogT.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LogT.Location = new System.Drawing.Point(12, 259);
            this.LogT.Multiline = true;
            this.LogT.Name = "LogT";
            this.LogT.ReadOnly = true;
            this.LogT.Size = new System.Drawing.Size(438, 78);
            this.LogT.TabIndex = 9;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 349);
            this.Controls.Add(this.LogT);
            this.Controls.Add(this.InstallFromZipB);
            this.Controls.Add(this.CurrentModB);
            this.Controls.Add(this.CurrentModT);
            this.Controls.Add(this.CurrentModL);
            this.Controls.Add(this.InstallDirB);
            this.Controls.Add(this.InstallDirT);
            this.Controls.Add(this.InstallDirL);
            this.Controls.Add(this.InstalledL);
            this.Controls.Add(this.InstalledLB);
            this.Name = "MainForm";
            this.Text = "Driftmoon Mod Switcher";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox InstalledLB;
        private System.Windows.Forms.Label InstalledL;
        private System.Windows.Forms.Label InstallDirL;
        private System.Windows.Forms.TextBox InstallDirT;
        private System.Windows.Forms.Button InstallDirB;
        private System.Windows.Forms.Label CurrentModL;
        private System.Windows.Forms.TextBox CurrentModT;
        private System.Windows.Forms.Button CurrentModB;
        private System.Windows.Forms.Button InstallFromZipB;
        private System.Windows.Forms.TextBox LogT;
    }
}

