//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V helper script to toggle permanent stereo on 
// and off with Microham MK2R/u2R.
// Replaces DXLog's own Ctrl-Alt-S (AltGr-S) key.
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-07-30

using IOComm;
using System.Threading;

namespace DXLog.net
{
    public class CqOtherRadio : ScriptClass
    {
        readonly string statusMessage = "Focus on radio {0}.";

        public void Initialize(FrmMain main) { }
        public void Deinitialize() { }

        // Toggle radio focus, call CQ
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            if (cdata.OPTechnique != ContestData.Technique.SO1R)
            {
                switch (cdata.OPTechnique)
                {
                    case ContestData.Technique.SO2R:
                        if (cdata.FocusedRadio == 1)
                        {
                            cdata.ActiveRadio = 2;
                            cdata.FocusedRadio = 2;
                        }
                        else
                        {
                            cdata.ActiveRadio = 1;
                            cdata.FocusedRadio = 1;
                        }
                        break;
                    case ContestData.Technique.SO2R_ADV:
                    case ContestData.Technique.SO2V:
                        cdata.FocusedRadio = cdata.FocusedRadio == 1 ? 2 : 1;
                        break;
                }

                comMain.SetActiveRadio(cdata.FocusedRadio);
                cdata.TXOnRadio = cdata.FocusedRadio;

                //UCQSO cl1 = main.CurrentEntryLine;
                //if (cl1 != null && cl1.ActualQSO != null && cl1.ActualQSO.Callsign == string.Empty)
                //    cl1.Controls["txtCallSign"].Focus();

//                Thread.Sleep(1000);
                //main.SendFKeyMessage("F1");

                main.SetMainStatusText(string.Format(statusMessage, cdata.FocusedRadio));

                main.ScriptContinue = false; // Do not continue with DXLog's own key definition
            }
            else
                main.ScriptContinue = true; // Use DXLog's own key definition if SO1R

        }
    }
}
