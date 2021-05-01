// Copyright (c) 2021, Chris Hyndman
// SPDX-License-Identifier: BSD-3-Clause

module Common

open LedControl

type Color = int * int * int

let setLedColor (led: RgbLed) ((r, g, b): Color) =
    led.Color <- (byte r, byte g, byte b)
