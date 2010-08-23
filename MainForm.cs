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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;

namespace driftmoon_mod_switcher {
    public partial class MainForm : Form {
        private Regex modPattern = new Regex("^Mod=(\\w*)", RegexOptions.Multiline);
        private Boolean settingsChanged = false;

        public MainForm() {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e) {
            try {
                loadSettings();
            } catch (ApplicationException) {
                findDriftmoonDir();
                settingsChanged = true;
            }
        }

        private void loadSettings() {
            try {
                TextReader tr = new StreamReader(Application.UserAppDataPath + "\\driftmoonpath.txt");
                string driftmoondir = tr.ReadLine();
                tr.Close();
                if (!isDriftmoonDir(driftmoondir))
                    throw new ApplicationException("No valid settings found");
                setDriftmoonDir(driftmoondir);
            } catch (IOException) {
                throw new ApplicationException("No settings found");
            }
        }

        private void saveSettings() {
            if (!settingsChanged)
                return;
            TextWriter tw = new StreamWriter(Application.UserAppDataPath + "\\driftmoonpath.txt");
            tw.WriteLine(InstallDirT.Text);
            tw.Close();
        }

        private void refreshMods() {
            refreshModList();
            refreshCurrentMod();
        }

        private void refreshCurrentMod() {
            TextReader tr = new StreamReader(InstallDirT.Text + "\\options.ini");
            string options = tr.ReadToEnd();
            tr.Close();
            Match m = modPattern.Match(options);
            string mod = m.Groups[1].Value;
            Match m2 = m.NextMatch();
            if (m2.Success) {
                MessageBox.Show("There are multiple mods defined in options.ini, please fix this before I break something. I am clumsy.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            CurrentModT.Text = mod;
            InstalledLB.SelectedItem = mod;
        }

        private void refreshModList() {
            string[] dirs = Directory.GetDirectories(InstallDirT.Text);
            List<string> mods = new List<string>();
            foreach (string dir in dirs) {
                string lastpart = dir.Substring(dir.LastIndexOf("\\") + 1);
                if (lastpart != "ui" && lastpart != "data") {
                    mods.Add(lastpart);
                }
            }
            InstalledLB.Items.Clear();
            InstalledLB.Items.AddRange(mods.ToArray());
        }

        private void findDriftmoonDir() {
            string d = ProgramFilesx86() + "\\Driftmoon";
            setDriftmoonDir(d);
        }

        private void setDriftmoonDir(string d){
            if (isDriftmoonDir(d)) {
                InstallDirT.Text = d;
                refreshMods();
            }
        }

        private bool isDriftmoonDir(string path) {
            return Directory.Exists(path) && File.Exists(path + "\\Driftmoon.exe")
                && File.Exists(path + "\\options.ini");
        }

        private void InstallDirB_Click(object sender, EventArgs e) {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.Description = "Select the Driftmoon install directory";
            f.ShowNewFolderButton = false;
            DialogResult result = f.ShowDialog();
            if (result == DialogResult.OK) {
                string d = f.SelectedPath;
                if (isDriftmoonDir(d)) {
                    settingsChanged = true;
                    InstallDirT.Text = d;
                    refreshMods();
                } else {
                    MessageBox.Show("This is not a valid Driftmoon install directory!",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CurrentModB_Click(object sender, EventArgs e) {
            if (InstalledLB.SelectedItem == null)
                return;
            string newMod = (string)InstalledLB.SelectedItem;
            if (CurrentModT.Text == newMod)
                return;

            changeMod(newMod);

            refreshCurrentMod();
        }

        private void changeMod(string newMod) {
            TextReader tr = new StreamReader(InstallDirT.Text + "\\options.ini");
            string options = tr.ReadToEnd();
            tr.Close();
            string[] lines = options.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            try {
                TextWriter tw = new StreamWriter(InstallDirT.Text + "\\options.ini");
                foreach (string line in lines) {
                    Match m = modPattern.Match(line);
                    if (m.Success) {
                        //FIXME: if no line "Mod=" exists, no new will be written?
                        tw.WriteLine("Mod=" + newMod);
                    } else {
                        tw.WriteLine(line);
                    }
                }
                tw.Close();
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show("Windows told me: \"" + ex.Message + "\" Perhaps try running as administrator?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InstallModB_Click(object sender, EventArgs e) {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.Description = "Select the extracted mod directory to install";
            f.ShowNewFolderButton = false;
            DialogResult result = f.ShowDialog();
            if (result == DialogResult.OK) {
                string d = f.SelectedPath;
                //FIXME: do a sanity check on the directory being a mod
                //TODO: what if mod already installed?
                try {
                    DirectoryCopy(d, InstallDirT.Text + d.Substring(d.LastIndexOf("\\")));
                    changeMod(d.Substring(d.LastIndexOf("\\") + 1));
                    refreshMods();
                } catch (UnauthorizedAccessException ex) {
                    MessageBox.Show("Windows told me: \"" + ex.Message + "\" Perhaps try running as administrator?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void DirectoryCopy(
            string sourceDirName, string destDirName) {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files) {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (DirectoryInfo subdir in dirs) {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath);
            }
        }

        static string ProgramFilesx86() {
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))) {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            saveSettings();
        }

    }
}
