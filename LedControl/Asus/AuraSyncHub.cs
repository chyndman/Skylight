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
    public class AuraSyncHub : Hub<Tuple<int, int>>
    {
        private IAuraSdk sdk;
        private IAuraSyncDeviceCollection devs;
        private Dictionary<Tuple<int, int>, AuraSyncRgbLed> rgbLedsCache;

        public AuraSyncHub()
        {
            sdk = new AuraSdk();
            rgbLedsCache = new Dictionary<Tuple<int, int>, AuraSyncRgbLed>();
        }

        public override void Initialize()
        {
            sdk.SwitchMode();
            devs = sdk.Enumerate(0);
        }

        public override Tuple<int, int>[] ScanRgbLeds()
        {
            int count = 0;
            for (int i = 0; i < devs.Count; i++)
            {
                count += devs[i].Lights.Count;
            }
            Tuple<int, int>[] addrs = new Tuple<int, int>[count];
            
            count = 0;
            for (int i = 0; i < devs.Count; i++)
            {
                for (int j = 0; j < devs[i].Lights.Count; j++)
                {
                    addrs[count++] = new Tuple<int, int>(i, j);
                }
            }

            return addrs;
        }

        public override RgbLed GetRgbLed(Tuple<int, int> addr)
        {
            AuraSyncRgbLed led = null;

            if (rgbLedsCache.ContainsKey(addr))
            {
                led = rgbLedsCache[addr];
            }
            else if (addr.Item1 < devs.Count && addr.Item2 < devs[addr.Item1].Lights.Count)
            {
                led = new AuraSyncRgbLed(devs[addr.Item1], addr.Item2);
                rgbLedsCache.Add(addr, led);
            }

            return led;
        }
    }
}
