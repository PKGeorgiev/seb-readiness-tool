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
        private SelfUpdate _update;

        public MainForm()
        {
            InitializeComponent();
            _utils = new SEBUtils();
            _update = new SelfUpdate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _= _update.CheckForUpdate(Constants.SEBRepoOwner, Constants.SEBRepoName);

            //_ = _update.GetLatestTagAsync(Constants.SEBRepoOwner, Constants.SEBRepoName);
            //var res = SEBUtils.GetSoftwareEntry("Safe Exam Browser");
            //SEBUtils.IsAdministrator();
            //_utils.ArchiveLogs();
        }
    }
}
