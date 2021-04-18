// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

module SkylightFxEngine.Thread

open System
open System.Threading
open LedControl

let run (param: obj) =
    let hub = new Asus.AuraSyncHub()
    hub.Activate()

    let skyParam: Scenes.SkyScene.Param<int * int> =
        { Levels =
              [ (-20, [ (0, 2) ])
                (-17, [ (0, 1) ])
                (-14, [ (0, 0) ])
                (-5, [ (1, 4); (1, 5) ])
                (5, [ (1, 3); (1, 0) ])
                (15, [ (1, 2); (1, 1) ]) ]
          Swatches =
              { Day = 170, 226, 255
                Night = 0, 4, 20
                Sun = 255, 213, 109 }
          SunTimes =
              { Dawn = new TimeSpan(5, 0, 0)
                Sunrise = new TimeSpan(6, 0, 0)
                Sunset = new TimeSpan(19, 0, 0)
                Dusk = new TimeSpan(20, 0, 0) }
          FramePeriodMsec = 250
          GetTimeOfDay = Scenes.SkyScene.Demo.createTodCounter 15 }

    let handleFrame () = Scenes.SkyScene.Demo.handleFrame hub skyParam

    let wh = (param :?> CancellationToken).WaitHandle
    while not (wh.WaitOne (handleFrame())) do ()
    
    hub.Deactivate()
