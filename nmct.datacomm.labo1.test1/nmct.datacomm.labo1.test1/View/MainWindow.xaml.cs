using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using USB_DAQBoard;


namespace nmct.datacomm.labo1.test1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LedControlLayer ledControl;
        Timer timer = new Timer();

        public MainWindow()
        {
            InitializeComponent();

            CheckBox[] leds = new CheckBox[8];
            leds[0] = chk1;
            leds[1] = chk2;
            leds[2] = chk3;
            leds[3] = chk4;
            leds[4] = chk5;
            leds[5] = chk6;
            leds[6] = chk7;
            leds[7] = chk8;

            ledControl = new LedControlLayer(leds);

            MPUSB.OpenMPUSBDevice();

            timer.AutoReset = true;
            timer.Interval = 200;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += bg_DoWork;
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            BoardStatus oldStatus = null;
            while (true)
            {
                byte portD = (byte)MPUSB.ReadDigitalOutPortD();
                byte portB = (byte)MPUSB.ReadDigitalInPortB();
                BoardStatus newStatus = new BoardStatus();

                // ingedrukt = nul
                newStatus.Button1 = (portB & 1) == 0;
                newStatus.Button2 = (portB & 2) == 0;

                if (oldStatus != newStatus)
                {
                    // report progress
                }
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            byte status = ledControl.GetLedStatus();
            //ledControl.SetLedStatus(status);
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            MPUSB.OpenMPUSBDevice();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            MPUSB.CloseMPUSBDevice();
        }

        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            txbVersion.Text = MPUSB.GetVersion();
        }

        private void btnLedsOn_Click(object sender, RoutedEventArgs e)
        {
            ledControl.TurnOnAllLeds();
        }

        private void btnLedsOff_Click(object sender, RoutedEventArgs e)
        {
            ledControl.TurnOffAllLeds();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            ledControl.IncreaseTeller();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            ledControl.DecreaseTeller();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lblDigitalD.Content = Convert.ToString(MPUSB.ReadDigitalOutPortD(), 2);
            lblDigitalB.Content = Convert.ToString(MPUSB.ReadDigitalInPortB(), 2);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Button_Click(null, null);
            ledControl.SetLedStatus(ledControl.GetLedStatus());
        }

    }
}
