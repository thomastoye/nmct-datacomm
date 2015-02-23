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
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            MPUSB.OpenMPUSBDevice();
        }

        private void btnWriteLcd_Click(object sender, RoutedEventArgs e)
        {
            SendInstructie(0x38); // function set => 0011 1000
            //SendInstructie(0x0F); // display on => 0000 1111
            SendInstructie(0x08); // 1111
            SendInstructie(0x01);
            SendInstructie(0x02);
            //SendInstructie(0x02);
            SendChar('A');
            SendChar('B');
            SendChar('5');

        }

        private void btnDisplayOff_Click(object sender, RoutedEventArgs e)
        {
            SendInstructie(0x8); // 1000
        }

        private void SendInstructie(byte i)
        {
            EHoogInstructie();
            WriteDataLijnen(i); // moet dit niet naar links geshift worden?
            ELaagInstructie();
            System.Threading.Thread.Sleep(20);
        }

        private void SendChar(char c)
        {
            System.Console.WriteLine(Convert.ToByte(c));
            EHoogData();
            WriteDataLijnen(Convert.ToByte(c));
            ELaagData();
            System.Threading.Thread.Sleep(20);
        }

        private void WriteDataLijnen(byte data)
        {
            int x = MPUSB.ReadDigitalOutPortD();
            int y = x & 0xF0;
            int z = y | data;
            MPUSB.WriteDigitalOutPortD((Int16)z);
        }

        private void WriteStuurLijnen(Int16 a)
        {
            // RS RW E
            int x = MPUSB.ReadDigitalOutPortD();
            int y = x & 0xF8FF;
            int z = y | a;
            Console.WriteLine("Schrijven (stuurlijnen): " + z);
            MPUSB.WriteDigitalOutPortD((Int16)z);
        }

        private void EHoogInstructie()
        {
            Console.WriteLine("Schrijven (stuurlijnen): EHoogInstr");
            WriteStuurLijnen(0x0100);
        }

        private void ELaagInstructie()
        {
            Console.WriteLine("Schrijven (stuurlijnen): ELaagInstr");
            WriteStuurLijnen(0x0000);
        }

        private void EHoogData()
        {
            Console.WriteLine("Schrijven (stuurlijnen): EHoogData");
            WriteStuurLijnen(0x0500);
        }

        private void ELaagData()
        {
            Console.WriteLine("Schrijven (stuurlijnen): ELaagData");
            WriteStuurLijnen(0x0400);
        }

    }
}
