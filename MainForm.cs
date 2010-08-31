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
        private Regex modPattern = new Regex("^Mod=([\\w-]*)", RegexOptions.Multiline);
        private Regex readmePathPattern = new Regex(@"^[/\\]?(([\w.])+[/\\])*([\w.]*)?\r?$", RegexOptions.Multiline);
        private Boolean settingsChanged = false;
        private string currentMod = "";
        private Dictionary<string, bool> fullyInstalled = new Dictionary<string, bool>();

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
            InstalledLB.DrawItem += new DrawItemEventHandler(InstalledLB_DrawItem);
            InstalledLB.DoubleClick += new EventHandler(InstalledLB_DoubleClick);
        }

        private void loadSettings() {
            try {
                TextReader tr = new StreamReader(Application.UserAppDataPath + "\\driftmoonpath.txt");
                string driftmoondir = tr.ReadLine();
                tr.Close();
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
            addLog("Searching for mods...");
            refreshModList();
            currentMod = getCurrentMod();
            addLog("Found all mods.");
        }

        private string getCurrentMod() {
            TextReader tr = new StreamReader(InstallDirT.Text + "\\options.ini");
            string options = tr.ReadToEnd();
            tr.Close();
            Match m = modPattern.Match(options);
            string mod = m.Groups[1].Value;
            Match m2 = m.NextMatch();
            if (m2.Success) {
                throw new ApplicationException("Multiple mods defined in options.ini, fix this first please.");
            }
            return mod;
        }

        private bool isCurrentMod(string mod) {
            if (currentMod == null) {
                currentMod = getCurrentMod();
            }
            return currentMod.Equals(mod);
        }

        private void refreshModList() {
            string[] dirs = Directory.GetDirectories(InstallDirT.Text);
            foreach (string dir in dirs) {
                string lastpart = dir.Substring(dir.LastIndexOf("\\") + 1);
                if (lastpart != "ui" && lastpart != "data") {
                    fullyInstalled[lastpart] = true;
                }
            }
            InstalledLB.Items.Clear();
            InstalledLB.Items.AddRange(fullyInstalled.Keys.ToArray());
        }

        private bool isFullyInstalled(string mod) {
            return fullyInstalled[mod];
        }

        private void InstalledLB_DrawItem(object sender,
    System.Windows.Forms.DrawItemEventArgs e) {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            
            // Define the default color of the brush as black.
            Brush drawBrush = Brushes.Black;
            Font drawFont = e.Font;


            string mod = (string)InstalledLB.Items[e.Index];
            if (! isFullyInstalled(mod)) {
               drawBrush = Brushes.Gray;
            }
            if (isCurrentMod(mod)) {
                drawFont = new Font(drawFont, FontStyle.Bold);
            }

            e.Graphics.DrawString(InstalledLB.Items[e.Index].ToString(),
                drawFont, drawBrush, e.Bounds, StringFormat.GenericDefault);
            e.DrawFocusRectangle();
        }


        private void findDriftmoonDir() {
            string d = ProgramFilesx86() + "\\Driftmoon";
            setDriftmoonDir(d);
        }

        private void setDriftmoonDir(string d) {
            clearLog();
            if (isDriftmoonDir(d)) {
                addLog("Found Driftmoon directory...");
                InstallDirT.Text = d;
                refreshMods();
            } else {
                MessageBox.Show("No Driftmoon directory found in " + d +
                    ", please select a valid Driftmoon directory", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        void InstalledLB_DoubleClick(object sender, EventArgs e) {
            string newMod = (string)InstalledLB.SelectedItem;
            if (currentMod == newMod)
                return;

            changeMod(newMod);

            refreshMods();
        }

        private void changeMod(string newMod) {
            addLog("Setting current mod to " + newMod);
            TextReader tr = new StreamReader(InstallDirT.Text + "\\options.ini");
            List<string> lines = new List<string>();
            string l = tr.ReadLine();
            while (l != null) {
                lines.Add(l);
                l = tr.ReadLine();
            }
            tr.Close();

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
                installMod(d);
            }
        }

        private void installMod(string d) {
            string realmod = d;
            if (!isModDir(d)) {
                string[] subdirs = Directory.GetDirectories(d);
                if (subdirs.Length == 1) {
                    addLog("No mod found here, trying subdirectory...");
                    realmod = subdirs[0];
                } else {
                    MessageBox.Show("No mod found in this directory, sorry!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            addLog("Trying to install from " + realmod);
            string newMod = realmod.Substring(realmod.LastIndexOf("\\") + 1);

            //TODO: what if mod already installed?
            try {
                PleaseWait popup = new PleaseWait();
                int posx = this.Location.X + (this.Size.Width - popup.Size.Width) / 2;
                int posy = this.Location.Y + (this.Size.Height - popup.Size.Height) / 2;
                popup.Show();
                popup.Location = new Point(posx, posy);
                popup.Update();
                DirectoryCopy(realmod, InstallDirT.Text + "\\" + newMod);
                installDependencies(d, newMod);
                installDependencies(InstallDirT.Text + "\\" + newMod, newMod);
                changeMod(newMod);
                addLog("Succeeded installing " + newMod + "!");
                refreshMods();
                popup.Hide();
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show("Windows told me: \"" + ex.Message + "\" Perhaps try running as administrator?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool isModDir(string path) {
            return Directory.Exists(path) && Directory.Exists(path + "\\script");
        }

        private void installDependencies(string searchpath, string destinationMod) {
            List<string> toInstall = gatherDependencies(searchpath);
            //TODO: might want to make some sort of abstract 'Tasks' of the copy jobs to make a progress bar
            string basedir = InstallDirT.Text;
            List<string> errors = new List<string>();
            foreach (string s in toInstall) {
                string from = basedir + "\\" + s;
                string to = basedir + "\\" + destinationMod + "\\" + s.Substring("mainmod\\".Length);
                addLog(from + " --> " + to);
                if (File.Exists(from)) {
                    if (File.Exists(to)) {
                        addLog("Destination file already exists, skipping...");
                    }
                    File.Copy(from, to, false);
                } else {
                    if (Directory.Exists(from)) {
                        if (Directory.Exists(to)) {
                            addLog("Destination directory already exists, merging but skipping existing files...");
                        }
                        DirectoryCopy(from, to);
                    } else {
                        addLog("Source file missing!");
                        errors.Add("The following dependency was not found and could not be copied: " + from);
                    }
                }
            }
            if (errors.Count > 0) {
                string errormsg = string.Join(Environment.NewLine, errors.ToArray());
                MessageBox.Show(errormsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> gatherDependencies(string moddir) {
            //paths returned do not include "Driftmoon", starting nor trailing slashes
            List<string> deps = new List<string>();
            DirectoryInfo di = new DirectoryInfo(moddir);
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis) {
                string s = fi.Name.ToLower();
                if (s.Contains("readme")) {
                    TextReader tr = fi.OpenText();
                    string contents = tr.ReadToEnd();
                    tr.Close();
                    MatchCollection matches = readmePathPattern.Matches(contents);
                    if (matches.Count != 0) {
                        addLog("Found dependencies in " + s + ", installing...");
                    }
                    foreach (Match match in matches) {
                        string path = match.ToString();
                        if (path.EndsWith("\r")) {
                            path = path.Remove(path.Length - 1);
                        }
                        path = path.Replace("/", "\\");
                        if (path.StartsWith("\\")) {
                            path = path.Substring(path.Length - 1);
                        }
                        if (path.StartsWith("Driftmoon\\")) {
                            path = path.Substring("Driftmoon\\".Length);
                        }
                        if (path.EndsWith("\\")) {
                            path = path.Remove(path.Length - 1);
                        }
                        if (!path.Equals("")) {
                            deps.Add(path);
                        }
                    }
                }
            }
            return deps;
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

        private void clearLog() {
            LogT.Text = "";
        }

        private void addLog(string logtxt) {
            LogT.AppendText(logtxt + Environment.NewLine);
        }

        private void RefreshListB_Click(object sender, EventArgs e) {
            refreshMods();
        }
    }
}
