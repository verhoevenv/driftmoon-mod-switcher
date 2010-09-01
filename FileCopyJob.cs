using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace driftmoon_mod_switcher {
    class FileCopyJob {
        public FileInfo source { get; set;  }
        public string sourceRootDir { get; set; }
        public string destDir { get; set; }
        public bool overwrite { get; set; }

        public FileCopyJob(FileInfo source, string sourceRootDir, string destDir, bool overwrite) {
            this.source = source;
            this.sourceRootDir = sourceRootDir;
            this.destDir = destDir;
            this.overwrite = overwrite;
        }

        public void doCopy() {
            string filename = source.FullName;
            string relativeFilename = "";
            relativeFilename = filename.Substring(sourceRootDir.Length + 1);
            string temppath = Path.Combine(destDir, relativeFilename);
            FileInfo dest = new FileInfo(temppath);
            if (!dest.Directory.Exists) {
                dest.Directory.Create();
            }
            if (overwrite) {
                source.CopyTo(temppath, true);
            } else {
                if (!dest.Exists) {
                    source.CopyTo(temppath, overwrite);
                }
            }
        }
    }
}
