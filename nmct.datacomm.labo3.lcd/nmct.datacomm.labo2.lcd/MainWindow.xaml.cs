using System;
using System.Collections.Generic;
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

using TechUSBLCDSimulator.Lib;

namespace nmct.datacomm.labo2.lcd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MPUSB.OpenMPUSBDevice();
        }

        private void EHoogData()
        {
            int value = MPUSB.ReadDigitalOutPortD();
            value = value & 0x0FF;
            value = value | 0x500;
            MPUSB.WriteDigitalOutPortD((short)value);
            MPUSB.Wait(25);
        }

        private void ELaagData()
        {
            int value = MPUSB.ReadDigitalOutPortD();
            value = value & 0x0FF;
            value = value | 0x400;
            MPUSB.WriteDigitalOutPortD((short)value);
            MPUSB.Wait(25);
        }

        private void EHoogInstructie()
        {
            int value = MPUSB.ReadDigitalOutPortD();
            value = value & 0x0FF;
            value = value | 0x100;
            MPUSB.WriteDigitalOutPortD((short)value);
            MPUSB.Wait(25);
        }

        private void ELaagInstructie()
        {
            int value = MPUSB.ReadDigitalOutPortD();
            value = value & 0x0FF;
            MPUSB.WriteDigitalOutPortD((short)value);
            MPUSB.Wait(25);
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            InitialisatieLCD();
            ClearLCD();
        }

        private void InitialisatieLCD()
        {
            DisplayOn();
            FunctionSet();
        }

        private void DisplayOn()
        {
            //0000001111 -> 0x00F
            EHoogInstructie();
            DataVeranderen(0x00F);
            ELaagInstructie();
        }

        private void FunctionSet()
        {
            //0000111000 -> 0x038
            EHoogInstructie();
            DataVeranderen(0x038);
            ELaagInstructie();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            ClearLCD();
            char[] textAscii = txtData.Text.ToCharArray();

            for (int i = 0; i < textAscii.Length; i++)
            {
                if (i == 16)
                {
                    EHoogInstructie();
                    DataVeranderen(0x0C0);
                    ELaagInstructie();
                }
                EHoogData();
                DataVeranderen(textAscii[i]);
                ELaagData();
            }
        }

        // helpers

        private byte[] StringToASCII(string text)
        {
            // Convert the string into a byte[].
            byte[] asciiBytes = Encoding.ASCII.GetBytes(text);
            return asciiBytes;
        }

        private void DataVeranderen(int data)
        {
            int value = MPUSB.ReadDigitalOutPortD();
            value = value & 0xF00;
            value = value | data;
            MPUSB.WriteDigitalOutPortD((short)value);
            MPUSB.Wait(25);
        }
        private void ClearLCD()
        {
            //0000000001 -> 0x001
            EHoogInstructie();
            DataVeranderen(0x001);
            ELaagInstructie();
        }
    }
}
