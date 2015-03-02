using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace nmct.datacomm.labo4.analoog
{
    public partial class MainWindow : Window
    {
        BackgroundWorker backgroundWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            MPUSB.OpenMPUSBDevice();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerAsync();
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;

            sldKan0.Value = 10.0;
            sldKan1.Value = 5.0;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Tuple<int, int, int, int> tuple =  (Tuple<int, int, int, int>)e.Result;

            kanaal1.Value = tuple.Item1;
            kanaal2.Value = tuple.Item2;
            kanaal3.Value = tuple.Item3;
            kanaal4.Value = tuple.Item4;

            ZetLedsVoorSterktePercent(tuple.Item4);

            backgroundWorker.RunWorkerAsync();
        }

        private void ZetLedsVoorSterktePercent(int p)
        {
            // 1 led  = volledige duisternis (0)
            // 6 leds = volledig licht (100)
            // => 6 statussen
            // tussen 70% en 30%
            
            //int aantalLeds = p / 20 + 1; // niet gekalibreerd

            int gekalibreerdPercent = (int)((p - 30) * 2.5); // p - 30 ligt tussen 0 en 40, maal 2.5 => tussen nul en 100
            gekalibreerdPercent = Math.Min(gekalibreerdPercent, 100);
            gekalibreerdPercent = Math.Max(gekalibreerdPercent, 0);

            int aantalLeds = gekalibreerdPercent / 20 + 1;
            Console.WriteLine("Er moeten " + aantalLeds + " leds gezet worden, p => " + p + ", gekalibreerd => " + gekalibreerdPercent);

            byte status = 0xFF;

            switch (aantalLeds)
            {
                case 1:
                    status = 0x1;
                    break;
                case 2:
                    status = 0x3;
                    break;
                case 3:
                    status = 0x7;
                    break;
                case 4:
                    status = 0xF;
                    break;
                case 5:
                    status = 0x1F;
                    break;
                case 6:
                    status = 0x1F;
                    break;
            }

            MPUSB.WriteDigitalOutPortD(status);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var kanaal1Value = MPUSB.ReadAnalogIn(0);
            var kanaal2Value = MPUSB.ReadAnalogIn(1);
            var kanaal3Value = MPUSB.ReadAnalogIn(2);
            var kanaal4Value = MPUSB.ReadAnalogIn(3);

            System.Console.WriteLine("Kanaal 1: " + kanaal1Value + " -- Kan2: " + kanaal2Value + " -- Kan3: " + kanaal3Value + " -- Kan4: " + kanaal4Value);

            Thread.Sleep(100);
            backgroundWorker.ReportProgress((int)(kanaal1Value / 10.24));

            e.Result = Tuple.Create<int, int, int, int>(
                (int)(kanaal1Value / 10.24),
                (int)(kanaal2Value / 10.24),
                (int)(kanaal3Value / 10.24),
                (int)(kanaal4Value / 10.24)
            );
        }

        private void sldKan0_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MPUSB.WriteAnalogOut(0, (short)(e.NewValue / 10 * 1023));
        }

        private void sldKan1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MPUSB.WriteAnalogOut(1, (short)(e.NewValue / 10 * 1023));
        }


    }
}
