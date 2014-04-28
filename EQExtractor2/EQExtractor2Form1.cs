//
// Copyright (C) 2001-2010 EQEMu Development Team (http://eqemulator.net). Distributed under GPL version 2.
//
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2
{
    public delegate void LogCallback(string message);

    /// <summary>
    /// Main GUI form. Wherever possible, please add non-UI code to Domain/
    /// </summary>
    /// 
    public partial class EQExtractor2Form1 : Form
    {
        #if DEBUG
        private const string _configuration = "(Debug Build)";
        #else 
        private const string _configuration = "(Release Build)";
        #endif

        private const int PacketsSeen = 0;
        string _zoneName=string.Empty;

        readonly GenerateSQLForm _sqlForm = new GenerateSQLForm();
        readonly LogForm _debugLog = new LogForm();
        readonly UserOptions _options = new UserOptions();


        StreamWriter PacketDebugStream;
        private PCapProcessor _processor;
        private readonly string _versionText = string.Empty;

        public EQExtractor2Form1()
        {
            _versionText = string.Format("EQExtractor2 Version {0} {1}", GetType().Assembly.GetName().Version,_configuration);
            InitializeComponent();
            DisplayUsageInfo();
            _options.PacketDumpViewerProgram.Text = Properties.Settings.Default.TextFileViewer;
            _options.ShowDebugWindowOnStartup.Checked = Properties.Settings.Default.ShowDebugWindowOnStartup;
            _options.ShowTimeStamps.Checked = Properties.Settings.Default.DumpTimeStamps;
        }

        
        public void Log(string message)
        {
            if (InvokeRequired)
            {
                LogCallback d = Log;
                Invoke(d, new object[] {message});
            }
            else
            {
                if(!string.IsNullOrEmpty(message))_debugLog.ConsoleWindow.Items.Add(message);
                _debugLog.ConsoleWindow.SelectedIndex = _debugLog.ConsoleWindow.Items.Count - 1;
                Application.DoEvents();
            }
        }
        public void SetStatus(string message)
        {
            if (InvokeRequired)
            {
                LogCallback d = SetStatus;
                Invoke(d, new object[] { message });
            }
            else
            {
                StatusBar.Text = message;
            }
        }

        public void PacketDebugLogger(string message)
        {
            PacketDebugStream.WriteLine(message);
        }

        private void DisableAllControls()
        {
            foreach (Control c in Controls)
            {
                if ((c is Button) || (c is TextBox) || (c is MaskedTextBox) || (c is CheckBox))
                    c.Enabled = false;
            }

        }

        private void EnableAllControls()
        {
            foreach (Control c in Controls)
                c.Enabled = true;

            menuGenerateSQL.Enabled =_processor!=null && _processor.StreamProcessor.StreamRecognised() && _processor.StreamProcessor.SupportsSQLGeneration();
            menuDumpAAs.Enabled = _processor != null && _processor.StreamProcessor.StreamRecognised();
        }

        private void menuLoadPCAP_Click(object sender, EventArgs e)
        {
            if (InputFileOpenDialog.ShowDialog() != DialogResult.OK)
                return;

            menuGenerateSQL.Enabled = false;
            menuPacketDump.Enabled = false;
            menuViewPackets.Enabled = false;
            menuDumpAAs.Enabled = false;

            var bw = new BackgroundWorker {WorkerReportsProgress = true};
            bw.DoWork += ProcessPCapFile;
            bw.RunWorkerCompleted += OnProcessPCapFileCompleted;
            bw.RunWorkerAsync(InputFileOpenDialog.FileName);

        }

   
        private void ProcessPCapFile(object sender, DoWorkEventArgs e)
        {
            var capFile = e.Argument as string;
            if (string.IsNullOrEmpty(capFile)) return;
            var bw = sender as BackgroundWorker;
            if (bw == null) throw new ArgumentNullException("sender");

            if (_options.EQPacketDebugFilename.Text.Length > 0)
            {
                try
                {
                    PacketDebugStream = new StreamWriter(_options.EQPacketDebugFilename.Text);
                }
                catch
                {
                    Log("Failed to open netcode debug file for writing.");
                    _options.EQPacketDebugFilename.Text = "";
                }
            }
            _processor = new PCapProcessor(Log,PacketDebugLogger,SetStatus,bw.ReportProgress);
            _processor.ProcessPCapFile(capFile);
        }

        private void OnProcessPCapFileCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || _processor==null) return;
            ProgressBar.Hide();

            if (_options.EQPacketDebugFilename.Text.Length > 0)
                PacketDebugStream.Close();

            PacketCountLabel.Text = _processor.PacketsSeen.ToString();
            if ( _processor.StreamProcessor.Packets.ErrorsInStream)
                Log("There were errors encountered in the packet stream. Data may be incomplete.");

            _debugLog.ConsoleWindow.SelectedIndex = _debugLog.ConsoleWindow.Items.Count - 1;

            menuFile.Enabled = true;

            _processor.StreamProcessor.PCAPFileReadFinished();

            menuPacketDump.Enabled = true;

            menuViewPackets.Enabled = true;

            Log("Stream recognised as " + _processor.StreamProcessor.GetDecoderVersion());

            var ppLength = _processor.StreamProcessor.VerifyPlayerProfile();

            ClientVersionLabel.Text = _processor.StreamProcessor.GetDecoderVersion();

            if (ppLength == 0)
            {
                Log("Unable to find player profile packet, or packet not of correct size.");
                menuDumpAAs.Enabled = false;
                menuGenerateSQL.Enabled = false;
                ClientVersionLabel.ForeColor = Color.Red;
                ZoneLabel.Text = "";
                PacketCountLabel.Text = "";
                StatusBar.Text = "Unrecognised EQ Client Version. Press Ctrl-P to dump, or Ctrl-V to view packets.";
                return;
            }
            ClientVersionLabel.ForeColor = Color.Green;
            Log("Found player profile packet of the expected length (" + ppLength + ").");

            if (_processor.StreamProcessor.SupportsSQLGeneration())
                StatusBar.Text = "Client version recognised. Press Ctrl-S to Generate SQL";
            else
                StatusBar.Text = "Client version recognised. *SQL GENERATION NOT SUPPORTED FOR THIS CLIENT*";

            _zoneName = _processor.StreamProcessor.GetZoneName();

            UInt32 zoneNumber = _processor.StreamProcessor.GetZoneNumber();

            Log("Zonename is " + _processor.StreamProcessor.GetZoneName());

            Log("Zone number is " + zoneNumber);

            ZoneLabel.Text = _processor.StreamProcessor.GetZoneLongName() + " [" + _processor.StreamProcessor.GetZoneName() + "] (" + zoneNumber.ToString() + ")";

            _sqlForm.ZoneIDTextBox.Text = zoneNumber.ToString();
            _sqlForm.ZoneIDTextBox.Enabled = true;
            _sqlForm.DoorsTextBox.Enabled = true;
            _sqlForm.NPCTypesTextBox.Enabled = true;
            _sqlForm.SpawnEntryTextBox.Enabled = true;
            _sqlForm.SpawnGroupTextBox.Enabled = true;
            _sqlForm.Spawn2TextBox.Enabled = true;
            _sqlForm.GridTextBox.Enabled = true;
            _sqlForm.ObjectTextBox.Enabled = true;
            _sqlForm.GroundSpawnTextBox.Enabled = true;
            _sqlForm.MerchantTextBox.Enabled = true;
            _sqlForm.VersionSelector.Enabled = true;
            menuGenerateSQL.Enabled = _processor.StreamProcessor.SupportsSQLGeneration();
            menuPacketDump.Enabled = true;
            menuViewPackets.Enabled = true;
            menuDumpAAs.Enabled = true;

            _sqlForm.RecalculateBaseInsertIDs();

            _processor.StreamProcessor.GenerateZonePointList();
        }


        private void menuGenerateSQL_Click(object sender, EventArgs e)
        {
            if (_sqlForm.ShowDialog() != DialogResult.OK)
                return;
            try
            {
                var config = new SqlGeneratorConfiguration
                {
                    SpawnDBID = Convert.ToUInt32(_sqlForm.NPCTypesTextBox.Text),
                    SpawnGroupID = Convert.ToUInt32(_sqlForm.SpawnGroupTextBox.Text),
                    SpawnEntryID = Convert.ToUInt32(_sqlForm.SpawnEntryTextBox.Text),
                    Spawn2ID = Convert.ToUInt32(_sqlForm.Spawn2TextBox.Text),
                    GridDBID = Convert.ToUInt32(_sqlForm.GridTextBox.Text),
                    MerchantDBID = Convert.ToUInt32(_sqlForm.MerchantTextBox.Text),
                    DoorDBID = Convert.ToInt32(_sqlForm.DoorsTextBox.Text),
                    GroundSpawnDBID = Convert.ToUInt32(_sqlForm.GroundSpawnTextBox.Text),
                    ObjectDBID = Convert.ToUInt32(_sqlForm.ObjectTextBox.Text),
                    ZoneID = Convert.ToUInt32(_sqlForm.ZoneIDTextBox.Text),
                    SpawnNameFilter = _sqlForm.SpawnNameFilter.Text,
                    CoalesceWaypoints = _sqlForm.CoalesceWaypoints.Checked,
                    GenerateZone = _sqlForm.ZoneCheckBox.Checked,
                    GenerateZonePoint = _sqlForm.ZonePointCheckBox.Checked,
                    ZoneName = _zoneName,
                    SpawnVersion = (UInt32)_sqlForm.VersionSelector.Value,
                    GenerateDoors = _sqlForm.DoorCheckBox.Checked,
                    GenerateSpawns = _sqlForm.SpawnCheckBox.Checked,
                    GenerateGrids = _sqlForm.GridCheckBox.Checked,
                    GenerateMerchants = _sqlForm.MerchantCheckBox.Checked,
                    UpdateExistingNPCTypes = _sqlForm.UpdateExistingNPCTypesCheckbox.Checked,
                    UseNPCTypesTint = _sqlForm.NPCTypesTintCheckBox.Checked,
                    GenerateInvisibleMen = _sqlForm.InvisibleMenCheckBox.Checked,
                    GenerateGroundSpawns = _sqlForm.GroundSpawnCheckBox.Checked,
                    GenerateObjects = _sqlForm.ObjectCheckBox.Checked
                };

                var sqlgenerator = new SqlGenerator(_sqlForm.FileName, _processor.StreamProcessor,config,Log,SetStatus);
                sqlgenerator.GenerateSql();
            }
            catch (IOException)
            {
                Log("Unable to open file " + _sqlForm.FileName + " for writing.");
                StatusBar.Text = "Unable to open file " + _sqlForm.FileName + " for writing.";
            }

        }

        private void menuPacketDump_Click(object sender, EventArgs e)
        {
            if (PacketDumpFileDialog.ShowDialog() != DialogResult.OK) return;
            SetStatus("Packet dump in progress. Please wait...");
            Log("Packets dump in progress...");
            DisableAllControls();
            Application.DoEvents();
            var bw = new BackgroundWorker();
            bw.DoWork += DumpPackets;
            bw.RunWorkerCompleted += EnableAllControlsOnCompletion;
            bw.RunWorkerAsync();
        }

        private void EnableAllControlsOnCompletion(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableAllControls();
        }

        private void DumpPackets(object sender, DoWorkEventArgs e)
        {
            if (_processor != null && _processor.StreamProcessor.DumpPackets(PacketDumpFileDialog.FileName, Properties.Settings.Default.DumpTimeStamps))
            {
                SetStatus("Packets dumped successfully.");
                Log("Packets dumped successfully.");
            }
            else
            {
                SetStatus("Packet dump failed.");
                Log("Packet dump failed.");
            }
        }

        private void menuDumpAAs_Click(object sender, EventArgs e)
        {
            if (PacketDumpFileDialog.ShowDialog() != DialogResult.OK) return;
            Log("AA dump in progress...");
            DisableAllControls();
            var bw = new BackgroundWorker();
            bw.DoWork += DumpAAs;
            bw.RunWorkerCompleted += EnableAllControlsOnCompletion;
            bw.RunWorkerAsync();
        }

        private void DumpAAs(object sender, DoWorkEventArgs e)
        {
            if (_processor != null && _processor.StreamProcessor.DumpAAs(PacketDumpFileDialog.FileName))
            {
                SetStatus("AAs dumped successfully.");
                Log("AAs dumped successfully.");
            }
            else
            {
                SetStatus("AA dumped failed.");
                Log("AA dump failed.");
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuViewDebugLog_Click(object sender, EventArgs e)
        {
            menuViewDebugLog.Checked = _debugLog.Visible;

            if (!menuViewDebugLog.Checked)
            {
                menuViewDebugLog.Checked = true;
                ShowDebugLog();

            }
            else
            {
                menuViewDebugLog.Checked = false;
                _debugLog.Hide();
            }
        }

        private void menuViewPackets_Click(object sender, EventArgs e)
        {
            if (_processor == null) return;
            DisableAllControls();

            Application.DoEvents();

            var textFileViewer = Properties.Settings.Default.TextFileViewer;

            var tempFileName = Path.GetTempFileName();

            if (_processor.StreamProcessor.DumpPackets(tempFileName, Properties.Settings.Default.DumpTimeStamps))
            {
                try
                {
                    System.Diagnostics.Process.Start(textFileViewer, tempFileName);
                }
                catch
                {
                    StatusBar.Text = "Unable to launch " + textFileViewer;
                }
            }
            else
            {
                StatusBar.Text = "Unexpected error while generating temporary file.";
            }
            EnableAllControls();
        }

        private void EQExtractor2Form1_Load(object sender, EventArgs e)
        {
            Text = _versionText;
            if (Properties.Settings.Default.ShowDebugWindowOnStartup)
            {
                ShowDebugLog();
            }
        }

        private void DisplayUsageInfo()
        {
            Log(_versionText + " Initialised.");
            Log("");
            Log("Instructions:");
            Log("Generate a .pcap file using Wireshark. To do this, park a character in the zone you want to collect in.");
            Log("Camp to character select. Start Wireshark capturing. Zone your character in and just sit around for a");
            Log("while, or go and inspect merchant inventories if you want to collect those. When finished, stop the");
            Log("Wireshark capture and save it (File/Save As).");
            Log("");
            Log("Load the .pcap file into this program by pressing Ctrl-L.");
            Log("To generate SQL, press Ctrl-S and select the check boxes and set the starting SQL INSERT IDs as required.");
            Log("Review the generated SQL before sourcing as DELETEs are auto-generated.");
            Log("Press Ctrl-V to view packets, or Ctrl-D to dump them to a text file.");
            Log("");
        }

        private void menuOptions_Click(object sender, EventArgs e)
        {

            if (_options.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.TextFileViewer = _options.PacketDumpViewerProgram.Text;
                Properties.Settings.Default.ShowDebugWindowOnStartup = _options.ShowDebugWindowOnStartup.Checked;
                Properties.Settings.Default.DumpTimeStamps = _options.ShowTimeStamps.Checked;
                Properties.Settings.Default.Save();

                if (Properties.Settings.Default.ShowDebugWindowOnStartup)
                {
                    if (!_debugLog.Visible)
                        ShowDebugLog();
                }
                else
                {
                    if (!_debugLog.Visible) return;
                    _debugLog.Hide();
                    menuViewDebugLog.Checked = false;
                }
            }
            else
            {
                _options.PacketDumpViewerProgram.Text = Properties.Settings.Default.TextFileViewer;
                _options.ShowDebugWindowOnStartup.Checked = Properties.Settings.Default.ShowDebugWindowOnStartup;
                _options.ShowTimeStamps.Checked = Properties.Settings.Default.DumpTimeStamps;
            }
        }

        private void ShowDebugLog()
        {
            _debugLog.Left = this.Location.X;
            _debugLog.Top = this.Location.Y + this.Height;
            _debugLog.Show();
            menuViewDebugLog.Checked = true;
            this.Focus();
        }

        private void menuView_Popup(object sender, EventArgs e)
        {
            menuViewDebugLog.Checked = _debugLog.Visible;
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            SeqPatchImporter.Import();
        }
    }
}

