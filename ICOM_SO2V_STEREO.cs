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
            // ListenStatusMode: 0=Radio 1, 1=Radio 2 toggle, 2=Radio 2, 3=Both
            bool stereoAudio = main.ListenStatusMode == 3;
            bool modeIsSo2V = cdata.OPTechnique == ContestData.Technique.SO2V;

            if (modeIsSo2V)
            {
                if (!stereoAudio)
                    main.SetListenStatusMode(3, true, false);
                else
                    main.SetListenStatusMode(0, true, false);

                main.SetMainStatusText(string.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", stereoAudio ? "Single receiver" : "Stereo"));
            }

            main.ScriptContinue = false; // Do not continue with DXLog's own key definition
        }
    }
}
