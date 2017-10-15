using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChangeToolRange
{
    static class Program
    {
        static public bool ExitMainFrmWithOk { get; set; }
        static public int SelectedToolRange { get; set; }
        static public bool AutoStartMultiselectedFiles { get; set; }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AutoStartMultiselectedFiles = true;

            if (args.Length != 0)
            {
                if (AutoStartMultiselectedFiles)
                {
                    if (System.IO.File.Exists(args[0])) { Application.Run(new FrmMain(args[0])); }
                    if (ExitMainFrmWithOk == true)
                    {
                        for (var i = 1; i < args.Length; i++)
                        {
                            if (System.IO.File.Exists(args[i]))
                            {
                                FileToChange ftc = new FileToChange(new System.IO.FileInfo(args[i]));
                                ftc.ChangeToolRange(SelectedToolRange, true);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var file in args)
                    {
                        if (System.IO.File.Exists(file)) { Application.Run(new FrmMain(file)); }
                    }
                }
            }
            else
            {
                OpenFileDialog fd = new OpenFileDialog()
                {
                    Multiselect = true,
                    Filter = "*.h files (*.h)|*.h|All files (*.*)|*.*",
                    Title = "HEINDENHAIN-Programm bzw. *.h-Datei wählen... " + FrmMain.AboutApp                };

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    if (AutoStartMultiselectedFiles)
                    {
                        Application.Run(new FrmMain(fd.FileNames[0]));
                        if (ExitMainFrmWithOk == true)
                        {
                            for (var i = 1; i < fd.FileNames.Length; i++)
                            {
                                FileToChange ftc = new FileToChange(new System.IO.FileInfo(fd.FileNames[i]));
                                ftc.ChangeToolRange(SelectedToolRange, true);
                            }
                        }
                    }
                    else
                    {
                        foreach (var file in fd.FileNames)
                        {
                            Application.Run(new FrmMain(file));
                        }
                    }
                }
            }
        }
    }
}
