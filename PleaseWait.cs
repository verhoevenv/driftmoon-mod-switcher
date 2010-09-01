using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace driftmoon_mod_switcher {
    public partial class PleaseWait : Form {
        public PleaseWait(int totalWork) {
            InitializeComponent();
            WorkPB.Maximum = totalWork;
            WorkPB.Step = 1;
        }

        public void addProgress(){
            WorkPB.PerformStep();
        }
    }
}
