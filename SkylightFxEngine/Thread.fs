// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

module SkylightFxEngine.Thread

open System
open System.Threading
open OpenRGB.NET

let run (param: obj) =
    let getTod () =
        let now = DateTime.Now
        System.Diagnostics.Debug.WriteLine("{0}", now.TimeOfDay)
        let nowAdj = if now.IsDaylightSavingTime() then now.AddHours(-1.0) else now
        nowAdj.TimeOfDay

    let sunTimeMonths = Map [
        (1, ((6, 52), (16, 52)))
        (2, ((6, 44), (17, 21)))
        (3, ((6, 16), (17, 46)))
        (4, ((5, 36), (18, 9)))
        (5, ((5, 1), (18, 31)))
        (6, ((4, 40), (18, 53)))
        (7, ((4, 43), (19, 2)))
        (8, ((5, 2), (18, 48)))
        (9, ((5, 23), (18, 13)))
        (10, ((5, 43), (17, 33)))
        (11, ((6, 7), (16, 57)))
        (12, ((6, 34), (16, 44)))
        ]

    let ((srh, srm), (ssh, ssm)) = sunTimeMonths.[DateTime.Now.Month]

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
                SunHigh = 255, 198, 20
                SunLow = 209, 68, 8 }
          SunTimes =
              { Sunrise = new TimeSpan(srh, srm, 0)
                Sunset = new TimeSpan(ssh, ssm, 0) }
          FramePeriodMsec = (15 * 1000)
          GetTimeOfDay = getTod }

    let devColors =
        let newColors n = Array.init n (fun _ -> new Models.Color())
        [ 5; 6 ]
        |> List.map newColors

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
