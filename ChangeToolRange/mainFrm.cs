using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Deployment.Application;

namespace ChangeToolRange
{
    public partial class FrmMain : Form
    {
        public FrmMain(string file)
        {
            InitializeComponent();

            _fileToChange = new FileToChange(_heidenhainFile = new FileInfo(file));

            ToolRangeNumericUpDown.Value = 0; ToolRangeNumericUpDown.Value = _fileToChange.OriginalToolRange;

            this.FileNameTextBox.Text = _fileToChange.NoteText;

            this.Text = AboutApp;

            if (_fileToChange.IsToolRangeConsistent != true)
            {
                FileNameTextBox.ForeColor = System.Drawing.Color.Red;

                ChangeButton.BackColor = System.Drawing.Color.Red;

                ToolRangeNumericUpDown.ForeColor = System.Drawing.Color.Red;
            }

            ToolTip tt = new ToolTip();
            tt.ShowAlways = true;
            tt.SetToolTip(this, _fileToChange.ToString());
            foreach (Control c in this.Controls )
            {
                tt.SetToolTip(c, _fileToChange.ToString());
            }            

            if (_fileToChange.MatchesOfToolCalls.Count < 1 | _fileToChange.OnlyRestrictiveToolValues)
            {
                ChangeButton.Enabled = false; ToolRangeNumericUpDown.Enabled = false;
            }
        }

        public FileInfo _heidenhainFile { get; private set; }
        public FileToChange _fileToChange { get; private set; }

        private void Button1_Click(object sender, EventArgs e)
        {
            _fileToChange.ChangeToolRange(Int32.Parse(ToolRangeNumericUpDown.Value.ToString()), false);
            Program.SelectedToolRange = Int32.Parse(ToolRangeNumericUpDown.Value.ToString());
            Program.ExitMainFrmWithOk = true;
            this.Close();
        }

        private void MyNumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ChangeButton.Text = "in Bsp.: 'TOOL CALL " + (_fileToChange.FirstNotRestrictiveToolValue + (ToolRangeNumericUpDown.Value * 1000)).ToString() +
                                " Z S635' ...ändern"; // + "\n(" + MyFTC.CountOfRestrictiveToolValues.ToString() +"x Standardwerkzeuge werden nicht geändert)";

            if (ToolRangeNumericUpDown.Value == _fileToChange.OriginalToolRange)
            {
                ToolRangeNumericUpDown.Font = new Font(ToolRangeNumericUpDown.Font, FontStyle.Bold);
                ChangeButton.Enabled = false;

            }
            else
            {
                ToolRangeNumericUpDown.Font = new Font(ToolRangeNumericUpDown.Font, FontStyle.Regular);
                ChangeButton.Enabled = true;
            }
        }

        private void TextBox1_DoubleClick(object sender, EventArgs e)
        {
            Process.Start(@"notepad.exe", _heidenhainFile.FullName);
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            Activate();
        }

        public static String AboutApp
        {
            get
            {
                return Application.ProductName +
                                   " V" + (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed ?
                                   System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() :
                                   Application.ProductVersion.ToString());
            }
        }
    }
}