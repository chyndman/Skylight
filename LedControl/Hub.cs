using System;
using System.Collections.Generic;

namespace LedControl
{
    public abstract class Hub<TAddress>
    {
        public abstract void Initialize();

        public abstract TAddress[] ScanRgbLeds();
        public abstract RgbLed GetRgbLed(TAddress addr);
    }
}
