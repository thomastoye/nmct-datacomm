using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USB_DAQBoard;
using System.Windows.Controls;

namespace nmct.datacomm.labo1.test1
{
    public class LedControlLayer
    {
        private Int16 teller = 0;
        private CheckBox[] checkboxes;

        public LedControlLayer(CheckBox[] checkboxes) {
            this.checkboxes = checkboxes;
        }

        public void TurnOnAllLeds()
        {
            MPUSB.WriteDigitalOutPortD(0xFF);
            foreach(CheckBox c in checkboxes)
                c.IsChecked = true;
        }

        public void TurnOffAllLeds()
        {
            MPUSB.WriteDigitalOutPortD(0x00);
            foreach (CheckBox c in checkboxes)
                c.IsChecked = false;
        }

        public void IncreaseTeller()
        {
            this.teller++;
            if (teller > Math.Pow(2, 8))
                teller = (Int16)Math.Pow(2, 8);
            MPUSB.WriteDigitalOutPortD(teller);
        }

        public void DecreaseTeller()
        {
            this.teller--;
            if (teller < 0)
                teller = 0;
            MPUSB.WriteDigitalOutPortD(teller);
        }

        public byte GetLedStatus()
        {
            return (byte)MPUSB.ReadDigitalOutPortD();
        }

        public void SetLedStatus(byte status)
        {
            bool led1 = (status & 1) != 0;
            bool led2 = (status & 2) != 0;
            bool led3 = (status & 4) != 0;
            bool led4 = (status & 8) != 0;
            bool led5 = (status & 16) != 0;
            bool led6 = (status & 32) != 0;
            bool led7 = (status & 64) != 0;
            bool led8 = (status & 128) != 0;

            checkboxes[0].IsChecked = led1;
            checkboxes[1].IsChecked = led2;
            checkboxes[2].IsChecked = led3;
            checkboxes[3].IsChecked = led4;
            checkboxes[4].IsChecked = led5;
            checkboxes[5].IsChecked = led6;
            checkboxes[6].IsChecked = led7;
            checkboxes[7].IsChecked = led8;
        }
    }
}
