//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V helper script to toggle permanent stereo on and off. 
// Mapped to Ctrl-Alt-S (AltGr-S). 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-07-29

using IOComm;

namespace DXLog.net
{
    public class IcomSO2Stereo : ScriptClass
    {
        readonly bool Debug = false;

        readonly string statusMessage = "Focus on {0} VFO. {1}.";
        COMPort microHamPort = null;


        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            // Find and save Microham port
            foreach (COMPort _comport in main.COMMainProvider._com)
            {
                if (_comport != null || _comport._portDeviceName == "MK2R/MK2R+/u2R")
                {
                    if (_comport._mk2r != null)
                    {
                        microHamPort = _comport;
                        //main.SetMainStatusText("Found Microham device");
                    }
                }
            }
        }

        public void Deinitialize() { } 

        // Toggle permanent stereo, execution of Main is mapped to Ctrl-Alt-S
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;
            // ListenStatusMode: 0=Radio 1, 1=Radio 2 toggle, 2=Radio 2, 3=Both
            bool stereoAudio = main.ListenStatusMode == 3;
            bool modeIsSo2V = cdata.OPTechnique == ContestData.Technique.SO2V;

            if (modeIsSo2V && microHamPort != null)
            {
                if (!stereoAudio)
                    main.SetListenStatusMode(3, false, false);
                else
                {
                    if (focusedRadio == 1)
                    {
                        main.SetListenStatusMode(0, false, false); // To set ListenStatusMode correctly
                        microHamPort._mk2r.SendCustomCommand("FR1");  // Override, to select correct radio in Microham
                    }
                    else
                    {
                        main.SetListenStatusMode(0, false, false); // To set ListenStatusMode correctly
                        microHamPort._mk2r.SendCustomCommand("FR2");  // Override, to select correct radio in Microham
                    }
                }
                main.SetMainStatusText(string.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", stereoAudio ? "Single receiver" : "Stereo"));
            }

            main.ScriptContinue = false;
        }
    }
}
