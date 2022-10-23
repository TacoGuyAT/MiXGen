using Microsoft.Win32;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiXGen {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Load(object sender, RoutedEventArgs e) {
            var fdMiNET = new VistaFolderBrowserDialog();
            fdMiNET.Description = "Open MiNET source code folder";

            var fdgopher = new VistaFolderBrowserDialog();
            fdgopher.Description = "Open gophertunnel source code folder";
            
            if(fdMiNET.ShowDialog() == false)
                return;

            if(fdgopher.ShowDialog() == false)
                return;

            Data.Protocol.LoadXML(fdMiNET.SelectedPath, Data.Protocol.PacketsCurrent);
            //Data.Protocol.PacketsNew = Data.Protocol.PacketsCurrent;
            Data.Protocol.GenerateProtocolFromSource(fdgopher.SelectedPath);

            ProtocolInfo.Text = $"MiNET: {Data.Protocol.MiNETInfo.GameVersion} ({Data.Protocol.MiNETInfo.ProtocolVersion}) | gophertunnel: {Data.Protocol.gophertunnelInfo.GameVersion} ({Data.Protocol.gophertunnelInfo.ProtocolVersion})";

            File.WriteAllText("result.json", JsonConvert.SerializeObject(Data.Protocol.PacketsNew, Formatting.Indented));
            MessageBox.Show("Saved to \"result.json\"", "MiXGen");
        }
        private void Save(object sender, RoutedEventArgs e) {
            MessageBox.Show(Data.Protocol.ToMiNETPacketName("_", 0xFE));
        }
    }
}
