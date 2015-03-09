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
using TechUSBSimulator.Lib;


namespace nmct.datacomm.labo1.test1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LedControlLayer ledControl;
        private Timer timer = new Timer();
        private byte oldStatus;
        private Timer timerZotteKnop = new Timer();
        private byte zotteKnopAbOud;

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
            bg.RunWorkerAsync();

            timer.AutoReset = true;
            timer.Interval = 1;
            timer.Elapsed += timerNew_Elapsed;
            timer.Start();
        }

        void timerNew_Elapsed(object sender, ElapsedEventArgs e)
        {
            byte zotteKnopAb = MPUSB.ReadDigitalInPortB();
            zotteKnopAb = (byte)(zotteKnopAb >> 6);

            if (zotteKnopAb == zotteKnopAbOud) return;

            bool? draaiNaarLinks = null;
            if (zotteKnopAbOud == 0x3) // 11b
            {
                if (zotteKnopAb == 0x2) // 10b
                {
                    draaiNaarLinks = true;
                } else {
                    draaiNaarLinks = false;
                }
            }
            else if(zotteKnopAbOud == 0x0) // 00
            {
                if (zotteKnopAb == 0x2) // 10
                {
                    draaiNaarLinks = false;
                }
                else
                {
                    draaiNaarLinks = true;
                }
            }

            zotteKnopAbOud = zotteKnopAb;

            Dispatcher.Invoke(new Action(() => {
                if (draaiNaarLinks.HasValue)
                {
                    if (draaiNaarLinks.Value)
                    {
                        lblDraai.Content = "links";
                    }
                    else
                    {
                        lblDraai.Content = "rechts";
                    }
                }
            }));
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
                    Dispatcher.Invoke(new Action(() => {
                        chkButton1.IsChecked = newStatus.Button1;
                        chkButton2.IsChecked = newStatus.Button2;
                        lblDigitalB.Content = Convert.ToString(portB, 2);
                        lblDigitalD.Content = Convert.ToString(portD, 2);
                    }));
                }

                System.Threading.Thread.Sleep(100);
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            byte status = ledControl.GetLedStatus();
            if (status != oldStatus)
            {
                Dispatcher.Invoke(new Action(() => ledControl.SetLedStatus(status)));
                oldStatus = status;
            }
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
            ledControl.ShiftOmhoog();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            ledControl.ShiftOmlaag();
        }

        private void Button_ReadOutputs_Click(object sender, RoutedEventArgs e)
        {
            lblDigitalD.Content = Convert.ToString(MPUSB.ReadDigitalOutPortD(), 2);
            lblDigitalB.Content = Convert.ToString(MPUSB.ReadDigitalInPortB(), 2);
        }

        private void Button_SetChecks_Click(object sender, RoutedEventArgs e)
        {
            this.Button_ReadOutputs_Click(null, null);
            ledControl.SetLedStatus(ledControl.GetLedStatus());
        }

        private void ClickCheckBox(int number, bool turnOn)
        {
            if (turnOn)
            {
                ledControl.TurnOnSpecificLed(number);
            }
            else
            {
                ledControl.TurnOffSpecificLed(number);
            }
        }

        #region Checkbox event handlers (warning: ugly code)

        private void chk1_Checked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(0, true);
        }

        private void chk2_Checked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(1, true);
        }

        private void chk3_Checked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(2, true);
        }

        private void chk4_Checked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(3, true);
        }

        private void chk5_Checked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(4, true);
        }

        private void chk6_Checked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(5, true);
        }

        private void chk7_Checked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(6, true);
        }

        private void chk8_Checked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(7, true);
        }

        private void chk1_Unchecked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(0, false);
        }

        private void chk2_Unchecked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(1, false);
        }

        private void chk3_Unchecked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(2, false);
        }

        private void chk4_Unchecked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(3, false);
        }

        private void chk5_Unchecked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(4, false);
        }

        private void chk6_Unchecked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(5, false);
        }

        private void chk7_Unchecked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(6, false);
        }

        private void chk8_Unchecked(object sender, RoutedEventArgs e)
        {
            ClickCheckBox(7, false);
        }

        #endregion

        private void btnCounter_Click(object sender, RoutedEventArgs e)
        {
            ledControl.IncreaseTeller();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < e.OldValue)
            {
                // naar rechts
                ledControl.ShiftOmhoog();
            }
            else
            {
                // naar links
                ledControl.ShiftOmlaag();
            }
        }

    }
}
