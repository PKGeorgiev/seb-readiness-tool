using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEBReadinessTool
{    
    public partial class MainForm : Form
    {
        private SEBUtils _utils;

        public MainForm()
        {
            InitializeComponent();
            _utils = new SEBUtils();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SEBUtils.IsAdministrator();
            _utils.ArchiveLogs();
        }
    }
}
