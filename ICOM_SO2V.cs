//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V VFO focus and audio management, event driven but also mapped 
// to key for temporary stereo on/off toggling.
// Key used is typically § (on EU keyboard) or `(on US keyboard) 
// to maintain muscle-memory compatibility with N1MM.
// Tested on IC-7610 but should work on all modern dual receiver radios.
// Use Ctrl-Alt-S/AltGr-S to toggle permanent stereo on/off. 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-07-28

using IOComm;

namespace DXLog.net
{
    public class IcomSO2V : ScriptClass
    {
        readonly bool Debug = false;
        ContestData cdata;
        FrmMain mainForm;
        COMPort microHamPort;


        readonly byte[] IcomDualWatchOn = { 0x07, 0xC1 };
        readonly byte[] IcomSelectMain = { 0x07, 0xD0 };
        readonly byte[] IcomSelectSub = { 0x07, 0xD1 };
        readonly byte[] IcomSplitOff = { 0x0F, 0x00 };
        readonly string statusMessage = "Focus on {0} VFO. {1}.";

        bool tempStereoAudio;
        int lastFocus;

        // Executes at DXLog.net start 

        public void Initialize(FrmMain main)
        {
            CATCommon radio1 = main.COMMainProvider.RadioObject(1);
            cdata = main.ContestDataProvider;
            mainForm = main;

            // Find Microham port
            foreach (COMPort _comport in main.COMMainProvider._com)
            {
                if (_comport != null || _comport._portDeviceName == "MK2R/MK2R+/u2R")
                {
                    if (_comport._mk2r != null)
                    {
                        microHamPort = _comport;
                        //mainForm.SetMainStatusText("Found Microham device");
                    }
                }
            }

            // Initialize temporary stereo mode to DXLog's stereo mode to support temporary toggle
            // At start up, radio 1 is always focused and stereo audio is disabled
            tempStereoAudio = mainForm.ListenStatusMode == 3;
            lastFocus = 1;

            cdata.FocusedRadioChanged += new ContestData.FocusedRadioChange(HandleFocusChange);

            // Only initialize radio if present and ICOM
            if (radio1 != null)
                if (radio1.IsICOM())
                {
                    // Initialize radio to DW off, Split off and Main VFO focused
                    radio1.SendCustomCommand(IcomDualWatchOn);
                    radio1.SendCustomCommand(IcomSelectMain);
                    radio1.SendCustomCommand(IcomSplitOff);
                }

            main.SetListenStatusMode(3, false, false);
        }

        public void Deinitialize() { }

        // Toggle dual watch in SO2V. Typically mapped to a key in upper left corner of keyboard.

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;

            if (cdata.OPTechnique == ContestData.Technique.SO2V)
            {
                tempStereoAudio = !tempStereoAudio;

                if (microHamPort != null)
                {
                    main.SetMainStatusText(string.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", tempStereoAudio ? "Stereo" : "Single receiver"));

                    if (tempStereoAudio)
                        microHamPort._mk2r.SendCustomCommand("FRS");
                    else
                    {
                        if (focusedRadio == 1)
                            microHamPort._mk2r.SendCustomCommand("FR1");
                        else
                            microHamPort._mk2r.SendCustomCommand("FR2");
                    }
                }
            }
        }

        // Event handler invoked when switching between radios (SO2R) or VFO (SO2V) in DXLog.net

        private void HandleFocusChange()
        {
            CATCommon radio1 = mainForm.COMMainProvider.RadioObject(1);
            int focusedRadio = cdata.FocusedRadio;
            // ListenStatusMode: 0=Radio 1, 1=Radio 2 toggle, 2=Radio 2, 3=Both
            int listenMode = mainForm.ListenStatusMode;
            bool stereoAudio = listenMode == 3;
            bool modeIsSo2V = cdata.OPTechnique == ContestData.Technique.SO2V;
            string audioStatus;

            if (modeIsSo2V && focusedRadio != lastFocus) // Only active in SO2V and with ICOM. Ignore false triggers.
            {
                tempStereoAudio = stereoAudio; // Set temporary stereo mode to DXLog's stereo mode to support temporary toggle
                lastFocus = focusedRadio;

                if (microHamPort != null)
                {
                    if (stereoAudio)
                        mainForm.SetListenStatusMode(3, true, false);
                    else
                    {
                        if (focusedRadio == 1)
                        {
                            mainForm.SetListenStatusMode(0, false, false); // To set ListenStatusMode correctly
                            microHamPort._mk2r.SendCustomCommand("FR1");  // Override, to select correct radio in Microham
                        }
                        else
                        {
                            mainForm.SetListenStatusMode(0, false, false); // To set ListenStatusMode correctly
                            microHamPort._mk2r.SendCustomCommand("FR2");  // Override, to select correct radio in Microham
                        }
                    }
                }

                if (radio1 != null)
                    if (radio1.IsICOM())
                    radio1.SendCustomCommand(focusedRadio == 1 ? IcomSelectMain : IcomSelectSub);

                audioStatus = stereoAudio ? "Stereo" : "Single receiver";
                if (Debug) mainForm.SetMainStatusText(string.Format("IcomSO2V: Listenmode {0}. Focus is Radio #{1}, {2}.",
                    listenMode, focusedRadio, audioStatus));
                else
                    mainForm.SetMainStatusText(string.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", audioStatus));
            }
        }
    }
}
