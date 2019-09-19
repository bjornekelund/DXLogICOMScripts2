//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V VFO focus and audio management for Microham MK2R, 
// event driven but also mapped to key for temporary stereo on/off toggling.
// Key used is typically § (on EU keyboard) or `(on US keyboard) 
// to maintain muscle-memory compatibility with N1MM.
// Tested on IC-7610 but should work on all modern dual receiver radios.
// Use Ctrl-Alt-S/AltGr-S to toggle permanent stereo on/off. 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-09-19

using IOComm;

namespace DXLog.net
{
    public class IcomSO2V : ScriptClass
    {
        FrmMain mainForm;
        COMPort microHamPort;

        readonly byte[] IcomDualWatchOn = { 0x07, 0xC1 };
        readonly byte[] IcomSelectMain = { 0x07, 0xD0 };
        readonly byte[] IcomSelectSub = { 0x07, 0xD1 };
        readonly byte[] IcomSplitOff = { 0x0F, 0x00 };
        readonly string statusMessage = "Focus on {0} VFO. {1}.";

        int lastFocusedRadio;
        bool tempStereoToggle;

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            mainForm = main;
            CATCommon radio1 = main.COMMainProvider.RadioObject(1);

            if (main.ContestDataProvider.OPTechnique == ContestData.Technique.SO2V)
            {
                // Find Microham port
                foreach (COMPort _comport in main.COMMainProvider._com)
                    if (_comport._mk2r != null)
                        microHamPort = _comport;

                if (microHamPort == null)
                    main.COMMainProvider.SignalMessage("ICOM_SO2V: ERROR no Microham device");

                // Initialize temporary stereo mode to DXLog's stereo mode to support temporary toggle
                // At start up, radio 1 is always focused and stereo audio is forced
                main.SetListenStatusMode(COMMain.ListenMode.R1R2, true, false);
                tempStereoToggle = false;
                lastFocusedRadio = 1;

                main.ContestDataProvider.FocusedRadioChanged += new ContestData.FocusedRadioChange(HandleFocusChange);

                // Only initialize radio if present and ICOM
                if (radio1 != null)
                    if (radio1.IsICOM())
                    {
                        // Initialize radio to DW off, Split off and Main VFO focused
                        radio1.SendCustomCommand(IcomDualWatchOn);
                        radio1.SendCustomCommand(IcomSelectMain);
                        radio1.SendCustomCommand(IcomSplitOff);
                    }
            }
        }

        public void Deinitialize() { }

        // Toggle dual watch in SO2V. Typically mapped to a key in upper left corner of keyboard.
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;
            bool stereoAudio = main.ListenStatusMode == COMMain.ListenMode.R1R2;
            bool tempStereoAudio; 

            if (cdata.OPTechnique == ContestData.Technique.SO2V)
            {
                tempStereoAudio = tempStereoToggle ? stereoAudio : !stereoAudio;

                if (microHamPort != null)
                {
                    main.SetMainStatusText(string.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", tempStereoAudio ? "Stereo" : "Single receiver"));

                    if (tempStereoAudio)
                        microHamPort._mk2r.SendCustomCommand("FRS");
                    else
                        microHamPort._mk2r.SendCustomCommand(string.Format("FR{0}", focusedRadio == 1 ? 1 : 2));

                    tempStereoToggle = !tempStereoToggle;
                }
            }
        }

        // Event handler invoked when switching between VFO in SO2V in DXLog.net
        private void HandleFocusChange()
        {
            CATCommon radio1 = mainForm.COMMainProvider.RadioObject(1);
            int focusedRadio = mainForm.ContestDataProvider.FocusedRadio;
            bool stereoAudio = mainForm.ListenStatusMode == COMMain.ListenMode.R1R2;

            tempStereoToggle = false;

            if (focusedRadio != lastFocusedRadio) // Only active in SO2V. Ignore redundant invokations.
            {
                lastFocusedRadio = focusedRadio;

                if (microHamPort != null)
                    if (stereoAudio)
                        mainForm.SetListenStatusMode(COMMain.ListenMode.R1R2, true, false);
                    else
                        mainForm.SetListenStatusMode(focusedRadio == 1 ? COMMain.ListenMode.R1R1 : COMMain.ListenMode.R2R2, true, false);

                if (radio1 != null)
                    if (radio1.IsICOM())
                    radio1.SendCustomCommand(focusedRadio == 1 ? IcomSelectMain : IcomSelectSub);

                mainForm.SetMainStatusText(string.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", stereoAudio ? "Stereo" : "Single receiver"));
            }
        }
    }
}
