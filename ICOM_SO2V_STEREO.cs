//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V helper script to toggle permanent dual watch on and off. 
// Mapped to Ctrl-Alt-S (AltGr-S) so that it executes together with the 
// built-in listen mode toggle. 
// Only active for ICOM radio but does not verify radio is SO2V capable
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-04-16

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomSO2VDW : ScriptClass
    {
        readonly bool Debug = false;
        COMPort microHamPort;

        readonly string statusMessage = "Focus on {0} VFO. {1}.";


        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            // Find Microham port
            if (microHamPort == null)
                foreach (COMPort _comport in main.COMMainProvider._com)
                {
                    if (_comport != null)
                    {
                        switch (_comport._portDeviceName)
                        {
                            case "MK2R/MK2R+/u2R":
                                if (_comport._mk2r != null)
                                    microHamPort = _comport;
                                main.SetMainStatusText("Found Microham device");
                                break;
                            default:
                                main.SetMainStatusText("Did not find Microham device");
                                microHamPort = null;
                                break;
                        }
                    }
                }
        }

        public void Deinitialize() { } 

        // Toggle permanent stereo, execution of Main is mapped to Ctrl-Alt-D
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;
            // ListenStatusMode: 0=Radio 1, 1=Radio 2 toggle, 2=Radio 2, 3=Both
            bool stereoAudio = (main.ListenStatusMode != 0);
            bool modeIsSo2V = (cdata.OPTechnique == ContestData.Technique.SO2V);

            if (true)
            {
                main.SetMainStatusText(string.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", stereoAudio ? "Single receiver" : "Stereo"));

                if (modeIsSo2V)
                    if (microHamPort != null)
                    {
                        microHamPort._mk2r.SendCustomCommand("FRS");
                    }

                //main.ScriptContinue = true;
            }

        }
    }
}
