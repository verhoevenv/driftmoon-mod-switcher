using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace driftmoon_mod_switcher {
    abstract class DriftmoonVersion {
        protected string path;

        protected DriftmoonVersion(string path) {
            this.path = path;
        }

        public static DriftmoonVersion getDriftmoonVersion(string path) {
            return getVersionFromPath(path);
        }

        private static string getMD5HashFromFile(string fileName) {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++) {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private static DriftmoonVersion getVersionFromPath(string path) {
            string md5 = getMD5HashFromFile(path + "\\Driftmoon.exe");
            switch (md5) {
                case "a3b68f22936451316f453a11364aae0b":
                    return new PreviewVersion(path);
                default:
                    return new NonPreviewVersion(path);
            }
        }

        public abstract bool usesOptions();

        public bool usesRegistry() {
            return !usesOptions();
        }

        public abstract void setMod(string newMod);
        public abstract string getCurrentMod();
    }

    class PreviewVersion : DriftmoonVersion {
        public PreviewVersion(string path)
            : base(path) {
        }

        public override string ToString() {
            return "Driftmoon Preview version found";
        }

        public override bool usesOptions() {
            return true;
        }

        public override string getCurrentMod() {
            TextReader tr = new StreamReader(this.path + "\\options.ini");
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

        public override void setMod(string newMod) {
            TextReader tr = new StreamReader(this.path + "\\options.ini");
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
    }

    class NonPreviewVersion : DriftmoonVersion {
        public NonPreviewVersion(string path)
            : base(path) {
        }

        public override string ToString() {
            return "Driftmoon Pre-order version found";
        }

        public override bool usesOptions() {
            return false;
        }
    }
}
