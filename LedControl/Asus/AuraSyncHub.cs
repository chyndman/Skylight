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
    public class AuraSyncHub : Hub<Tuple<uint, uint>>
    {
        private IAuraSdk sdk;
        private IAuraSyncDeviceCollection devs;
        private Dictionary<Tuple<uint, uint>, AuraSyncRgbLed> rgbLedsCache;

        public AuraSyncHub()
        {
            sdk = new AuraSdk();
            rgbLedsCache = new Dictionary<Tuple<uint, uint>, AuraSyncRgbLed>();
        }

        public override void Initialize()
        {
            sdk.SwitchMode();
            devs = sdk.Enumerate(0);
        }

        public override Tuple<uint, uint>[] ScanRgbLeds()
        {
            uint count = 0;
            for (int i = 0; i < devs.Count; i++)
            {
                count += (uint)devs[i].Lights.Count;
            }
            Tuple<uint, uint>[] addrs = new Tuple<uint, uint>[count];
            
            count = 0;
            for (int i = 0; i < devs.Count; i++)
            {
                for (int j = 0; j < devs[i].Lights.Count; j++)
                {
                    addrs[count++] = new Tuple<uint, uint>((uint)i, (uint)j);
                }
            }

            return addrs;
        }

        public override RgbLed GetRgbLed(Tuple<uint, uint> addr)
        {
            AuraSyncRgbLed led = null;

            if (rgbLedsCache.ContainsKey(addr))
            {
                led = rgbLedsCache[addr];
            }
            else if (addr.Item1 < devs.Count && addr.Item2 < devs[(int)addr.Item1].Lights.Count)
            {
                led = new AuraSyncRgbLed(devs[(int)addr.Item1], addr.Item2);
                rgbLedsCache.Add(addr, led);
            }

            return led;
        }
    }
}
