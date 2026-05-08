# LTspice Ideal Diode Support

SpiceSharp supports the LTspice-style idealized diode branch through additive diode model parameters. This branch is selected when at least one ideal-diode model parameter is explicitly given on a **[DiodeModel](xref:SpiceSharp.Components.DiodeModel)**.

The implementation is intended for LTspice and vendor decks that use linear-region diode parameters such as `Ron`, `Roff`, and `Vfwd`. It is not a replacement for the default Berkeley exponential diode model. Existing diode models keep the default behavior unless one of the ideal-diode parameters is set.

The public reference for the LTspice model describes three linear conduction regions with optional current limiting and quadratic smoothing: [LTspice: Simple Idealized Diode](https://www.analog.com/en/resources/technical-articles/2017/09/14/04/06/ltspice-simple-idealized-diode.html).

## Activation

The idealized branch is active when any of these model parameters is given:

| Parameter | Meaning |
| --- | --- |
| `Ron` | Forward-region resistance. |
| `Roff` | Off-region resistance. |
| `Vfwd` | Forward turn-on voltage. |
| `Vrev` | Reverse-breakdown voltage magnitude. |
| `Rrev` | Reverse-breakdown resistance. |
| `Ilimit` | Forward current limit. |
| `RevIlimit` | Reverse current limit. |
| `Epsilon` | Forward transition smoothing voltage. |
| `RevEpsilon` | Reverse transition smoothing voltage. |

For example:

```csharp
var model = new DiodeModel("Dideal")
    .SetParameter("Ron", 2.0)
    .SetParameter("Roff", 1e9)
    .SetParameter("Vfwd", 1.0);
```

With SpiceSharpParser LTspice compatibility enabled, the equivalent netlist model is passed through to these engine parameters:

```spice
.model Dideal D(Ron=2 Roff=1G Vfwd=1)
```

Parser default mode still rejects these parameters as LTspice-only. This is deliberate: the parameters change the diode current law and should not silently alter non-LTspice decks.

## Default Values

When the ideal branch is active, omitted parameters use these defaults:

| Parameter | Default |
| --- | --- |
| `Ron` | `1` ohm |
| `Roff` | `1 / Gmin` as conductance, using the active simulation biasing `Gmin` |
| `Vfwd` | `0` V |
| `Vrev` | Not enabled unless explicitly given |
| `Rrev` | Same value as `Ron` |
| `Ilimit` | Not enabled unless explicitly given |
| `RevIlimit` | Not enabled unless explicitly given |
| `Epsilon` | `0` V |
| `RevEpsilon` | `0` V |

`Ron`, `Roff`, `Rrev`, `Ilimit`, and `RevIlimit` must be greater than zero when specified. `Vrev`, `Epsilon`, and `RevEpsilon` must be non-negative.

## Current Law

The model is memoryless. At each bias point, SpiceSharp evaluates one of the ideal diode regions and stamps the resulting current and small-signal conductance.

The off region starts from:

```text
I = Goff * V
```

where `Goff = 1 / Roff` if `Roff` is specified, otherwise `Goff = Gmin`.

The forward region is:

```text
I = (V - Vfwd) / Ron
```

Reverse breakdown is only enabled when `Vrev` is given. The reverse region is:

```text
I = (V + Vrev) / Rrev
```

where omitted `Rrev` uses `Ron`.

Region boundaries are computed from the intersection of the adjacent linear regions, not by blindly switching at the named voltage. This keeps behavior consistent when `Roff`, `Ron`, or `Rrev` are unusual but valid.

## Smoothing

`Epsilon` and `RevEpsilon` add a quadratic join around the forward or reverse transition. The smoothing window is centered on the line intersection. Inside that window, current and conductance are continuous.

Outside the smoothing window, the model returns to the ordinary linear region. Forward smoothing is only evaluated near the forward transition, so it does not mask an already-selected reverse-breakdown region.

## Current Limiting

`Ilimit` and `RevIlimit` apply tanh-style limiting after the regional current and conductance are calculated:

```text
Ilimited = Limit * tanh(I / Limit)
```

The conductance is scaled by the derivative of the limiter:

```text
Glimited = G * (1 - tanh(I / Limit)^2)
```

Forward limiting applies only when current is positive. Reverse limiting applies only when current is negative.

## Analysis Behavior

The idealized branch is supported for operating point, DC, transient, and AC small-signal analyses.

| Analysis | Behavior |
| --- | --- |
| OP/DC | Stamps the memoryless current and conductance at the solved bias point. |
| TRAN | Uses the same memoryless branch; ideal-diode capacitance and charge are zero. |
| AC | Uses the small-signal conductance at the operating point. |
| NOISE | Reuses the existing diode noise export path; exact LTspice ideal-diode noise parity is not claimed. |

The ordinary diode instance parameters still apply where they are meaningful:

| Instance parameter | Effect |
| --- | --- |
| `m` | Parallel multiplier scales current and conductance. |
| `n` | Series multiplier scales the internal diode voltage and conductance contribution. |
| `area` | Scales ideal branch current and conductance. |
| `off` | Influences the initial operating-point guess. |
| `temp` | Preserved as an instance parameter, but the ideal branch itself has no exponential temperature law. |

The Berkeley junction capacitance and charge-storage equations are bypassed for ideal diodes, so ideal-diode capacitance and stored charge are zero.

## Implementation Notes

