// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

module SkylightFxEngine.Thread

open System
open System.Threading
open OpenRGB.NET

let run (param: obj) =
    let getTod () =
        let tod = DateTime.Now.TimeOfDay
        System.Diagnostics.Debug.WriteLine("{0}", tod)
        tod

    let skyParam: Scenes.SkyScene.Param =
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

    let devColors =
        [
            Array.create 5 (Models.Color());
            Array.create 6 (Models.Color())
        ]

    let handleColorSet ((r, g, b): Common.Color, addrs: Common.LedAddress list) =
        let updateDevColor (devColor: Models.Color) =
            devColor.R <- (byte r)
            devColor.G <- (byte g)
            devColor.B <- (byte b)
        let updateAddr (zi, di) = updateDevColor devColors.[zi].[di]
        addrs
        |> List.iter updateAddr

    let handleFrame () =
        let (colorSets, period) = Scenes.SkyScene.handleSceneFrame skyParam
        colorSets
        |> List.iter handleColorSet

        try
            let client = new OpenRGBClient()
            devColors
            |> List.iteri (fun zi zoneColors ->
                client.UpdateZone(0, zi, zoneColors))
            client.Dispose()
        with
            _ -> ()
        period


    let wh = (param :?> CancellationToken).WaitHandle
    while not (wh.WaitOne (handleFrame())) do ()
