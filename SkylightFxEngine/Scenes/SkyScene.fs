﻿// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

module SkylightFxEngine.Scenes.SkyScene

open Common
open LedControl
open System

type Level<'TAddress> = int * ('TAddress list)

type SunTimes =
    { Sunrise: TimeSpan
      Sunset: TimeSpan }

type Swatches =
    { Day: Color
      Twilight: Color
      Night: Color
      SunHigh: Color
      SunLow: Color }

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

let private interpColor interp (xa, ((ra, ga, ba): Color)) (xb, ((rb, gb, bb): Color)) x =
    let rf = interp (xa, float ra) (xb, float rb) x
    let gf = interp (xa, float ga) (xb, float gb) x
    let bf = interp (xa, float ba) (xb, float bb) x
    (int rf, int gf, int bf)

let private todToFloat (tod: TimeSpan) = tod.TotalSeconds

let private getSunPosition times tod =
    let sr = todToFloat times.Sunrise
    let ss = todToFloat times.Sunset
    let t = todToFloat tod
    let rad = (t - sr) * Math.PI / (ss - sr)
    let alt = sin rad
    let deg = (asin alt) * 180.0 / Math.PI
    (alt, deg)

let private getSkyBackground (day: Color) (twilight: Color) (night: Color) =
    let points =
        [ (-18.0, night)
          (0.0, twilight)
          (90.0, day) ]
    sortedMultiInterp (interpColor interpLinClamp) points

let private getSunScatterColor (high: Color) (low: Color) =
    let points =
        [ (0.0, low)
          (90.0, high) ]
    sortedMultiInterp (interpColor interpLinClamp) points

let private getSunScatterFactor =
    let points =
        [ (-9.0, 0.0)
          (0.0, 1.0)
          (90.0, 0.0) ]
    sortedMultiInterp interpLinClamp points

let private blend a b =
    let points =
        [ (0.0, a)
          (1.0, b) ]
    sortedMultiInterp (interpColor interpLinClamp) points

let private sampleColor times swatches tod angle =
    let _, sunAngle = getSunPosition times tod
    let sky = getSkyBackground swatches.Day swatches.Twilight swatches.Night sunAngle
    let scatter = getSunScatterColor swatches.SunHigh swatches.SunLow sunAngle
    let scatterBlend = blend sky scatter (getSunScatterFactor sunAngle)
    let gradient =
        [ (0.0, scatterBlend)
          (18.0, sky) ]
    sortedMultiInterp (interpColor interpLinClamp) gradient angle

module Demo =
    let createTodCounter mstep =
        let mutable m = 0
        fun () ->
            let tod = new TimeSpan(m / 60, m % 60, 0)
            m <- (m + mstep) % (24 * 60)
            tod

    let handleFrame<'a> (hub: Hub<'a>) (param: Param<'a>) =
        let tod = param.GetTimeOfDay()
        System.Diagnostics.Debug.WriteLine("{0}", tod)
        let sampleColorByAngle = sampleColor param.SunTimes param.Swatches tod
        let setColor ((r, g, b): Color) addr =
            let led = hub.GetRgbLed(addr)
            led.Color <- (byte r, byte g, byte b)

        let setColorByAngle = sampleColorByAngle >> setColor
        param.Levels
        |> List.iter (fun (angle, addrs) -> addrs |> List.iter (setColorByAngle (float angle)))
        hub.Flush();
        
        param.FramePeriodMsec
    