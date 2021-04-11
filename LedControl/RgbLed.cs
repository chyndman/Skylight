using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedControl
{
    public abstract class RgbLed
    {
        private byte r;
        private byte g;
        private byte b;

        public byte Red
        {
            get => r;
            set
            {
                SetRed(r = value);
            }
        }

        public byte Green
        {
            get => g;
            set
            {
                SetGreen(g = value);
            }
        }

        public byte Blue
        {
            get => b;
            set
            {
                SetBlue(b = value);
            }
        }

        public Tuple<byte, byte, byte> Color
        {
            get => new Tuple<byte, byte, byte>(r, g, b);
            set
            {
                SetAllChannels(r = value.Item1, g = value.Item2, b = value.Item3);
            }
        }

        protected abstract void SetRed(byte r);
        protected abstract void SetGreen(byte g);
        protected abstract void SetBlue(byte b);
        protected abstract void SetAllChannels(byte r, byte g, byte b);
    }
}
