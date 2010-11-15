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
using System.Security.Cryptography;

namespace driftmoon_mod_switcher {
    public partial class MainForm : Form {
        private Regex readmePathPattern = new Regex(@"^[/\\]?(([\w.])+[/\\])*([\w.]*)?\r?$", RegexOptions.Multiline);
        private Boolean settingsChanged = false;
        private string currentMod = "";
        private DriftmoonVersion dmVersion;
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
            currentMod = dmVersion.getCurrentMod();
            addLog("Found all mods.");
        }

        public bool isCurrentMod(string mod) {
            if (currentMod == null) {
                currentMod = dmVersion.getCurrentMod();
            }
            return currentMod.Equals(mod);
        }

        private void refreshModList() {
            fullyInstalled = new Dictionary<string, installStatus>();
            string[] dirs = Directory.GetDirectories(InstallDirT.Text);
            foreach (string dir in dirs) {
                string lastpart = dir.Substring(dir.LastIndexOf("\\") + 1);
                if (lastpart != "ui" && lastpart != "data") {
                    if (isModDir(dir)) {
                        try {
                            List<FileCopyJob> files = gatherDependencyJobs(dir,lastpart);
                            bool installed = true;
                            foreach (FileCopyJob f in files) {
                                if (!f.destExists()) {
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
            if (isDriftmoonDir(d)) {
                setDriftmoonDir(d);
            } else {
                MessageBox.Show("No Driftmoon directory found in " + d +
                    ", please select a valid Driftmoon directory", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void setDriftmoonDir(string d) {
            settingsChanged = true;
            clearLog();
            addLog("Found Driftmoon directory...");
            try {
                dmVersion = DriftmoonVersion.getDriftmoonVersion(d);
                addLog(dmVersion.ToString());
                InstallDirT.Text = d;
                refreshMods();
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show("Windows told me: \"" + ex.Message + "\" Perhaps try running as administrator?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool isDriftmoonDir(string path) {
            return Directory.Exists(path) && File.Exists(path + "\\Driftmoon.exe");
        }

        private void InstallDirB_Click(object sender, EventArgs e) {
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.Description = "Select the Driftmoon install directory";
            f.ShowNewFolderButton = false;
            DialogResult result = f.ShowDialog();
            if (result == DialogResult.OK) {
                string d = f.SelectedPath;
                if (isDriftmoonDir(d)) {
                    setDriftmoonDir(d);
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

            try {
                changeMod(newMod);
            } catch (UnauthorizedAccessException ex) {
                MessageBox.Show("Windows told me: \"" + ex.Message + "\" Perhaps try running as administrator?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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
            dmVersion.setMod(newMod);
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
            string newModDir = InstallDirT.Text + "\\" + newMod;
            List<FileCopyJob> jobs = new List<FileCopyJob>();
            jobs.AddRange(gatherDirectoryCopyJobs(realmod, newModDir));
            jobs.AddRange(gatherDependencyJobs(d, newModDir));

            //TODO: what if mod already installed?
            doWork(jobs);
            foreach (FileInfo f in findReadmes(d)) {
                f.CopyTo(Path.Combine(newModDir,f.Name),true);
            }
            setMod(newMod);
            addLog("Succeeded installing " + newMod + "!");
            refreshMods();
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

            doWork(gatherDependencyJobs(searchpath,destinationMod));
        }

        private void doWork(List<FileCopyJob> jobs) {
            PleaseWait popup = new PleaseWait(jobs.Count);
            try {
                int posx = this.Location.X + (this.Size.Width - popup.Size.Width) / 2;
                int posy = this.Location.Y + (this.Size.Height - popup.Size.Height) / 2;
                popup.Show();
                popup.Location = new Point(posx, posy);
                popup.Update();

                //TODO: maybe put this in another thread to prevent not-responding thing
                foreach (FileCopyJob job in jobs) {
                    string to = job.getDestinationPath();
                    //addLog(job.source.FullName + " --> " + to);
                    if (File.Exists(to)) {
                        addLog("Destination file already exists, skipping...");
                    }
                    job.doCopy();
                    popup.addProgress();
                }
            } finally {
                popup.Hide();
            }
        }

        private List<FileCopyJob> gatherDependencyJobs(string readmepath, string mod) {
            List<FileCopyJob> jobs = new List<FileCopyJob>();
            List<string> paths = gatherDependenciesRaw(readmepath);
            string mainmoddir = InstallDirT.Text + "\\mainmod";
            foreach (string path in paths) {
                string from = Path.Combine(InstallDirT.Text, path);
                if (File.Exists(from)) {
                    jobs.Add(new FileCopyJob(new FileInfo(from), mainmoddir, Path.Combine(InstallDirT.Text, mod), false));
                } else {
                    if (Directory.Exists(from)) {
                        foreach (FileInfo fi in getFilesInDir(from)) {
                            jobs.Add(new FileCopyJob(fi, mainmoddir, Path.Combine(InstallDirT.Text, mod), false));
                        }
                    } else {
                        addLog("Dependency " + path + " missing!");
                        throw new DependencyBrokenException();
                    }
                }
            }
            return jobs;
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

        private List<FileCopyJob> gatherDirectoryCopyJobs(
            string sourceDirName, string destDirName) {
            List<FileCopyJob> jobs = new List<FileCopyJob>();
            foreach (FileInfo file in getFilesInDir(sourceDirName)) {
                jobs.Add(new FileCopyJob(file, sourceDirName, destDirName, false));
            }

            return jobs;
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
