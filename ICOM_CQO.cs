//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM_CQO script to switch to other radio and transmit CQ
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2020-03-23

using IOComm;
using System.Threading;

namespace DXLog.net
{
    public class CqOtherRadio : ScriptClass
    {
        public void Initialize(FrmMain main) { }

        public void Deinitialize() { }

        // Toggle radio focus, call CQ
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            switch (cdata.OPTechnique)
            {
                case ContestData.Technique.SO2R:
                case ContestData.Technique.SO2V:
                    cdata.ActiveRadio = (cdata.ActiveRadio == 1 ? 2 : 1);
                    cdata.FocusedRadio = cdata.ActiveRadio;
                    Thread.Sleep(50); // Wait 50ms for switch to safely complete
                    main.SendKeyMessage(false, "F1");
                    main.SetMainStatusText(string.Format("Focus on radio {0}.", cdata.ActiveRadio));
                    main.ScriptContinue = false; // Do not continue with DXLog's own key definition
                    break;
                default:
                    main.ScriptContinue = true; // Use DXLog's own key definition if SO1R or SO2R advanced
                    break;
            }
        }
    }
}
