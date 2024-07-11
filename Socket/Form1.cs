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
using System.Net;

namespace Socket
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Button requestButton;
        private TextBox computerNameTextBox;
        private ListBox drivesListBox;
        private ListBox programsListBox;
        private TcpListener server;
        private TcpClient connectedClient;


        private void Form1_Load(object sender, EventArgs e)
        {
            requestButton = new Button() { Text = "Bilgisayar Bilgilerini İste", Dock = DockStyle.Top };
            computerNameTextBox = new TextBox() { Dock = DockStyle.Top };
            drivesListBox = new ListBox() { Dock = DockStyle.Top };
            programsListBox = new ListBox() { Dock = DockStyle.Fill };

            requestButton.Click += RequestButton_Click;

            this.Controls.Add(programsListBox);
            this.Controls.Add(drivesListBox);
            this.Controls.Add(computerNameTextBox);
            this.Controls.Add(requestButton);

            Task.Run(() => StartServer());
        }

        private void StartServer()
        {
            server = new TcpListener(IPAddress.Any, 8888);
            server.Start();

            while (true)
            {
                connectedClient = server.AcceptTcpClient();
            }
        }

        private async void RequestButton_Click(object sender, EventArgs e)
        {
            if (connectedClient == null)
            {
                MessageBox.Show("Bağlı bir istemci yok.");
                return;
            }

            NetworkStream stream = connectedClient.GetStream();
            byte[] request = Encoding.UTF8.GetBytes("REQUEST_COMPUTER_INFO");
            await stream.WriteAsync(request, 0, request.Length);

            byte[] buffer = new byte[4096];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string[] parts = response.Split('|');
            string computerName = parts[0];
            string[] drives = parts[1].Split(',');
            string[] programs = parts[2].Split(',');

            computerNameTextBox.Text = computerName;
            drivesListBox.Items.Clear();
            programsListBox.Items.Clear();

            foreach (var drive in drives)
            {
                drivesListBox.Items.Add(drive);
            }

            foreach (var program in programs)
            {
                programsListBox.Items.Add(program);
            }
        }


    }
}
