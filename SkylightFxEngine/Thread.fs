// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

module SkylightFxEngine.Thread

open System
open System.Threading
open LedControl

let run (param: obj) =
    let hub = new Asus.AuraSyncHub()
    hub.Activate()

    let getTod () =
        let tod = DateTime.Now.TimeOfDay
        System.Diagnostics.Debug.WriteLine("{0}", tod)
        tod

    let skyParam: Scenes.SkyScene.Param<int * int> =
        { Levels =
              [ (0, [ (0, 2) ])
                (2, [ (0, 1) ])
                (4, [ (0, 0) ])
                (9, [ (1, 4); (1, 5) ])
                (12, [ (1, 3); (1, 0) ])
                (15, [ (1, 2); (1, 1) ]) ]
          Swatches =
              { Day = 170, 226, 255
                Night = 0, 0, 0
                Twilight = 2, 8, 20
                SunHigh = 255, 178, 0
                SunLow = 209, 55, 0 }
          SunTimes =
              { Sunrise = new TimeSpan(6, 0, 0)
                Sunset = new TimeSpan(19, 0, 0) }
          FramePeriodMsec = (15 * 1000)
          GetTimeOfDay = getTod }

    let handleFrame () = Scenes.SkyScene.handleFrame hub skyParam

    let wh = (param :?> CancellationToken).WaitHandle
    while not (wh.WaitOne (handleFrame())) do ()
    
    hub.Deactivate()
