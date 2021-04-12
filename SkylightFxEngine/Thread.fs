// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

namespace SkylightFxEngine

open System.Threading
open LedControl

module Thread =
    let run (param: obj) =
        let hub = new Asus.AuraSyncHub()
        hub.Activate()

        let levels = [
            [ (0, 2) ] 
            [ (0, 1) ]
            [ (0, 0) ]
            [ (1, 4); (1, 5) ]
            [ (1, 3); (1, 0) ]
            [ (1, 2); (1, 1) ]
        ]

        let setColor color addr =
            let led = hub.GetRgbLed(addr)
            led.Color <- color

        let rec loop (wh: WaitHandle) iter =
            System.Diagnostics.Debug.WriteLine("thdloop");
            let iterNext = (iter + 1) % levels.Length
            levels.[iter] |> List.iter (setColor (0uy, 0uy, 0uy))
            levels.[iterNext] |> List.iter (setColor (255uy, 255uy, 255uy))
            if (wh.WaitOne 500) then () else loop wh iterNext

        loop (param :?> CancellationToken).WaitHandle 0
        System.Diagnostics.Debug.WriteLine("thd done")
        hub.Deactivate()
