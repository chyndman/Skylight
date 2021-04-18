// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

using System;
using System.Collections.Generic;

namespace LedControl
{
    public abstract class Hub<TAddress>
    {
        public abstract void Activate();
        public abstract void Deactivate();

        public abstract TAddress[] ScanRgbLeds();
        public abstract RgbLed GetRgbLed(TAddress addr);
        public virtual void Flush() {}
    }
}
