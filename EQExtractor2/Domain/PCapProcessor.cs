using System;
using System.IO;
using System.Windows.Forms;
using SharpPcap;

namespace EQExtractor2.Domain
{

    public class PcapProcessingProgressEventArgs:EventArgs
    {
        public int Progress { get; set; }    
    }
    class PCapProcessor
    {
        public Action<string> Log { get; private set; }
        public Action<string> PacketDebugLogger { get; private set; }
        public Action<string> SetStatus { get; private set; }
        public EQStreamProcessor StreamProcessor { get; set; }
        public UserOptions Options { get; set; }
        public StreamWriter PacketDebugStream { get; set; }
        public int PacketsSeen { get; set; }
        public long BytesRead { get; set; }
        public long CaptureFileSize { get; set; }
        public string ZoneName { get; set; }
        public Action<int> ReportProgress { get; private set; }


        public PCapProcessor(Action<string> logAction, Action<string> packetDebugLogAction,Action<string> setStatusAction, Action<int> reportProgressAction)
        {
            if (logAction == null) throw new ArgumentNullException("logAction");
            if (packetDebugLogAction == null) throw new ArgumentNullException("packetDebugLogAction");
            if (setStatusAction == null) throw new ArgumentNullException("setStatusAction");
            if (reportProgressAction == null) throw new ArgumentNullException("reportProgressAction");
            Log = logAction;
            PacketDebugLogger=packetDebugLogAction;
            SetStatus = setStatusAction;
            ReportProgress = reportProgressAction;
            Options=new UserOptions();
        }

        public void ProcessPCapFile(string capFile)
        {
            if (string.IsNullOrEmpty(capFile)) return;
            OfflinePcapDevice device=null;
            try
            {
                device = new OfflinePcapDevice(capFile);
                device.Open();
            }
            catch (Exception ex)
            {
                if(Log!=null) Log("Error: File does not exist, not in .pcap format ro you don't have winpcap installed.");
                throw new PCapFormatException("Error: File does not exist, not in .pcap format ro you don't have winpcap installed.", ex);
            }
            StreamProcessor = new EQStreamProcessor();

            if (!StreamProcessor.Init(Application.StartupPath + "\\Configs", Log))
            {
                if (Log != null) Log("Fatal error initialising Stream Processor. No decoders could be initialised (mostly likely misplaced patch_XXXX.conf files.");
                if (SetStatus != null) SetStatus("Fatal error initialising Stream Processor. No decoders could be initialised (mostly likely misplaced patch_XXXX.conf files.");
                return;
            }

            if (Options.EQPacketDebugFilename.Text.Length > 0)
            {
                try
                {
                    if(PacketDebugStream==null) PacketDebugStream = new StreamWriter(Options.EQPacketDebugFilename.Text);
                    StreamProcessor.Packets.SetDebugLogHandler(PacketDebugLogger);
                }
                catch
                {
                    if (Log != null) Log("Failed to open netcode debug file for writing.");
                    Options.EQPacketDebugFilename.Text = "";
                    StreamProcessor.Packets.SetDebugLogHandler(null);
                }
            }
            else
                StreamProcessor.Packets.SetDebugLogHandler(null);

            SetStatus( "Reading packets from " + capFile+ ". Please wait...");
            device.OnPacketArrival += device_OnPacketArrival;
            BytesRead = 0;
            PacketsSeen = 0;
            if (Log != null) Log("-- Capturing from '" + capFile);
            ReportProgress(0);
            CaptureFileSize = device.FileSize;
            device.Capture();
            device.Close();
            if(Log!=null)Log("End of file reached. Processed " + PacketsSeen + " packets and " + BytesRead + " bytes.");
        }

        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if (e.Packet.LinkLayerType != PacketDotNet.LinkLayers.Ethernet) return;
            PacketDotNet.Packet packet;

            long totalPacketSize = e.Packet.Data.Length;
            BytesRead += totalPacketSize;
            ++PacketsSeen;

            if ((PacketsSeen > 0) && ((PacketsSeen % 10000) == 0))
            {
                if (Log != null) Log(null);
                ReportProgress( (int)((float)BytesRead / (float)CaptureFileSize * 100));
                Application.DoEvents();
            }

            try
            {
                packet = PacketDotNet.Packet.ParsePacket(e.Packet);
            }
            catch
            {
                return;
            }

            var udpPacket = PacketDotNet.UdpPacket.GetEncapsulated(packet);

            if (udpPacket == null) return;
            var ipPacket = (PacketDotNet.IpPacket)udpPacket.ParentPacket;
            var srcIp = ipPacket.SourceAddress;
            var dstIp = ipPacket.DestinationAddress;

            var payload = udpPacket.PayloadData;

            var l = udpPacket.Length - udpPacket.Header.GetLength(0);

            if (l <= 0) return;
            Array.Resize(ref payload, l);
            StreamProcessor.ProcessPacket(srcIp, dstIp, udpPacket.SourcePort, udpPacket.DestinationPort, payload, packet.Timeval.Date);
        }
    }

    public class PCapFormatException : Exception
    {
        public PCapFormatException(string message, Exception innerException):base(message, innerException)
        {
            
        }
    }
}
