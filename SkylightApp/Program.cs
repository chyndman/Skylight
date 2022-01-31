// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SkylightApp
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Thread thdFx = new Thread(new ParameterizedThreadStart(SkylightFxEngine.Thread.run));
            thdFx.Start(cts.Token);

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            cts.Cancel();
            thdFx.Join();
        }
    }
}
