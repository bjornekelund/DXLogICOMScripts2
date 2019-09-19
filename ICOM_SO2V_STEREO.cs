//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V helper script to toggle permanent stereo on 
// and off with Microham MK2R/u2R.
// Replaces DXLog's own Ctrl-Alt-S (AltGr-S) key.
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-07-30

using IOComm;

namespace DXLog.net
{
    public class IcomSO2Stereo : ScriptClass
    {
        readonly string statusMessage = "Focus on {0} VFO. {1}.";

        public void Initialize(FrmMain main) { }
        public void Deinitialize() { } 

        // Toggle permanent stereo, execution of Main is mapped to Ctrl-Alt-S
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;
            bool stereoAudio = main.ListenStatusMode == COMMain.ListenMode.R1R2;

            if (cdata.OPTechnique == ContestData.Technique.SO2V)
            {
                if (!stereoAudio)
                    main.SetListenStatusMode(COMMain.ListenMode.R1R2, true, false);
                else
                    main.SetListenStatusMode(focusedRadio == 1 ? COMMain.ListenMode.R1R1 : COMMain.ListenMode.R2R2, true, false);

                main.SetMainStatusText(string.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", stereoAudio ? "Single receiver" : "Stereo"));

                main.ScriptContinue = false; // Do not continue with DXLog's own key definition
            }
            else
                main.ScriptContinue = true; // Use DXLog's own key definition if not SO2V

        }
    }
}
