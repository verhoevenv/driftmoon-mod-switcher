using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace driftmoon_mod_switcher {
    abstract class DriftmoonVersion {
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
                    return new PreviewVersion();
                default:
                    return new NonPreviewVersion();
            }
        }

        abstract public bool usesOptions();

        public bool usesRegistry() {
            return !usesOptions();
        }
    }

    class PreviewVersion : DriftmoonVersion {
        public override string ToString() {
            return "Driftmoon Preview version found";
        }

        public override bool usesOptions() {
            return true;
        }
    }

    class NonPreviewVersion : DriftmoonVersion {
        public override string ToString() {
            return "Driftmoon Pre-order version found";
        }

        public override bool usesOptions() {
            return false;
        }
    }
}
