using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace driftmoon_mod_switcher {
    class DriftmoonVersion {
        public enum Version {DMPREVIEW091123, NON_PREVIEW};
        private Version v;

        public DriftmoonVersion(string path) {
            this.v = getVersionFromPath(path);
        }

        private string getMD5HashFromFile(string fileName) {
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

        private Version getVersionFromPath(string path) {
            string md5 = getMD5HashFromFile(path + "\\Driftmoon.exe");
            switch (md5) {
                case "a3b68f22936451316f453a11364aae0b":
                    return Version.DMPREVIEW091123;
                default:
                    return Version.NON_PREVIEW;
            }
        }

        public override string ToString() {
            switch (v) {
                case Version.DMPREVIEW091123:
                    return "Driftmoon Preview version found";
                case Version.NON_PREVIEW:
                    return "Driftmoon Pre-order version found";
                default:
                    return "Error while scanning Driftmoon version";
            }
        }

        public bool usesOptions() {
            return v == Version.DMPREVIEW091123;
        }

        public bool usesRegistry() {
            return !usesOptions();
        }
    }
}
