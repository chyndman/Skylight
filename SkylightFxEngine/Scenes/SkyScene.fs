// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

module SkylightFxEngine.Scenes.SkyScene

open Common
open LedControl
open System

type Level<'TAddress> = int * ('TAddress list)

type SunTimes =
    { Dawn: TimeSpan
      Sunrise: TimeSpan
      Sunset: TimeSpan
      Dusk: TimeSpan }

type Swatches =
    { Day: Color
      Night: Color
      Sun: Color }

type Param<'TAddress> =
    { Levels: Level<'TAddress> list
      Swatches: Swatches
      SunTimes: SunTimes
      FramePeriodMsec: int
      GetTimeOfDay: unit -> TimeSpan }

let private interpLin (xa, ya) (xb, yb) (x: float) =
    let dx = xb - xa
    let dy = yb - ya
    ya + (((x - xa) / dx) * dy)

let private interpLinClamp (xa, ya) (xb, yb) x =
    let xmin = min xa xb
    let xmax = max xa xb
    let xclamp = (max (min xmax x) xmin)
    interpLin (xa, ya) (xb, yb) xclamp

let private ident x = x

let private interpComposite (fnx: 'x -> float, fny: 'y -> float) interp (xa, ya) (xb, yb) x =
    (interp (fnx xa, fny ya) (fnx xb, fny yb) (fnx x)): float

let private interpColor interp (xa, ((ra, ga, ba): Color)) (xb, ((rb, gb, bb): Color)) x =
    let rf = interp (xa, float ra) (xb, float rb) x
    let gf = interp (xa, float ga) (xb, float gb) x
    let bf = interp (xa, float ba) (xb, float bb) x
    (int rf, int gf, int bf)

let private todToFloat (tod: TimeSpan) = tod.TotalSeconds

let private interpFloatByTod = interpComposite (todToFloat, ident)
let private interpColorByTod interp = interpColor (interpFloatByTod interp)

let rec private sortedMultiInterp interp xyList x =
    match xyList with
    | (xa, ya) :: xybList ->
        match xybList with
        | (xb, yb) :: tail ->
            if xb <= x then
                sortedMultiInterp interp xybList x
            else
                interp (xa, ya) (xb, yb) x
        | _ -> ya
    | _ -> invalidArg "xyList" "cannot be empty"

let private getSunAltitude times tod =
    let sunrise = times.Sunrise
    let sunset = times.Sunset
    let solarNoon = (times.Sunrise + times.Sunset) / 2.0
    let halfDay = new TimeSpan(12, 0, 0)
    let solarMidnightPre = solarNoon - halfDay
    let solarMidnightPost = solarNoon + halfDay
    let points =
        [ (solarMidnightPre, -90.0)
          (sunrise, 0.0)
          (solarNoon, 90.0)
          (sunset, 0.0)
          (solarMidnightPost, -90.0) ]
    sortedMultiInterp (interpFloatByTod interpLin) points tod

let private getSkyBackground (day: Color) (night: Color) times tod =
    let dawn = times.Dawn
    let sunrise = times.Sunrise
    let sunset = times.Sunset
    let dusk = times.Dusk
    let points =
        [ (dawn, night)
          (sunrise, day)
          (sunset, day)
          (dusk, night) ]
    sortedMultiInterp (interpColorByTod interpLinClamp) points tod

let private sampleColor times swatches tod (alt: int) =
    let bg = getSkyBackground swatches.Day swatches.Night times tod
    let fgSun = swatches.Sun
    let radius = 30.0
    let sunAlt = getSunAltitude times tod
    System.Diagnostics.Debug.WriteLine("tod {0}:{1} -> sunAlt {2}; alt {3}", tod.Hours, tod.Minutes, sunAlt, alt)
    let gradient =
        [ (sunAlt - radius, bg)
          (sunAlt, fgSun)
          (sunAlt + radius, bg) ]
    sortedMultiInterp (interpColor interpLinClamp) gradient (float alt)

module Demo =
    let createTodCounter mstep =
        let mutable m = 0
        fun () ->
            let tod = new TimeSpan(m / 60, m % 60, 0)
            m <- (m + mstep) % (24 * 60)
            tod

    let handleFrame<'a> (hub: Hub<'a>) (param: Param<'a>) =
        let sampleColorByAltitude = sampleColor param.SunTimes param.Swatches (param.GetTimeOfDay())
        let setColor ((r, g, b): Color) addr =
            let led = hub.GetRgbLed(addr)
            led.Color <- (byte r, byte g, byte b)

        let setColorByAltitude = sampleColorByAltitude >> setColor
        param.Levels
        |> List.iter (fun (alt, addrs) -> addrs |> List.iter (setColorByAltitude alt))
        hub.Flush();
        
        param.FramePeriodMsec
    