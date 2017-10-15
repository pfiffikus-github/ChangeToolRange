using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ChangeToolRange
{
    public class FileToChange
    {
        public const string ToolCallString = "TOOL CALL";

        public FileToChange(FileInfo file)
        {
            File = file;
            CountOfRestrictiveToolValues = 0;   //in DetectIsToolRangeConsistent() neu initalisiert
            OriginalToolRange = 0;              //in DetectIsToolRangeConsistent() neu initalisiert
            FirstNotRestrictiveToolValue = 0;   //in DetectIsToolRangeConsistent() neu initalisiert
            OnlyRestrictiveToolValues = false;  //in DetectIsToolRangeConsistent() neu initalisiert
            FileContent = System.IO.File.ReadAllText(File.FullName);
            MatchesOfToolCalls = GetDetectMatchesOfToolCalls();
            IsToolRangeConsistent = DetectIsToolRangeConsistent();
        }

        public int OriginalToolRange { get; private set; }
        public MatchCollection MatchesOfToolCalls { get; private set; }
        public string FileContent { get; private set; }
        public FileInfo File { get; private set; }
        public bool IsToolRangeConsistent { get; private set; }
        public bool OnlyRestrictiveToolValues { get; private set; }
        public int FirstNotRestrictiveToolValue { get; private set; }
        public int CountOfRestrictiveToolValues { get; private set; }

        private MatchCollection GetDetectMatchesOfToolCalls()
        {
            var rgx = new Regex(@"TOOL\s+CALL\s+(\d+)");

            MatchCollection matches = rgx.Matches(FileContent);

            return matches;
        }

        private bool DetectIsToolRangeConsistent()
        {
            decimal[] ToolCallValues = new decimal[0];
            decimal CalcConsistency = 0;
            OnlyRestrictiveToolValues = true;

            foreach (Match m in MatchesOfToolCalls)
            {
                ToolCall tc = new ToolCall(m);

                //Continue ForEach by Restriction
                if (IsRestrictiveToolValue(tc.OrgToolCallValue - tc.OrgToolRangeValue))
                {
                    this.CountOfRestrictiveToolValues++;
                    continue;
                }
                else if (OnlyRestrictiveToolValues)
                {
                    OnlyRestrictiveToolValues = false;
                    FirstNotRestrictiveToolValue = tc.OrgToolCallValue - Int32.Parse(tc.OrgToolRangeValue.ToString());
                }

                if (m.Groups.Count > 1)
                {
                    Array.Resize(ref ToolCallValues, ToolCallValues.Length + 1);
                    ToolCallValues[ToolCallValues.Length - 1] = System.Math.Floor(decimal.Parse(m.Groups[1].ToString()) / 1000);
                    CalcConsistency = CalcConsistency + ToolCallValues[ToolCallValues.Length - 1];
                }
            }

            if (ToolCallValues.Length > 0)
            {
                OriginalToolRange = Int32.Parse(ToolCallValues[0].ToString());

                if (CalcConsistency / ToolCallValues.Length != ToolCallValues[0])
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        public void ChangeToolRange(int newRange, bool showErrAtInconsistency)
        {
            if (IsToolRangeConsistent == false && showErrAtInconsistency)
            {
                DialogResult result = MessageBox.Show(NoteText + "\n" + "\n" + this.ToString() + "\n" + "Neuen ToolRange dennoch in '" + newRange.ToString() + "' ändern?", "HINWEIS... " + FrmMain.AboutApp,
                                                      MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No) { return; }
            }

            var newFileContent = FileContent;
            string newToolCallString;
            int newToolCallValue;

            foreach (Match m in MatchesOfToolCalls)
            {
                var tc = new ToolCall(m);

                //Continue ForEach by Restriction
                if (IsRestrictiveToolValue(tc.OrgToolCallValue - tc.OrgToolRangeValue)) { continue; }

                newToolCallValue = (Int32.Parse(tc.OrgToolCallString.Replace(ToolCallString + " ", "")) -
                                   Int32.Parse(tc.OrgToolRangeValue.ToString())) +
                                   (newRange * 1000);

                newToolCallString = tc.OrgToolCallString.Replace(tc.OrgToolCallValue.ToString(), newToolCallValue.ToString());

                newFileContent = newFileContent.Replace(tc.OrgToolCallString, newToolCallString);
            }
            System.IO.File.WriteAllText(System.IO.Path.Combine(File.DirectoryName, File.Name), newFileContent);
        }

        public override string ToString()
        {
            var ToolCallsToString = "";
            var SubString = "";

            foreach (Match m in MatchesOfToolCalls)
            {
                ToolCall tc = new ToolCall(m);

                if (!IsRestrictiveToolValue(tc.OrgToolCallValue - tc.OrgToolRangeValue))
                {
                    SubString = "";
                }
                else
                {
                    SubString = "   (≙ Standardwerkzeug)";
                }

                ToolCallsToString = ToolCallsToString + "• " + m.Value + SubString + "\n";
            }
            return ToolCallsToString + "\n-------------- INFORMATION --------------" +
                   "\n(• " + this.MatchesOfToolCalls.Count.ToString() + "x '" + ToolCallString + "' insgesamt)" +
                   "\n(• " + (this.MatchesOfToolCalls.Count - this.CountOfRestrictiveToolValues).ToString() + "x veränderbare '" + ToolCallString + "')" +
                   "\n(• " + this.CountOfRestrictiveToolValues.ToString() + "x Standardwerkzeuge)";
        }

        public string NoteText
        {
            get
            {
                string nt = "PGM '" + Path.GetFileName(File.Name) + "' ";

                if (this.MatchesOfToolCalls.Count == 0)
                {
                    return nt + "(" + this.MatchesOfToolCalls.Count + "x 'TOOL CALL' enthalten)";
                }

                if (this.OnlyRestrictiveToolValues)
                {
                    return nt + "(" + this.MatchesOfToolCalls.Count + "x ausschließlich Standardwerkzeuge enthalten)";
                }

                if (this.IsToolRangeConsistent)
                {
                    return nt + "(" + this.MatchesOfToolCalls.Count + "x 'TOOL CALL' in Tool-Range " + this.OriginalToolRange.ToString() + " gefunden)";
                }
                else
                {
                    return nt + "(" + this.MatchesOfToolCalls.Count + "x nicht übereinstimmende 'TOOL CALL' gefunden)";
                }
            }
        }

        private bool IsRestrictiveToolValue(decimal toolValue)
        {
            if (toolValue <= 18)
            {
                return true;
            }
            return false;
        }
    }

    public class ToolCall
    {
        public ToolCall(Match m)
        {
            OrgToolCallString = m.Value + " ";
            OrgToolRangeValue = System.Math.Floor(decimal.Parse(m.Groups[1].ToString()) / 1000) * 1000;
            OrgToolCallValue = (Int32.Parse(OrgToolCallString.Replace(FileToChange.ToolCallString + " ", "")));
        }

        public String OrgToolCallString { get; private set; }
        public decimal OrgToolRangeValue { get; private set; }
        public int OrgToolCallValue { get; private set; }
    }

}
