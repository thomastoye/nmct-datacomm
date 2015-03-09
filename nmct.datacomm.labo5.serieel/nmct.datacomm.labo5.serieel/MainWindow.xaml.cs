using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using TechUSBSimulator.Lib;

namespace nmct.datacomm.labo5.serieel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort sp;
        private string incompleteCommand = "";

        public MainWindow()
        {
            InitializeComponent();
            MPUSB.OpenMPUSBDevice();
            SetStatus("Opened connection to data acquisiton board.");
        }

        private void btnGetAllPorts_Click(object sender, RoutedEventArgs e)
        {
            List<String> poorten = SerialPort.GetPortNames().ToList();
            cmdAllPorts.Items.Clear();
            poorten.ForEach(p => cmdAllPorts.Items.Add(p));
            if (cmdAllPorts.Items.Count > 0) cmdAllPorts.SelectedIndex = 0;
        }

        private void SetStatus(string status)
        {
            lblStatus.Content = status;
        }

        private void CloseConnection()
        {
            SetStatus("Closing connection...");
            if (sp == null)
            {
                SetStatus("No serial port.");
            }
            else if (!sp.IsOpen)
            {
                SetStatus("Connection was not open.");
            }
            else
            {
                sp.Close();
                SetStatus("Connection closed.");
            }
        }

        private void MakeConnection(string port)
        {
            CloseConnection();
            SetStatus("Connecting to port " + port + "...");

            sp = new SerialPort(port, 9600, Parity.Even, 8, StopBits.One);
            sp.ReadTimeout = 10;

            sp.Open();

            sp.DataReceived += sp_DataReceived;

            SetStatus("Connected to port " + port + ".");
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort spL = (SerialPort)sender;
            string data = sp.ReadExisting();

            Console.WriteLine("Received data: " + data);


            foreach (char c in data)
            {
                if (c == '\n' || c == '\r')
                {
                    // end of the command
                    ParseAndExecuteCommand(incompleteCommand);
                    incompleteCommand = ""; // reset the incomplete command, this command was complete
                    Dispatcher.Invoke(new Action(() => txbHuidigCommando.Content = ""));
                }
                else if (c == 127)
                {
                    // backspace
                    if (incompleteCommand.Length != 0)
                    {
                        incompleteCommand = incompleteCommand.Substring(0, incompleteCommand.Length - 1);
                        Dispatcher.Invoke(new Action(() => txbHuidigCommando.Content = incompleteCommand));
                    }
                }
                else
                {
                    // command not done yet, append the char
                    incompleteCommand += c;
                    Dispatcher.Invoke(new Action(() => txbHuidigCommando.Content = incompleteCommand));
                }
            }

            this.Dispatcher.Invoke(new Action(() => txbReceived.AppendText(data)));
        }

        private void ParseAndExecuteCommand(string command)
        {
            bool bekend = true; // of het command bekend is

            // commands
            if (command == "ledsaan")
            {
                MPUSB.WriteDigitalOutPortD(0xFF);
            }
            else if (command == "ledsuit")
            {
                MPUSB.WriteDigitalOutPortD(0x00);
            } else if (new Regex("^(?:ledsaan )[0-7]*$").Match(command).Success) { // ledsaan 0125 turns on leds 0, 1, 2 and 5
                var test = new Regex("^(?:ledsaan )[0-7]*$").Match(command);
                string leds = new Regex("^(?:ledsaan )[0-7]*$").Match(command).Value;
                string substring = leds.Substring(7);
                foreach (char c in substring)
                {
                    int i = (int)Char.GetNumericValue(c);
                    short status = (short)MPUSB.ReadDigitalOutPortD();
                    short toWrite = (short)(status | (short)(Math.Pow(2, i)));
                    MPUSB.WriteDigitalOutPortD(toWrite);
                }
            } else {
                Console.WriteLine("Command niet herkend: " + command);
                bekend = false;
            }


            if (bekend)
            {
                this.Dispatcher.Invoke(new Action(() => lstCommands.Items.Add(command + " <gelukt>")));
            }
            else
            {
                this.Dispatcher.Invoke(new Action(() => lstCommands.Items.Add(command + " <niet gekend>")));
            }
        }

        private void Write(string s)
        {
            sp.Write(s);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string selectedPort = (string)cmdAllPorts.SelectedItem;
            MakeConnection(selectedPort);
        }

        private void btnCloseConnection_Click(object sender, RoutedEventArgs e)
        {
            CloseConnection();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            Write(txbWrite.Text);
        }
    }
}
