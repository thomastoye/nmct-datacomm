using System;
using System.Collections;
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

namespace nmct.datacomm.labo2.i2c
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte address = 64; // 0b0100 0000

        private const int SDA = 0x1;
        private const int SCL = 0x2;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            MPUSB.OpenMPUSBDevice();

            // Startconditie
            StartConditie();

            Wait();

            WriteSlaveAddress();

            StopConditie();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ZendCijfer(new Random().Next(0, 9));

            StopConditie();
        }

        private void ZendEen()
        {
            MPUSB.WriteDigitalOutPortD(SDA);
            MPUSB.WriteDigitalOutPortD(SDA | SCL);
            Wait();
            MPUSB.WriteDigitalOutPortD(0);
            System.Console.WriteLine("Een gestuurd");
        }

        private void ZendNul()
        {
            MPUSB.WriteDigitalOutPortD(0);
            MPUSB.WriteDigitalOutPortD(SCL);
            Wait();
            MPUSB.WriteDigitalOutPortD(0);
            System.Console.WriteLine("Nul gestuurd");
        }

        private void Wait()
        {
            System.Threading.Thread.Sleep(10);
        }

        private void WriteSlaveAddress()
        {
            ZendByte(this.address);
        }

        private void StartConditie()
        {
            MPUSB.WriteDigitalOutPortD(SDA | SCL);
            Wait();
            MPUSB.WriteDigitalOutPortD(SCL);
            Wait();
        }

        private void StopConditie()
        {
            MPUSB.WriteDigitalOutPortD(SCL);
            Wait();
            MPUSB.WriteDigitalOutPortD(SDA | SCL);
            Wait();
        }

        private void ZendCijfer(int cijfer)
        {
            if (cijfer < 0 || cijfer > 9) throw new Exception("Cijfer moet tussen nul en negen liggen");

            byte value = 0;

            if (cijfer == 0)
            {
                value = 0x21;
            }
            else if (cijfer == 1)
            {
                value = 0xbd;
            }
            else if (cijfer == 2)
            {
                value = 0x13;
            }
            else if (cijfer == 3)
            {
                value = 0x19;
            }
            else if (cijfer == 4)
            {
                value = 0x8d;
            }
            else if (cijfer == 5)
            {
                value = 0x49;
            }
            else if (cijfer == 6)
            {
                value = 0x41;
            }
            else if (cijfer == 7)
            {
                value = 0x40;
            }
            else if (cijfer == 8)
            {
                value = 0x1;
            }
            else if (cijfer == 9)
            {
                value = 0x9;
            }

            System.Console.WriteLine("-- Cijfer " + cijfer + " wordt gestuurd. Byte is " + value);

            ZendByte(value);
        }

        private void ZendByteReverse(byte toSend)
        {
            bool bit0 = (byte)(toSend & 1) == 0;
            bool bit1 = (byte)(toSend & 2) == 0;
            bool bit2 = (byte)(toSend & 4) == 0;
            bool bit3 = (byte)(toSend & 8) == 0;
            bool bit4 = (byte)(toSend & 16) == 0;
            bool bit5 = (byte)(toSend & 32) == 0;
            bool bit6 = (byte)(toSend & 64) == 0;
            bool bit7 = (byte)(toSend & 128) == 0;

            ZendEenOfNul(bit0);
            ZendEenOfNul(bit1);
            ZendEenOfNul(bit2);
            ZendEenOfNul(bit3);
            ZendEenOfNul(bit4);
            ZendEenOfNul(bit5);
            ZendEenOfNul(bit6);
            ZendEenOfNul(bit7);
        }

        private void ZendByte(byte toSend)
        {
            bool bit0 = (byte)(toSend & 1) == 0;
            bool bit1 = (byte)(toSend & 2) == 0;
            bool bit2 = (byte)(toSend & 4) == 0;
            bool bit3 = (byte)(toSend & 8) == 0;
            bool bit4 = (byte)(toSend & 16) == 0;
            bool bit5 = (byte)(toSend & 32) == 0;
            bool bit6 = (byte)(toSend & 64) == 0;
            bool bit7 = (byte)(toSend & 128) == 0;

            ZendEenOfNul(bit7);
            ZendEenOfNul(bit6);
            ZendEenOfNul(bit5);
            ZendEenOfNul(bit4);
            ZendEenOfNul(bit3);
            ZendEenOfNul(bit2);
            ZendEenOfNul(bit1);
            ZendEenOfNul(bit0);
        }

        private void ZendEenOfNul(bool zendNul) {
            if (zendNul) ZendNul(); else ZendEen();
        }

    }
}
