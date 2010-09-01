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
        private Dictionary<string, installStatus> fullyInstalled = new Dictionary<string, installStatus>();
        private enum installStatus { OK, NOT_INSTALLED, DEPENDENCY_BROKEN, INSTALLABLE_FROM_HERE, NON_MOD_DIR };

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
                    if (isModDir(dir)) {
                        try {
                            List<FileInfo> files = gatherDependencies(dir);
                            bool installed = true;
                            foreach (FileInfo f in files) {
                                string dest = dependencyToDestination(f, lastpart);
                                if (!File.Exists(dest)) {
                                    installed = false;
                                    break;
                                }
                            }
                            if (installed) {
                                fullyInstalled[lastpart] = installStatus.OK;
                            } else {
                                addLog("Dependencies for " + lastpart + " seem to be incomplete");
                                fullyInstalled[lastpart] = installStatus.NOT_INSTALLED;
                            }
                        } catch (DependencyBrokenException) {
                            fullyInstalled[lastpart] = installStatus.DEPENDENCY_BROKEN;
                        }
                    } else {
                        if (isInstallableDir(dir)) {
                            fullyInstalled[lastpart] = installStatus.INSTALLABLE_FROM_HERE;
                        } else {
                            fullyInstalled[lastpart] = installStatus.NON_MOD_DIR;
                        }
                    }
                }
            }
            InstalledLB.Items.Clear();
            InstalledLB.Items.AddRange(fullyInstalled.Keys.ToArray());
        }

        private installStatus getInstallStatus(string mod) {
            return fullyInstalled[mod];
        }

        private void InstalledLB_DrawItem(object sender,
    System.Windows.Forms.DrawItemEventArgs e) {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            
            // Define the default color of the brush as black.
            Brush drawBrush = Brushes.Black;
            Font drawFont = e.Font;

            if (e.Index == -1)
                return;

            string mod = (string)InstalledLB.Items[e.Index];
            switch(getInstallStatus(mod)) {
                case installStatus.NOT_INSTALLED:
                    drawBrush = Brushes.Gray;
                    break;
                case installStatus.DEPENDENCY_BROKEN:
                    drawBrush = Brushes.Red;
                    break;
                case installStatus.NON_MOD_DIR:
                    drawBrush = Brushes.Purple;
                    break;
                case installStatus.INSTALLABLE_FROM_HERE:
                    drawBrush = Brushes.Green;
                    break;
                case installStatus.OK:
                    drawBrush = Brushes.Black;
                    break;
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
            switch (getInstallStatus(newMod)) {
                case installStatus.NOT_INSTALLED:
                    installDependencies(newMod);
                    setMod(newMod);
                    break;
                case installStatus.DEPENDENCY_BROKEN:
                    MessageBox.Show("This mod has a dependency that couldn't be found. I won't let you play around with it.", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case installStatus.NON_MOD_DIR:
                    MessageBox.Show("So this directory is a random unclassifiable directory unrelated to Driftmoon. I have no idea how you managed to put this thing here, but you can be proud of yourself. Nothing much more you can do with it though.", "Congratulations", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case installStatus.INSTALLABLE_FROM_HERE:
                    installMod(InstallDirT.Text + "\\" + newMod);
                    break;
                case installStatus.OK:
                    setMod(newMod);
                    break;
            }
        }

        private void setMod(string newMod) {
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
            if (!isInstallableDir(d)) {
                MessageBox.Show("No mod found in this directory, sorry!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!isModDir(d)) {
                string[] subdirs = Directory.GetDirectories(d);
                realmod = subdirs[0];
                addLog("No mod found here, going to subdirectory.");
            }
            addLog("Trying to install from " + realmod);
            string newMod = realmod.Substring(realmod.LastIndexOf("\\") + 1);

            //TODO: what if mod already installed?
            PleaseWait popup = new PleaseWait();
            try {
                int posx = this.Location.X + (this.Size.Width - popup.Size.Width) / 2;
                int posy = this.Location.Y + (this.Size.Height - popup.Size.Height) / 2;
                popup.Show();
                popup.Location = new Point(posx, posy);
                popup.Update();
                string newModDir = InstallDirT.Text + "\\" + newMod;
                DirectoryCopy(realmod, newModDir);
                foreach (FileInfo f in findReadmes(d)) {
                    f.CopyTo(Path.Combine(newModDir,f.Name),true);
                }
                installDependencies(newMod);
                setMod(newMod);
                addLog("Succeeded installing " + newMod + "!");
                refreshMods();
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show("Windows told me: \"" + ex.Message + "\" Perhaps try running as administrator?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                popup.Hide();
            }
        }

        private bool isModDir(string path) {
            return Directory.Exists(path) && Directory.Exists(path + "\\maps");
        }

        private bool isInstallableDir(string path) {
            if (!Directory.Exists(path))
                return false;
            if (isModDir(path))
                return true;
            string[] subdirs = Directory.GetDirectories(path);
            if (subdirs.Length == 1) {
                string realmod = subdirs[0];
                return isModDir(realmod);
            } else {
                return false;
            }
        }

        private void installDependencies(string destinationMod) {
            string searchpath = InstallDirT.Text + "\\" + destinationMod;
            List<FileInfo> toInstall = gatherDependencies(searchpath);
            //TODO: make a progress bar
            foreach (FileInfo s in toInstall) {
                string to = dependencyToDestination(s, destinationMod);
                addLog(s.FullName + " --> " + to);
                if (File.Exists(to)) {
                    addLog("Destination file already exists, skipping...");
                }
                copyRelativeFileCreateDir(s, InstallDirT.Text + "\\mainmod", Path.Combine(InstallDirT.Text, destinationMod), false);
            }
        }

        private string dependencyToDestination(FileInfo dep, string destinationMod) {
            string filename = dep.FullName;
            string relativeFilename = "";
            if (filename.Contains("mainmod")) {
                relativeFilename = filename.Substring(filename.IndexOf("mainmod") + "mainmod\\".Length);
            } else {
                //drop files not from mainmod (like readme) plainly into the root dir
                relativeFilename = dep.Name;
            }
            string to = InstallDirT.Text +"\\" + destinationMod + "\\" + relativeFilename;
            return to;
        }

        private List<FileInfo> gatherDependencies(string moddir) {
            //returns list of files, expands dirs into files
            List<string> paths = gatherDependenciesRaw(moddir);
            List<FileInfo> files = new List<FileInfo>();
            string basedir = InstallDirT.Text;
            foreach (string path in paths) {
                string from = Path.Combine(basedir, path);
                if (File.Exists(from)) {
                    files.Add(new FileInfo(from));
                } else {
                    if (Directory.Exists(from)) {
                        files.AddRange(getFilesInDir(from));
                    } else {
                        addLog("Dependency " + path + " missing!");
                        throw new DependencyBrokenException();
                    }
                }
            }
            return files;
        }

        private List<string> gatherDependenciesRaw(string moddir) {
            //paths returned do not include "Driftmoon", starting nor trailing slashes
            List<string> deps = new List<string>();
            List<FileInfo> fis = findReadmes(moddir);
            foreach (FileInfo fi in fis) {
                TextReader tr = fi.OpenText();
                string contents = tr.ReadToEnd();
                tr.Close();
                MatchCollection matches = readmePathPattern.Matches(contents);
                if (matches.Count != 0) {
                    addLog("Found dependencies in " + fi.FullName);
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
            return deps;
        }

        private List<FileInfo> findReadmes(string dir) {
            List<FileInfo> readmes = new List<FileInfo>();
            DirectoryInfo di = new DirectoryInfo(dir);
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis) {
                string s = fi.Name.ToLower();
                if (s.Contains("readme")) {
                    readmes.Add(fi);
                }
            }
            return readmes;
        }

        private List<FileInfo> getFilesInDir(string dirname) {
            List<FileInfo> files = new List<FileInfo>();
            DirectoryInfo dir = new DirectoryInfo(dirname);
            DirectoryInfo[] subdirs = dir.GetDirectories();

            files.AddRange(dir.GetFiles());

            foreach (DirectoryInfo subdir in subdirs) {
                string temppath = Path.Combine(dirname, subdir.Name);
                files.AddRange(getFilesInDir(temppath));
            }
            return files;
        }

        private void DirectoryCopy(
            string sourceDirName, string destDirName) {
            List<FileInfo> files = getFilesInDir(sourceDirName);

            foreach (FileInfo file in files) {
                copyRelativeFileCreateDir(file, sourceDirName, destDirName, false);
            }
        }

        private void copyRelativeFileCreateDir(FileInfo source, string sourceRootDir, string toDir, bool overwrite) {
            string filename = source.FullName;
            string relativeFilename = "";
            relativeFilename = filename.Substring(sourceRootDir.Length + 1);
            string temppath = Path.Combine(toDir, relativeFilename);
            FileInfo dest = new FileInfo(temppath);
            if (!dest.Directory.Exists) {
                dest.Directory.Create();
            }
            if (overwrite) {
                source.CopyTo(temppath,true);
            } else {
                if (!dest.Exists) {
                    source.CopyTo(temppath, overwrite);
                }
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

    public class DependencyBrokenException : Exception {

    }

}
