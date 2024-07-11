using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Task.Run(() => StartClient());
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async Task StartClient()
        {
            using (TcpClient client = new TcpClient("127.0.0.1", 8888))
            {
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (request == "REQUEST_COMPUTER_INFO")
                    {
                        string computerName = Environment.MachineName;
                        string[] drives = Directory.GetLogicalDrives();
                        string drivesString = string.Join(",", drives);

                        List<string> installedPrograms = GetInstalledPrograms();
                        string programsString = string.Join(",", installedPrograms);

                        string response = $"{computerName}|{drivesString}|{programsString}";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    }
                }
            }
        }

        private List<string> GetInstalledPrograms()
        {
            List<string> programs = new List<string>();

            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                foreach (string subkeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                    {
                        string displayName = (string)subkey.GetValue("DisplayName");
                        if (!string.IsNullOrEmpty(displayName))
                        {
                            programs.Add(displayName);
                        }
                    }
                }
            }

            return programs;
        }

    }
}
