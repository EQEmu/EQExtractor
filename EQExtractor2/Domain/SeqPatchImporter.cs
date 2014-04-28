using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace EQExtractor2.Domain
{
    class SeqPatchImporter
    {
        public static void Import()
        {
            using (var dlg = new OpenFileDialog())
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var doc = new XmlDocument();
                using (var fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.None))
                {
                    var settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.None;
                    settings.DtdProcessing = DtdProcessing.Ignore;
                    settings.XmlResolver = new NullXmlResolver();
                    using (var reader = XmlReader.Create(fs, settings))
                    {
                        doc.Load(reader);
                    }
                }
                var seqopcodes = doc.SelectSingleNode("//seqopcodes");
                if (seqopcodes == null) return;
                var opcodes = seqopcodes.SelectNodes("//opcode");
                if (opcodes == null) return;
                var register = new ZoneOpCodeRegister();
                foreach (XmlNode opcode in opcodes)
                {
                    if (opcode.Attributes == null) continue;
                    var item = new OpcodeItem
                    {
                        Name = opcode.Attributes["name"].Value,
                        Opcode = opcode.Attributes["id"].Value
                    };
                    DateTime dt;

                    var updateValue = string.Empty;
                    if (opcode.Attributes["updated"] != null)
                    {
                        updateValue = opcode.Attributes["updated"].Value;
                    }
                    else if (opcode.Attributes["update"] != null)
                    {
                        updateValue = opcode.Attributes["update"].Value;
                    }
                    if (!string.IsNullOrEmpty(updateValue) && DateTime.TryParse(updateValue, out dt))
                    {
                        item.Updated = dt;
                    }
                    if (!register.Opcodes.ContainsKey(item.Name))
                    {
                        register.Opcodes.Add(item.Name, item);
                    }
                }
                //total hack
                var configFileLines = new List<string>();
                var line = string.Empty;

                // Read the file and display it line by line.
                using (var file = new StreamReader("patch_SoD.conf"))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        configFileLines.Add(line);
                    }
                }
                var found = 0;
                foreach (var opcodeItem in register.Opcodes)
                {
                    //get the index of the first item with it
                    var item = configFileLines.SingleOrDefault(x => x.StartsWith(opcodeItem.Key + "="));
                    if (item == null) continue;
                    var index = configFileLines.IndexOf(item);
                    if (index <= 0) continue;
                    configFileLines[index] = string.Format("{0}=0x{1}", opcodeItem.Key, opcodeItem.Value.Opcode);
                    found++;
                }
                var maxDate = register.Opcodes.Values.Select(x => x.Updated).Max();
                Debug.WriteLine("Found {0} matches", found);
                using (var svDlg = new SaveFileDialog())
                {
                    svDlg.DefaultExt = ".conf";
                    svDlg.FileName = string.Format("patch_{0}", maxDate.ToString("MMMdd-yyyy"));
                    if (svDlg.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllLines(svDlg.FileName, configFileLines);
                    }
                }


            }
        }
    }
}
