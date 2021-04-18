// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuraServiceLib;

namespace LedControl.Asus
{
    class AuraSyncRgbLed : RgbLed
    {
        private IAuraSyncDevice dev;
        private IAuraRgbLight light;

        public AuraSyncRgbLed(IAuraSyncDevice dev, int lightIdx)
        {
            this.dev = dev;
            light = dev.Lights[(int)lightIdx];
        }

        protected override void SetRed(byte r)
        {
            light.Red = r;
            dev.Apply();
        }

        protected override void SetGreen(byte g)
        {
            light.Green = g;
            dev.Apply();
        }

        protected override void SetBlue(byte b)
        {
            light.Blue = b;
            dev.Apply();
        }

        protected override void SetAllChannels(byte r, byte g, byte b)
        {
            uint rgb = ((uint)r) | ((uint)g << 8) | ((uint)b << 16);
            light.Color = rgb;
            dev.Apply();
        }
    }
}
