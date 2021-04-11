// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

using System;
using System.Text.RegularExpressions;
using LedControl;

namespace AuraSyncTestCli
{
    class Program
    {
        static void Main(string[] args)
        {
            var hub = new LedControl.Asus.AuraSyncHub();
            hub.Initialize();
            var addrs = hub.ScanRgbLeds();
            var black = new Tuple<byte, byte, byte>(0, 0, 0);

            foreach (var addr in addrs)
            {
                hub.GetRgbLed(addr).Color = black;
            }

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    break;
                }

                Regex regex = new Regex(@"([rgbwo]) ([\d]+) ([\d]+)");
                Match match = regex.Match(line);
                if (match.Success && 4 == match.Groups.Count)
                {
                    byte r = 0;
                    byte g = 0;
                    byte b = 0;
                    switch (match.Groups[1].Value)
                    {
                        case "r":
                            r = 0xff;
                            break;
                        case "g":
                            g = 0xff;
                            break;
                        case "b":
                            b = 0xff;
                            break;
                        case "w":
                            r = 0xff;
                            g = 0xff;
                            b = 0xff;
                            break;
                        default:
                            break;
                    }

                    var dev = Convert.ToUInt32(match.Groups[2].Value);
                    var light = Convert.ToUInt32(match.Groups[3].Value);
                    var addr = new Tuple<uint, uint>(dev, light);
                    var led = hub.GetRgbLed(addr);

                    if (null == led)
                    {
                        Console.WriteLine("NO LED");
                    }
                    else
                    {
                        led.Color = new Tuple<byte, byte, byte>(r, g, b);
                    }
                }
            }
        }
    }
}
