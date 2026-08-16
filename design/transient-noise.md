# Transient noise analysis — design plan

Status: Phases 0-2 implemented, Phases 3-5 still a proposal.

## 1. Goal

Add a time-domain noise capability: inject stochastic sources derived from each device's
physical noise density at its running operating point, integrate the circuit normally, and
obtain sample paths whose statistics reproduce the circuit's noise behaviour.

This is the Monte-Carlo "transient noise" analysis found in Spectre / ADS / LTspice, not a
noise *transfer* analysis.

### In scope

- A new simulation type deriving from `Transient`.
- A new behavior interface for devices to expose their power spectral densities in the time domain.
- Time-domain noise source primitives (thermal, shot, flicker) mirroring the existing AC ones.
- Per-device PSD definitions for every device in the framework that currently has an `INoiseBehavior`.
- A reproducible, seedable random stream, and a Monte-Carlo driver over `Rerun`.

### Out of scope

- Correlated noise sources (induced gate noise). A hook is left, see §9.
- Covariance/Lyapunov propagation, PSS/PNoise, SDE-specific integrators.
- Any change to the existing AC `Noise` analysis, which stays the tool of choice for
  precise noise floors.

### Non-goal, stated explicitly

This analysis is for jitter, eye diagrams, tail probabilities, noise-induced switching and
other things a linear analysis cannot express. It is **not** a cheaper or better way to get
a noise floor — see the estimator-variance row in §6.

### Reproducibility contract

Deliberately narrow: **the same seed, the same netlist and the same SpiceSharp version
reproduce the same result.** Nothing weaker is promised. In particular the result may change
if step control changes, and results are not required to be invariant under a different
tolerance setting. This narrowness is what makes §3 possible, so it is a contract, not a
caveat.

## 2. What already exists

| Piece | Location | Reused how |
|---|---|---|
| Variable-step integration, LTE control, rejection | `Trapezoidal` / `Gear` | unchanged |
| `IAcceptBehavior.Probe()/Accept()` | [IAcceptBehavior.cs](../SpiceSharp/Simulations/Implementations/Time/IAcceptBehavior.cs) | advance / commit the noise state |
| `StateValue<T>` over `IHistory<T>` | [StateValue.cs](../SpiceSharp/Simulations/Implementations/Time/IntegrationMethods/States/StateValue.cs) | rollback-safe noise state, free |
| `IIntegrationMethod.RegisterState` | [IIntegrationMethod.cs:57](../SpiceSharp/Simulations/Implementations/Time/IntegrationMethods/IIntegrationMethod.cs:57) | the method commits the state on `Accept()` |
| `ElementSet<double>` RHS stamping | `CurrentSource.Biasing` | inject the noise current |
| `Rerun` / `Repeat` / `CurrentRun` | [Simulation.cs](../SpiceSharp/Simulations/Simulation.cs) | Monte-Carlo loop reusing setup |
| AC `Noise` analysis | [Noise.cs](../SpiceSharp/Simulations/Implementations/Noise/Noise.cs) | ground truth for validation |
| `NoisePoint` on `INoiseSimulationState` | [NoisePoint.cs](../SpiceSharp/Simulations/Implementations/Noise/NoisePoint.cs) | **the template for §4** — see below |
| `NoiseSource` + `NoiseThermal`/`NoiseShot`/`NoiseGain` | [NoiseSource.cs](../SpiceSharp/Simulations/Implementations/Noise/NoiseSource.cs), [Components/Noise](../SpiceSharp/Components/Noise) | the shape the time-domain primitives copy |
| `OnePort<double>` | [OnePort.cs](../SpiceSharp/Components/Common/OnePort.cs) | the two terminals a source is connected between |

The AC noise path already solves the "don't recompute expensive things per source" problem, and
it is worth naming the mechanism explicitly because §4 reuses it wholesale.
[NoisePoint](../SpiceSharp/Simulations/Implementations/Noise/NoisePoint.cs) is a readonly struct
that, at construction, evaluates `Log(Frequency)` and `Log(InverseGainSquared)` — the only
transcendentals the integration in
[NoiseSource.Integrate](../SpiceSharp/Simulations/Implementations/Noise/NoiseSource.cs:54) needs
that depend on the sweep point rather than on the source. `Noise.Execute` builds **one**
`NoisePoint` per frequency ([Noise.cs:162](../SpiceSharp/Simulations/Implementations/Noise/Noise.cs:162))
and hands it to every source through `INoiseSimulationState.Point`. Sources contribute only
their own per-source math on top.

The transient case has the same split, with `Δt` in place of frequency: the shaping-filter
propagator and its noise Cholesky factor depend on `Δt` and `f_max` alone, both global, while
`σ_∞` depends on the source's operating point alone. So the same division of labour applies,
and it is worth strictly more here — the AC state saves two `Log` per point, the time-domain
state saves an `Exp`, three `Sqrt` and (with flicker) a whole pole ladder *per source* per
timepoint. §4.4 quantifies it.

Relevant properties of the existing code:

- `Transient.Probe()` calls `_method.Probe()` *before* the behaviors, so `Time` and `BaseTime`
  are both current and `Δt = Time − BaseTime` is available inside a behavior's `Probe()`.
- `Transient.Probe()` and `Transient.Accept()`
  ([Transient.cs:456](../SpiceSharp/Simulations/Implementations/Time/Transient.cs:456)) are
  `protected` but **not `virtual`**. `NoiseTransient` needs to update the shared point *before*
  the behaviors run, so both must become `protected virtual`. That one-word change is the only
  modification this design makes to existing code.
- `IHistory<T>.Accept()` rotates ([ArrayHistory.cs:70](../SpiceSharp/Simulations/States/Histories/ArrayHistory.cs:70)),
  so after an accepted step `Value` holds a stale rotated-out entry. Every `Probe()` must
  therefore write `Value` as a function of `GetPreviousValue(1)`, never read `Value` as the
  previous state. This is what makes rejection rollback free, and it is easy to get wrong.
- `Transient.Accept()` calls behaviors first, then `_method.Accept()`, which walks
  `RegisteredStates` and calls `Accept()` on each. A `StateValue<double>` written during
  `Probe()` therefore rolls back for free on rejection: the retry overwrites `Value` while the
  last accepted value stays at `GetPreviousValue(1)`.
- `RegisterState` only adds a state to `TruncatableStates` if it implements `ITruncatable`.
  `StateValue<T>` does not, so the noise state stays out of the LTE estimator. This is what we
  want — see §3.
- `TruncateNodes` defaults to `false` ([SpiceMethod.cs:80](../SpiceSharp/Simulations/Implementations/Time/IntegrationMethods/Spice/SpiceMethod.cs:80)),
  so raw node voltages are not in the LTE estimator. Tracked capacitor charges still are.

## 3. The central design decision

**Noise sources are band-limited, and band-limited noise has an exact transition density at
arbitrary Δt. There is no noise grid.**

The user specifies a maximum noise frequency `f_max`. A source is then white noise of physical
one-sided density `S` shaped by an `n`-pole lowpass at `f_max`, which is an Ornstein-Uhlenbeck
process. Its exact conditional distribution one step ahead is closed-form for *any* step, so
the integrator takes whatever steps it likes and the source is evaluated exactly on them.

### First order

Keep a **unit-variance** state `u` per source. With `λ = 2π f_max`:

```
u ← ρ·u + √(1 − ρ²)·Z,      ρ = exp(−λ·Δt),      Z ~ N(0,1)
```

and stamp

```
i = σ_∞ · u,        σ_∞² = k_n · S · f_max
```

Two properties carry the whole design:

- That update is **exact**, not a discretization of an SDE. Irregular, arbitrarily varying `Δt`
  costs nothing in accuracy.
- **`σ_∞` contains no `Δt`.** The injected power is a property of the source and `f_max` alone.

`k_n` is the equivalent-noise-bandwidth factor of an `n`-pole shape,
`k_n = (π/2)·(2n−3)!!/(2n−2)!!`:

| `n` | `k_n` | value |
|---|---|---|
| 1 | `π/2` | 1.5708 |
| 2 | `π/4` | 0.7854 |
| 3 | `3π/16` | 0.5890 |

Concretely, at first order:

| Source | One-sided PSD `S` | `σ_∞²` |
|---|---|---|
| Thermal | `4kTG` | `2π·kTG·f_max` |
| Shot | `2q·\|I\|` | `π·q·\|I\|·f_max` |
| Flicker | `KF·I^AF / f^β` | sum of OU sections, §5.3 |

### Why this replaces the fixed-grid design

The earlier revision of this document injected a zero-order-hold source on a fixed grid of
spacing `Δt_n`, with `σ² = S/(2Δt_n)`, and constrained the integrator to land on grid points.
That existed to avoid a specific failure: drawing a fresh sample per accepted step makes the
injected bandwidth `1/(2Δt_k)`, so the noise floor correlates with circuit activity, and — worse
— noise perturbs tracked charges, the divided-difference estimator in
[Trapezoidal.Instance.DerivativeInstance.cs:91](../SpiceSharp/Simulations/Implementations/Time/IntegrationMethods/Spice/Trapezoidal/Trapezoidal.Instance.DerivativeInstance.cs:91)
shrinks the step, and a smaller step means a *larger* amplitude. That positive feedback
converges on `MinStep` and throws.

Band-limiting removes the failure instead of working around it. Increment variance is
`1 − ρ² ≈ 2λ·Δt → 0`, so a smaller step yields a *smoother* source. **The feedback is now
negative**: shrink the step, see less jaggedness, relax the step. It self-stabilizes.

Everything the grid existed to support therefore goes away: no `ITruncatingBehavior`, no
landing-vs-capping problem, no ZOH discontinuity for trapezoidal to straddle, no `Δt_n`
selection rule, and no counter-based RNG (§4.6).

### Residual cost: LTE order, not stability

A one-pole OU path is Hölder-½ — continuous but nowhere differentiable. The third divided
difference scales as `Δt^−2.5`, so trapezoidal LTE falls only as `√Δt`. Convergent, so the
controller does not run away, but halving LTE costs 4× the steps. Each additional shaping pole
buys back a full order:

| Poles | Path regularity | Effective LTE order |
|---|---|---|
| 1 | C⁰ (Hölder ½) | 0.5 |
| 2 | C¹ | 1.5 |
| 3 | C² | 2.5 |

**Make the order configurable and default it to 2.** The exact-transition property survives;
see §4.7.

### Choosing f_max

For a circuit pole at `f_p`, cascading the shaping filter with the circuit captures a fraction
of the total noise power of

> **order 1:** `f_max / (f_max + f_p)`, deficit ≈ `f_p / f_max`
> **order 2:** `f_max·(2·f_max + f_p) / (2·(f_max + f_p)²)`, deficit ≈ `1.5·f_p / f_max`

| `f_max` | order 1 | order 2 |
|---|---|---|
| `10·f_p` | 90.9 % | 86.8 % |
| `20·f_p` | 95.2 % | 93.0 % |
| `100·f_p` | 99.0 % | 98.5 % |
| `300·f_p` | 99.7 % | 99.5 % |

These are exact, not first-order expansions. Halve the deficit if quoting σ rather than power.

`f_max` must be chosen from the fastest pole whose noise contribution matters, **not** from the
accuracy needed for the deterministic trajectory. Note also that this buys no speed over the
old grid design: the integrator must still resolve an `f_max`-bandwidth forcing function, so
step density is comparable to the former `Δt_n ≈ τ/33`. What it buys is the disappearance of
the grid machinery and its failure modes, plus a knob a user can reason about physically.

## 4. Architecture

The whole section is a transposition of the AC noise architecture from the frequency axis to
the time axis. The correspondence is one-to-one and deliberate:

| AC noise | Transient noise | What it holds |
|---|---|---|
| `INoiseSimulationState` | `ITimeNoiseSimulationState` | everything shared by all sources at the current point |
| `NoisePoint` (`Log f`, `Log 1/G²`) | `TimeNoisePoint` (propagator, Cholesky) | the point-dependent transcendentals, computed **once** |
| `INoiseSource` / `NoiseSource` | `ITimeNoiseSource` / `TimeNoiseSource` | per-source name, density, running state |
| `NoiseThermal(name, pos, neg)` | `TimeNoiseThermal(name, …, pos, neg)` | a source across two terminals |
| `INoiseBehavior : INoiseSource` | `ITimeNoiseBehavior : ITimeNoiseSource` | device aggregate, exports its sources by `[ParameterName]` |
| `Load()` / `Compute()` | `Inject()` / `Probe()` | stamp, then evaluate |

Three things do *not* transpose, and all three are simplifications:

- There is no adjoint solve and therefore no gain factor. AC's `NoiseThermal.Compute` fuses the
  PSD with `|ΔV|²` from the adjoint solution
  ([NoiseThermal.cs:36](../SpiceSharp/Components/Noise/NoiseThermal.cs:36)); the time-domain
  primitives produce a raw PSD and stamp a current. This fusion is exactly why the AC sources
  cannot simply be reused.
- Because there is no gain factor, **a bias-independent source is genuinely constant.** In AC,
  `Compute` must run at every frequency even for a linear resistor, because the gain moved. In
  transient, a linear resistor's `σ_∞` is fixed for the whole run and `Compute` runs once. §4.4.
- **`NoiseGain` has no counterpart at all.** It is the previous point taken to its limit: the class
  has no density law of its own, only the fusion, and every one of its five users in the framework
  is a flicker source handing over an `S(f)` it computed itself. Neither half survives — there is no
  frequency to evaluate `S` at, and no transfer function to fuse it with — so removing the fusion
  leaves an assignment. The `1/f` case it exists for becomes `TimeNoiseFlicker` and its pole ladder
  (§5.3), and a device wanting some other density subclasses `TimeNoiseCurrentSource` directly.

### 4.1 New behavior interface

```csharp
[SimulationBehavior]
public interface ITimeNoiseBehavior : ITimeNoiseSource, IBehavior
{
    /// Refresh noise densities from the last accepted operating point and advance
    /// the shaping state by the probed step. Called once per probed timepoint,
    /// never inside the Newton loop.
    void Probe();

    /// Stamp the frozen realization into the right-hand side.
    /// Called on every Load().
    void Inject();
}
```

with

```csharp
public interface ITimeNoiseSource
{
    /// The name of the noise source.
    string Name { get; }

    /// The one-sided power spectral density at the last accepted operating point, in A²/Hz.
    double NoiseDensity { get; }

    /// The frozen current realization for the probed timepoint, in A.
    double Current { get; }
}
```

`ITimeNoiseBehavior` inheriting `ITimeNoiseSource` mirrors `INoiseBehavior : INoiseSource`
exactly: a device sums its sources' `NoiseDensity` and `Current`, and exposes each individual
source as an `ITimeNoiseSource` property with `[ParameterName]`, the way
[Diodes/Noise.cs](../SpiceSharp/Components/Semiconductors/Diodes/Noise.cs) exposes `rs`, `id`
and `flicker`. Densities add, and currents into the same node pair add, so both aggregates are
physically meaningful and come for free as export quantities.

Freezing the realization at probe time, outside the Newton loop, matters for three reasons:
the Newton iteration needs a fixed target to converge onto; the PSD must not be modulated by
the noise it is generating within a single solve; and it pins the stochastic-calculus
convention (see §6, Itô/Stratonovich).

No `ITruncatingBehavior` involvement. Nothing constrains where the integrator steps.

### 4.2 Keep the state unit-variance

`σ_∞` depends on the operating point (shot noise goes as `√|I|`). Do **not** fold it into the
shaping state — hold `u` at unit variance and apply `σ_∞(t)` at stamping time:

```
u  : autonomous, unit-variance, driven only by the RNG
i  : σ_∞(t) · u(t)
```

Consequences, all of them wanted:

- No spurious transient when the operating point moves. Folding `σ_∞` into the state would make
  a step change in bias look like a settling artifact.
- `u` is completely decoupled from the circuit — a pure function of the seed and the step
  sequence. That makes it unit-testable without a circuit (§7.1).
- Correlated sources (§9) become straightforward: `u` becomes a vector and the per-device
  Cholesky factor applies at stamping time, not inside the state update.
- **It is what makes §4.4 possible.** With `σ_∞` out of the state update, the update
  coefficients contain nothing device-specific — only `λ` and `Δt`, both global. They can
  therefore be computed once per timepoint and shared. Folding `σ_∞` in would make every
  source's propagator different and the sharing would collapse.

### 4.3 The shared simulation state

The state is where every transcendental that does not depend on a specific source lives.

```csharp
public interface ITimeNoiseSimulationState : ISimulationState
{
    /// The noise bandwidth limit, in Hz.
    double MaximumNoiseFrequency { get; }

    /// The number of shaping poles (1..3). See §3.
    int BandLimitOrder { get; }

    /// Band-limit shaping coefficients for the currently probed step. Shared by every source.
    TimeNoisePoint Point { get; }

    /// Shaping coefficients for the flicker pole ladder, one per pole, for the currently
    /// probed step. Shared by every flicker source whatever its roll-off exponent. See §5.3.
    IReadOnlyList<TimeNoisePoint> FlickerLadder { get; }

    /// The per-section amplitude weights realizing a `1/f^β` roll-off on that ladder.
    /// Fixed for the run and cached per distinct `β`. See §5.3.
    FlickerWeights GetFlickerWeights(double beta);

    /// sqrt(k_n * f_max). Converts sqrt(PSD) to a stationary standard deviation.
    double AmplitudeScale { get; }

    /// Registers a source: allocates its shaping state with the integration method and
    /// seeds its RNG stream from hash(Seed, source.Name). See §4.6.
    void Register(TimeNoiseSource source);
}
```

`TimeNoisePoint` is the direct analogue of
[NoisePoint](../SpiceSharp/Simulations/Implementations/Noise/NoisePoint.cs) — a readonly struct
that does its expensive work in the constructor and is then read many times:

```csharp
public readonly struct TimeNoisePoint
{
    public double Decay { get; }      // e^{-z}
    public double Coupling { get; }   // z·e^{-z}, the propagator off-diagonal (0 at order 1)
    public double L11 { get; }        // Cholesky factor of Q(z)
    public double L21 { get; }
    public double L22 { get; }

    public TimeNoisePoint(double z, int order) { /* §4.7 */ }
}
```

Order 1 is order 2 with the second row dropped — `Q₁₁ = 1 − e^{−2z}` is the same expression in
both — so one struct covers every order and the `order` switch is confined to its constructor.

Unlike AC there is no `IHistory<TimeNoisePoint>`: nothing needs the previous point's
coefficients, because rollback lives in the per-source `StateValue` instead (§4.8). A plain
property is enough.

`FlickerLadder` is the payoff that most justifies a dedicated state. A flicker source is a sum
of `N` OU sections with distinct poles (§5.3); each section needs its own `Decay`/`L11` at the
probed `Δt`. But the pole *positions* are a property of the *simulation* — `f_min` from
`StopTime`, `f_max` from the parameters — not of the device. So the `N` exponentials are
computed once per timepoint for the whole circuit.

**The weights are the one part that is per-device, and they are deliberately split off.** The
roll-off exponent `β` of `S ∝ 1/f^β` is a model parameter (§5.3), so two devices in the same
circuit can want two different ladders' worth of weights. Pairing a weight with a
`TimeNoisePoint` in one struct — which an earlier revision did, as `TimeNoiseSection` — would
have forced the whole ladder to be rebuilt per `β` and destroyed the sharing. Separating them
costs nothing, because the two live on opposite sides of the expensive/cheap divide:

| | depends on | recomputed |
|---|---|---|
| `FlickerLadder` — `N` × `TimeNoisePoint` | pole positions, `Δt`, `order` | once per **timepoint**, for the whole circuit |
| `FlickerWeights` — `N` × `√w_i` | pole positions, `β` | once per **run**, per distinct `β` |

```csharp
public sealed class FlickerWeights
{
    /// The roll-off exponent these weights realize.
    public double Exponent { get; }

    /// sqrt(w_i), one per pole of FlickerLadder and in the same order. See §5.3.
    public IReadOnlyList<double> Amplitudes { get; }
}
```

`GetFlickerWeights` caches by `β`, so a circuit whose devices all use the SPICE default
`β = 1` allocates exactly one vector, and a mixed circuit pays one small array per distinct
value. `TimeNoiseFlicker` resolves its `FlickerWeights` once, in its constructor, and holds the
reference — it never looks the exponent up again. This is the same shape §9 proposes for a
per-source `f_max`, and it is worth noting that `β` gets the treatment now and `f_max` does not:
`β` is genuinely per-model and costs only a setup-time array, whereas a per-source `f_max` would
move a source off the shared per-timepoint point and onto its own `Exp`/`Sqrt` per step.

### 4.4 What this actually costs

Per **probed** timepoint, with `M` noise sources of which `V` sit at a moving operating point
and `M_f` are flicker sources, order-2 shaping, and an `N`-section flicker ladder. Write
`c(order)` for the cost of one `TimeNoisePoint`: 1 `Exp` + 1 `Sqrt` at order 1, 1 `Exp` +
3 `Sqrt` at order 2.

| | shared, on the state | per source | naive per-source design |
|---|---|---|---|
| Band-limit coefficients | `c(order)` | — | `M`·`c(order)` |
| Flicker ladder | `N`·`c(order)` | — | `M_f`·`N`·`c(order)` |
| Gaussian draws | — | 1 `Log`, 1 `Sqrt`, 1 `SinCos` | same |
| `σ_∞` refresh | — | `V`·(1 `Sqrt`) | `M`·(1 `Sqrt`) |
| Flicker `\|I\|^(AF/2)` | — | `M_f`·(1 `Log` + 1 `Exp`) | `M_f`·`N`·(1 `Log` + 1 `Exp`) |

Three things are doing the work:

1. **Coefficients are shared** — the first two rows go from `O(M)` to `O(1)`. The flicker row
   stays `O(1)` even when devices disagree about the roll-off exponent, because §4.3 puts `β`
   in the setup-time weights rather than in the per-timepoint coefficients. A second `β` in the
   circuit costs one array at setup and nothing per step.
2. **`σ_∞` is only refreshed when the bias moved.** `Compute` is called by the device from its
   own `Probe()`, so the device decides. A `Resistor` calls it once, from `InitializeStates`,
   and never pays a `Sqrt` again — the `V` in the table, not `M`. This is the direct benefit of
   there being no gain factor to re-fuse, and it is why `Compute` stays on the device side
   rather than being driven generically by the state.
3. **Order 2 consumes exactly one Box-Muller call per source per step.** `L·Z` needs `Z ∈ R²`,
   and trigonometric Box-Muller produces two normals from one `Log`, one `Sqrt` and one
   `SinCos`. No spare to carry, no cache, no branch — and the RNG consumption pattern is a
   fixed 2 uniforms per source per step, which keeps §4.6's reproducibility argument trivial.
   At order 1 the spare must be cached, which is a small argument for the order-2 default on
   top of the LTE one in §3.

Constant folding happens in each primitive's constructor, once, not per call.

### 4.5 Time-domain noise source primitives

Mirroring `NoiseSource` / `NoiseThermal` / `NoiseShot`, but not `NoiseGain` — see the head of §4:

```
TimeNoiseSource                  (abstract: shaping state, RNG stream, registration)
└── TimeNoiseCurrentSource       (abstract: one-port, element set, Inject)
    ├── TimeNoiseThermal         Compute(conductance, temperature)
    ├── TimeNoiseShot            Compute(current)
    └── TimeNoiseFlicker         ctor(…, β); Compute(coefficient, currentExponent, current)  // §5.3
```

`TimeNoiseFlicker` takes its roll-off exponent `β` in the **constructor**, not in `Compute`.
`β` selects the `FlickerWeights` vector (§4.3), which is fixed for the run, while `Compute` runs
per timepoint and carries only what the operating point moved. Note the two exponents are
different things and the naming has to keep them apart: `currentExponent` is SPICE's `af`, the
exponent on `|I|`, which every device in the framework already has
([Diodes/ModelParameters.cs](../SpiceSharp/Components/Semiconductors/Diodes/ModelParameters.cs)),
and `β` is the exponent on `f`, which none of them has yet — see §5.3.

`TimeNoiseCurrentSource` arrived with Phase 2; the paragraphs below describe the split between the
two abstract classes as it now stands. It is also the extension point: a device whose noise follows
neither law subclasses it and calls `SetNoiseDensity`, which is the whole of what a transposed
`NoiseGain` would have been.

The base class takes only a name and the noise state, calls `noise.Register(this)`, and provides:

```csharp
protected double Amplitude { get; set; }      // σ_∞, set by Compute
public double NoiseDensity { get; protected set; }
public double Current { get; private set; }

public virtual void Probe();    // u ← propagate(state.Point, u_prev) + L·Z; Current = Amplitude·u
public abstract void Inject();  // how the realization reaches the circuit
```

**`Inject()` is abstract on `TimeNoiseSource`, and the wiring lives below it.** That class
deliberately knows nothing about `OnePort<double>`, `ElementSet<double>` or
`IBiasingSimulationState`: it holds the shaping machinery and nothing else, so a source that is not
a current between two nodes needs no special case there. Every primitive listed above *is* one,
though — like `NoiseThermal`, each is connected between two terminals given to its constructor — so
the one-port and the element set live once, in `TimeNoiseCurrentSource`:

```csharp
public abstract class TimeNoiseCurrentSource : TimeNoiseSource
{
    private readonly ElementSet<double> _elements;

    protected TimeNoiseCurrentSource(string name, ITimeNoiseSimulationState noise,
        IBiasingSimulationState biasing, IVariable<double> pos, IVariable<double> neg)
        : base(name, noise)
    {
        var variables = new OnePort<double>(pos, neg);
        _elements = new ElementSet<double>(biasing.Solver, null, variables.GetRhsIndices(biasing.Map));
    }

    public override void Inject() => _elements.Add(-Current, Current);
}

public class TimeNoiseThermal : TimeNoiseCurrentSource
{
    /* constructor forwards */

    /// Thermal noise, S = 4·k·T·G.
    public void Compute(double conductance, double temperature)
        => SetNoiseDensity(4.0 * Constants.Boltzmann * temperature * conductance);
}
```

The element set is built from `biasing.Solver` and `variables.GetRhsIndices(biasing.Map)` exactly
as [CurrentSource.Biasing](../SpiceSharp/Components/Currentsources/ISRC/Biasing.cs:84) does.
`SetNoiseDensity` is on `TimeNoiseSource`, next to `Amplitude`: it stores `S` and applies
`Amplitude = AmplitudeScale · √S`, which is the one piece of arithmetic every primitive shares.

`Probe()` is the only virtual: `TimeNoiseFlicker` overrides it to walk `state.FlickerLadder`
instead of the single `state.Point`. Everything else — registration, seeding, rollback — is
shared.

Note the deliberate difference from the AC sources: these produce a **raw PSD**, with no
transfer-function factor, per the table at the head of §4.

Device behaviors then read like their AC counterparts. Compare
[Resistors/Noise.cs](../SpiceSharp/Components/RLC/Resistors/Noise.cs):

```csharp
[BehaviorFor(typeof(Resistor)), AddBehaviorIfNo(typeof(ITimeNoiseBehavior))]
[GeneratedParameters]
public partial class TimeNoise : Biasing, ITimeNoiseBehavior
{
    private readonly TimeNoiseThermal _thermal;

    public double NoiseDensity => _thermal.NoiseDensity;
    public double Current => _thermal.Current;

    [ParameterName("thermal"), ParameterInfo("The thermal noise source")]
    public ITimeNoiseSource Thermal => _thermal;

    public TimeNoise(IComponentBindingContext context) : base(context)
    {
        var biasing = context.GetState<IBiasingSimulationState>();
        var noise = context.GetState<ITimeNoiseSimulationState>();
        _thermal = new TimeNoiseThermal($"{Name}/r", noise, biasing,
            biasing.GetSharedVariable(context.Nodes[0]),
            biasing.GetSharedVariable(context.Nodes[1]));
    }

    void ITimeBehavior.InitializeStates()
        => _thermal.Compute(Conductance, Parameters.Temperature);   // once, for the whole run

    void ITimeNoiseBehavior.Probe() => _thermal.Probe();
    void ITimeNoiseBehavior.Inject() => _thermal.Inject();
}
```

The one deliberate deviation from AC: the source is given a **circuit-unique** name
(`$"{Name}/r"`) rather than the bare local name AC uses (`"r"`), because §4.6 seeds the RNG
stream from it. The `[ParameterName("thermal")]` export is unaffected and stays the user-facing
handle.

### 4.6 Random number generation

A **per-source sequential stream**, seeded `hash(seed, sourceId)` at setup. The narrow
reproducibility contract (§1) means the stream no longer has to be indexable by time — a
rejected step simply consumes draws that are then discarded, and the next run of the same
input rejects in the same place and discards the same draws.

Per-source rather than one global stream, for two reasons that cost nothing to honour up front:
`ParallelComponents` (Phase 4) would make a shared stream's consumption order nondeterministic,
and per-source streams keep results stable when an unrelated device is added to the netlist.

Do **not** use `System.Random`. Its algorithm has changed across .NET versions, and a runtime
upgrade is not a SpiceSharp version change, so it would breach the contract in §1. Use a small
in-repo `splitmix64` or `xoshiro256**`. For the Gaussian, prefer a non-rejection method
(trigonometric Box-Muller, or an inverse CDF) so the number of draws consumed per step is fixed
and the stream is easy to reason about.

Per-run seed for the Monte-Carlo driver: derive from `CurrentRun`, which `Simulation` already
exposes.

Seeding is centralized in `ITimeNoiseSimulationState.Register`, which hashes the master seed
with `source.Name`. Hashing the *name* rather than a registration index is what makes the
stream independent of netlist order, so adding an unrelated device does not shift anybody
else's realization.

### 4.7 Exact discretization, order 2

Two cascaded identical poles, `λ = 2π f_max`, state `x = (v, u)`:

```
dv = −λ·v·dt + √(2λ)·dW
du = λ·(v − u)·dt
```

`v` is unit-variance and `u` has stationary variance `1/2`, so the stamped quantity is
`σ_∞·√2·u`. `A = −λI + N` with `N` nilpotent, so the propagator is closed-form:

```
e^{AΔ} = e^{−λΔ} · [[1, 0], [λΔ, 1]]
```

and with `z = λΔ`, `m = e^{−2z}`, the conditional covariance
`Q(Δ) = ∫₀^Δ e^{As}·b·bᵀ·e^{Aᵀs} ds` is

```
Q₁₁ = 1 − m
Q₁₂ = 1/2 − m·(z + 1/2)
Q₂₂ = 1/2 − m·(z² + z + 1/2)
```

Update: `x ← e^{AΔ}·x + L·Z`, with `L` the 2×2 Cholesky factor of `Q` and `Z ~ N(0, I₂)`.
Both are precomputable functions of `z` alone.

**Numerical caveat.** For small `z` these are differences of nearly equal quantities. Use the
series below `z ≈ 1e-2`:

```
Q₁₁ = 2z − 2z² + (4/3)z³ − (2/3)z⁴
Q₁₂ =      z² − (4/3)z³ +      z⁴
Q₂₂ =           (2/3)z³ −      z⁴
```

The Cholesky residual is the dangerous one: `Q₂₂ − Q₁₂²/Q₁₁ → (1/6)z³`, a cancellation between
two `O(z³)` terms. Compute it from the series in that regime rather than from the closed form.

All of this lives in the `TimeNoisePoint` constructor and runs once per timepoint for the whole
circuit (§4.3), which is the main reason the closed-form/series branch is affordable at all:
the branch is taken once, not once per source.

### 4.8 New simulation and state lifecycle

```csharp
public partial class NoiseTransient : Transient,
    IBehavioral<ITimeNoiseBehavior>,
    IStateful<ITimeNoiseSimulationState>,
    IParameterized<NoiseTransientParameters>
```

`NoiseTransientParameters` carries `MaximumNoiseFrequency` (`f_max`), `BandLimitOrder`
(default 2), `Seed`, and flicker ladder configuration — the pole range and the sections per
decade, which set the ladder every flicker source shares. The roll-off exponent `β` is
deliberately *not* here: it is a model parameter of the device, so it arrives per source and
selects a cached weight vector instead (§4.3, §5.3).

Lifecycle, mirroring `Noise` ([Noise.cs:100](../SpiceSharp/Simulations/Implementations/Noise/Noise.cs:100)):

- **`CreateStates`** — after `base.CreateStates()`, so `_method` already exists. Constructs
  `TimeNoiseSimulationState`, which precomputes `AmplitudeScale = √(k_n·f_max)` and the flicker
  ladder's poles and weights (`f_min` from `TimeParameters.StopTime`, per §5.3), and takes the
  `IIntegrationMethod` so it can register shaping states on behalf of sources.
- **`CreateBehaviors`** — after `base.CreateBehaviors()`, grab
  `EntityBehaviors.GetBehaviorList<ITimeNoiseBehavior>()`. Source constructors have by now
  called `Register`, so every stream is seeded and every shaping state is registered.
- **`Execute`** — hook `AfterLoad` to call `Inject()` on every behavior.
- **`Probe`** — override (see §2; requires making the base method `virtual`):

  ```csharp
  protected override void Probe()
  {
      base.Probe();                                   // _method.Probe(), then IAcceptBehavior.Probe()
      _state.SetCurrentPoint(Time - _method.BaseTime); // one Exp + 3 Sqrt, for everybody
      foreach (var behavior in _noiseBehaviors)
          behavior.Probe();
  }
  ```

  The ordering is the whole point and it is the same ordering as
  [Noise.Execute](../SpiceSharp/Simulations/Implementations/Noise/Noise.cs:162): the state
  publishes the point first, then every source consumes it. `SetCurrentPoint(Δt)` is the
  analogue of `NoiseSimulationState.SetCurrentPoint(NoisePoint)`.

Each source registers its shaping state via `IIntegrationMethod.RegisterState`, which gives
rollback on rejection for free and — because `StateValue<T>` is not `ITruncatable` — keeps the
noise state out of the LTE estimator. Everything else is inherited.

**One trap with the shaping state.** Order-2 and flicker sources hold more than one scalar.
Do *not* use a `StateValue<double[]>`: `IHistory<T>.Accept()` rotates the array *references*, so
writing `Value[i]` during `Probe()` would mutate the last accepted array and destroy the
rollback. Either register one `StateValue<double>` per scalar component, or — preferred —
add a small `IIntegrationState` holding two `double[]` and swapping them on `Accept()`, so an
`N`-section flicker source costs one registration instead of `N`.

### 4.9 File layout

Placed alongside the AC equivalents so the correspondence is visible in the tree:

```
SpiceSharp/Simulations/Implementations/NoiseTransient/
  ITimeNoiseBehavior.cs                     ← Noise/INoiseBehavior.cs
  ITimeNoiseSource.cs                       ← Noise/INoiseSource.cs
  ITimeNoiseSimulationState.cs              ← Noise/INoiseSimulationState.cs
  TimeNoisePoint.cs                         ← Noise/NoisePoint.cs
  FlickerWeights.cs                         (the per-β weight vector of §4.3/§5.3)
  TimeNoiseSource.cs                        ← Noise/NoiseSource.cs
  NoiseTransient.cs                         ← Noise/Noise.cs
  NoiseTransient.TimeNoiseSimulationState.cs ← Noise/Noise.NoiseSimulationState.cs
  NoiseTransientParameters.cs               ← Noise/NoiseParameters.cs
  Rng/                                      (splitmix64 / xoshiro256**, Box-Muller)

SpiceSharp/Components/Noise/
  TimeNoiseCurrentSource.cs
  TimeNoiseThermal.cs  TimeNoiseShot.cs  TimeNoiseFlicker.cs
  (namespace SpiceSharp.Components.NoiseSources, next to NoiseThermal.cs etc.)

SpiceSharp/Components/**/TimeNoise.cs       ← **/Noise.cs, one per device
```

## 5. Implementation phases

### Phase 0 — `TimeNoisePoint`, RNG, and the validation harness — **done**

No device changes, no circuit. `TimeNoisePoint` is a pure function of `(z, order)`, so it can
be tested standalone: check the propagator and Cholesky factor against the closed forms, and
exercise the series crossover of §4.7 from both sides. Then unit-test the OU update driven by a
sequence of `TimeNoisePoint`s against its analytic stationary variance and autocorrelation over
a deliberately irregular step sequence (§7.1) — this is the sharpest and cheapest test in the
plan and it needs none of the rest of the feature. Then a hand-placed noise current source on an
RC for the `kT/C` test (§7.2).

Delivered:

- [TimeNoisePoint.cs](../SpiceSharp/Simulations/Implementations/NoiseTransient/TimeNoisePoint.cs) —
  orders 1 and 2, closed form from `z = 1e-2` upward and the series below it. Two additions to the
  shape sketched in §4.3: an `Order` property, because the output scaling differs per order, and a
  `Propagate` method, which is the `propagate(state.Point, u_prev)` of §4.5. Putting it on the
  struct is what lets the Phase-0 tests exercise the same code `TimeNoiseSource.Probe` will.
  `Stationary(order)` is the `z = ∞` point, for the stationary initialization of §5.3.
- [Rng/](../SpiceSharp/Simulations/Implementations/NoiseTransient/Rng) — `SplitMix64`,
  `Xoshiro256StarStar` and `NoiseRandomStream` (Box-Muller, plus the seed-and-name hash of §4.6).
  `NextNormals` yields the pair and there is deliberately no cached spare, so an order-1 caller
  discards the second variate and consumption stays a fixed two uniforms per source per step.
- Tests: [TimeNoisePointTests.cs](../SpiceSharpTest/Simulations/TimeNoisePointTests.cs),
  [NoiseRandomStreamTests.cs](../SpiceSharpTest/Simulations/NoiseRandomStreamTests.cs),
  [TimeNoiseShapingTests.cs](../SpiceSharpTest/Simulations/TimeNoiseShapingTests.cs).

Two deterministic tests turned out sharper than the Monte-Carlo one §7.1 asks for, and both are
worth keeping as the series regresses. Propagating the covariance over an irregular step sequence
must leave the stationary `[[1, ½], [½, ½]]` a fixed point. And the covariance accumulated from
zero over `K` small steps must equal the single-step `Q(z)` — that is the one that pins the `z³`
cancellation of §4.7, because the second state's whole variance is built out of the residual while
the small steps sit in the series branch and the reference sits in the closed-form one.

The `kT/C` test is the loose one, at ±15 % on a Monte-Carlo estimate over ~2000 correlation times.
It is wide enough to reject an unbanded `kT/C`, a two-sided density or a missing `k_n`, and not much
more; §7.2 becomes a tight test only once Phase 5 can average over runs.

### Phase 1 — Vertical slice — **done**

`ITimeNoiseSource` / `ITimeNoiseBehavior`, `ITimeNoiseSimulationState` + its implementation,
`TimeNoiseSource` + `TimeNoiseThermal`, `NoiseTransient`, `NoiseTransientParameters`, the
`protected virtual` change on `Transient.Probe`/`Accept` (§2), and the `Resistor` time-noise
behavior. End-to-end on one device.

Delivered, in the layout of §4.9:

- [ITimeNoiseSource.cs](../SpiceSharp/Simulations/Implementations/NoiseTransient/ITimeNoiseSource.cs),
  [ITimeNoiseBehavior.cs](../SpiceSharp/Simulations/Implementations/NoiseTransient/ITimeNoiseBehavior.cs),
  [ITimeNoiseSimulationState.cs](../SpiceSharp/Simulations/Implementations/NoiseTransient/ITimeNoiseSimulationState.cs) —
  as sketched in §4.1 and §4.3, minus `FlickerLadder`, which arrives with Phase 3 rather than
  standing empty until then.
- [TimeNoiseSource.cs](../SpiceSharp/Simulations/Implementations/NoiseTransient/TimeNoiseSource.cs),
  [TimeNoiseThermal.cs](../SpiceSharp/Components/Noise/TimeNoiseThermal.cs),
  [Resistors/TimeNoise.cs](../SpiceSharp/Components/RLC/Resistors/TimeNoise.cs).
- [NoiseTransient.cs](../SpiceSharp/Simulations/Implementations/NoiseTransient/NoiseTransient.cs),
  [NoiseTransient.TimeNoiseSimulationState.cs](../SpiceSharp/Simulations/Implementations/NoiseTransient/NoiseTransient.TimeNoiseSimulationState.cs),
  [NoiseTransientParameters.cs](../SpiceSharp/Simulations/Implementations/NoiseTransient/NoiseTransientParameters.cs).
- Tests: [NoiseTransientTests.cs](../SpiceSharpTest/Simulations/NoiseTransientTests.cs).

Five deviations from what §4 originally sketched, all of them narrowing rather than widening:

- **Stamping moved out of the base class**, per §4.5 as it now reads: `TimeNoiseSource` holds the
  shaping state, the stream and the registration, and `Inject()` is abstract. `TimeNoiseThermal`
  owns the `OnePort<double>` and the `ElementSet<double>`. The base class therefore never assumes
  that a noise source is a current between two nodes.
- **Both orders ship now.** §4.7's propagator was already delivered in Phase 0, and
  `TimeNoisePoint.Propagate` covers both orders in one call signature, so restricting Phase 1 to
  order 1 would have cost code rather than saved it. The default is 2, per §4.8. Phase 2 is
  therefore device coverage only.
- **The source *is* its own integration state.** §4.8 proposed a separate small `IIntegrationState`
  holding two arrays. Making `TimeNoiseSource` implement `IIntegrationState` itself is the same
  thing with one fewer type: one registration per source, the arrays never leave the object so
  nothing can alias the last accepted state, and `Accept()` swaps the two references.
- **`MaximumNoiseFrequency` has no default and refuses to be skipped.** A band limit of zero makes
  every source silent rather than white, so `CreateStates` throws instead of quietly running a
  noiseless transient analysis. This adds one resource string, which is the only modification to
  existing code beyond the two `virtual` keywords of §2.
- **`Accept` is virtual but not overridden.** Registering the shaping state with the integration
  method already gives the accept/rollback behaviour, so `NoiseTransient` only overrides `Probe`.
  `Accept` is made virtual anyway, because a source whose density depends on the accepted solution
  is the natural place for a subclass to hook in.

Validation coverage: §7.2 (`kT/C`, at both orders, now with a real device rather than a hand-placed
source, at ±12 %), §7.8 (four parallel resistors of 4R must give the variance of one R — a shared
stream or a shared shaping state would make them perfectly correlated and inflate it fourfold; plus
the bit-identical realization after inserting an unrelated device), and same-seed/different-seed
reproducibility.

**§7.6 could not be written as specified at the time.** It asks for reproducibility across `Rerun`,
which was broken for every transient analysis, so reproducibility was asserted across two freshly
constructed simulations instead. That has since been fixed — see the note below — and §7.6 is now
covered by `When_Rerun_Expect_SameRealization` in
[NoiseTransientTests.cs](../SpiceSharpTest/Simulations/NoiseTransientTests.cs).

### `Rerun` for transient analyses — **done**

Phase 5 loops `Rerun` with `seed = f(CurrentRun)`, so it needed this first. Four pieces of state
survived a run and were never rewound:

- **The integration method.** `Transient` only called `IIntegrationMethod.Initialize()` from
  `CreateBehaviors`, so a reran transient saw `Time` already at `StopTime` and terminated after a
  single point. It is now called from `Execute`, where the rest of the per-run setup lives — the
  same place `DC` creates its sweep enumerators.
- **Waveform values.** The operating point of a transient loads `IWaveform.Value`, which is only
  recomputed on `Probe()`. A rerun therefore biased the circuit with the source values of the
  *previous* stop time and started from a wrong DC point that then decayed over the first time
  constants. `Execute` now probes the accept behaviors once at t = 0, before the operating point.
- **Delayed signals.** `VoltageDelay` and `LosslessTransmissionLine` keep a history of timepoints;
  on a rerun that history lies in the future of what is about to be probed, and
  `DelayedSignal.Probe` threw `Time points are not monotonically increasing`. Both now drop it on
  the t = 0 probe, along with their breakpoint slope tracking.
- **The `Sampler` point enumerator**, which the first run leaves exhausted. Restarted from
  `InitializeStates`.

One thing deliberately not reset is the biasing solution, so a rerun warm-starts its operating
point from the previous one. That matches what `DC` and `AC` already do, and the reruns still
reproduce the reference run's timepoints and values exactly.

The master seed of a `NoiseTransient` is also no longer cached when the state is created — it is
read in `TimeNoiseSimulationState.Initialize()`, so a driver can pick a new seed in between two
`Rerun` calls.

### Phase 2 — Device coverage — **done**

Diode, BJT, MOSFET levels 1/2/3, the set that currently has an `INoiseBehavior`. Thermal and shot
only at this stage; flicker stubbed. `TimeNoiseShot` joins `TimeNoiseThermal` here.
`TimeNoiseGain` does not — it turned out to have no time-domain counterpart at all, see the head
of §4 and the note below.

Delivered:

- [TimeNoiseShot.cs](../SpiceSharp/Components/Noise/TimeNoiseShot.cs) and
  [TimeNoiseCurrentSource.cs](../SpiceSharp/Components/Noise/TimeNoiseCurrentSource.cs).
- [Diodes/TimeNoise.cs](../SpiceSharp/Components/Semiconductors/Diodes/TimeNoise.cs),
  [Bipolars/TimeNoise.cs](../SpiceSharp/Components/Semiconductors/Bipolars/TimeNoise.cs),
  [Mosfets/Level1/TimeNoise.cs](../SpiceSharp/Components/Semiconductors/Mosfets/Level1/TimeNoise.cs)
  and its Level 2 and Level 3 counterparts — one per device, next to the `Noise.cs` they mirror.
- Tests: [NoiseTransientDeviceTests.cs](../SpiceSharpTest/Simulations/NoiseTransientDeviceTests.cs),
  [NoiseInjector.cs](../SpiceSharpTest/Simulations/NoiseInjector.cs).

Six things worth recording:

- **A `TimeNoiseCurrentSource` sits between `TimeNoiseSource` and the primitives.** §4.5 makes
  the point that the base class must not assume a noise source is a current between two nodes, and
  that stands — but every primitive of §4.5 *is*, so the one-port and the `ElementSet<double>`
  moved into one abstract class in between rather than being written out once per primitive.
  `Inject()` is implemented there and the primitives are left with nothing but their density law,
  one line each. `TimeNoiseThermal` was retrofitted onto it. The other half of the same tidy-up is
  `TimeNoiseSource.SetNoiseDensity`, which does the `Amplitude = AmplitudeScale · √S` that every
  primitive would otherwise repeat.
- **`TimeNoiseGain` was written, then deleted.** It shipped first as the transposition §4.9 asked
  for, and reviewing it is what produced the third bullet at the head of §4: with the fusion gone
  its `Compute` was `SetNoiseDensity(density)` and nothing else, and the `1/f` case that is the only
  thing `NoiseGain` is ever used for goes to `TimeNoiseFlicker` instead. That left a public class
  with no user in the framework, duplicating a three-line subclass of an extension point that
  `TimeNoiseCurrentSource` already exposes and that does not presume the density is flat. Deleted
  rather than renamed: the abstract class is the better extension point, so the concrete one has
  nothing left to be.
- **A device's `TimeNoise` derives from the deepest behavior it needs**, the way `Noise : Frequency`
  does in AC: from `Time` for the diode and the bipolar, and from the level's `Biasing` for the
  mosfets, whose transient behavior is a separate class that reaches the biasing behavior through
  `IMosfetBiasingBehavior`. The dependency graph creates the derived-most behavior first, so in a
  `NoiseTransient` the `TimeNoise` object *is* the device's time or biasing behavior, and in every
  other analysis it is not created at all.
- **The mosfet's `MosfetVariables<double>` had to become `protected`** on the three level `Biasing`
  behaviors. The noise sources sit across `d`–`dp`, `s`–`sp` and `dp`–`sp`, and the internal nodes
  are private variables that cannot be looked up again — constructing a second `MosfetVariables`
  would silently create a *second* internal drain and source. This mirrors `Diodes.Biasing.Variables`
  and `Mosfets.Frequency.Variables`, both of which are already protected, and it is the third and
  last modification to existing code, after the two `virtual` keywords of §2 and the resource string
  of Phase 1.
- **§4.4's `V`-versus-`M` saving is not taken for the parasitic resistors.** A diode's `rs`, a
  bipolar's `rc`/`re` and a mosfet's `rd`/`rs` are bias-independent and could be computed once, as
  the resistor's is. They are not: every one of these devices also carries a source that *is*
  bias-dependent, so it pays a `Probe()` regardless, and the only per-run hook available —
  `ITimeBehavior.InitializeStates` — is an explicit interface implementation on the base behavior
  that a derived class cannot extend without hiding it. Refreshing all of a device's densities
  together costs one extra `Sqrt` per parasitic per timepoint and keeps the device's `Probe()`
  readable. The saving is still real for a device that has *no* bias-dependent source, which is
  exactly the resistor case §4.4 describes.
- **The diode's shot noise uses the conduction current, not `LocalCurrent`.** `Diodes.Time.Load`
  folds the capacitor current into `LocalCurrent`, and a displacement current carries no shot noise,
  so the behavior subtracts `CapCurrent` again. The bipolar needs no such correction — its
  `CollectorCurrent` and `BaseCurrent` are set by `Biasing.Load` and left alone by `Time.Load`.

Validation coverage. Each device is biased by an ideal, and therefore noiseless, current source and
loaded by a capacitor, so its node has a single pole and the captured fraction of §3 applies
unchanged:

- A current-biased diode charges its load to `q·Vte/(2C)` — half of the `kT/C` that a resistor of
  the same conductance would give, because `2q·I = 2kT·gd` rather than `4kT·gd`. Both orders.
- A diode-connected bipolar injects `2q(Ic + Ib)` into a node of conductance `gpi + gm + go`, which
  is `(Ic + Ib)/Vt` again, so it also lands on `kT/(2C)` — independently of the bias.
- A diode-connected mosfet lands on `(2/3)·kT/C`, which is the factor 2/3 of the channel noise and
  nothing else. All three levels, all three agreeing to a tenth of a percent, as they must: the
  three circuits are the same problem in normalized time and share a seed.
- Every source of every device is checked against the closed form evaluated at the device's own
  exported operating point, which is what pins the mapping from operating point to noise law.
- A test-local component ([NoiseInjector.cs](../SpiceSharpTest/Simulations/NoiseInjector.cs)) built
  only out of the public API injects a user-specified density on top of a resistor's thermal noise,
  and the two powers add to `3·kT/C`. It subclasses `TimeNoiseCurrentSource` for its density and
  implements `ITimeNoiseBehavior` for the rest, so it is the check that a device outside the
  framework can take part in the analysis, and the only coverage the extension point has.

§7.7 asked for the diode to be checked against the AC `Noise` analysis. It is checked against the
closed form instead, which is sharper: AC's `NoiseThermal.Compute` has already fused the density
with `|ΔV|²` from the adjoint solve, so recovering a bare density from it to compare against would
mean dividing that factor back out.

**One trap on the measuring side, and it is not confined to the tests.** These devices sit at a bias
of volts while the noise on them is microvolts, so `E[v²] − E[v]²` cancels away every significant
digit of the answer — the first version of these tests was wrong by up to 20 %, erratically, and
looked like estimator scatter. Any statistic has to be accumulated relative to a reference near the
mean. Phase 5 computes ensemble statistics for the user and will hit exactly this.

### Phase 3 — Flicker

`FlickerWeights`, the shared `FlickerLadder` on the state, `TimeNoiseFlicker`, and the flicker
source on each of the five devices that has one in AC. See §5.3 for the weight law and §7.9 for
what to test.

The exponent `β` ships with the ladder even though every present device passes the default 1
(§9). That is deliberate rather than speculative generality: the weight law, the guard-decade
budget and the acceptance criterion of §7.9 are all `β`-dependent, so deriving them once for
`β = 1` and again later is strictly more work than deriving them once in general — and the
runtime cost of carrying `β` is zero by §4.4.

### Phase 4 — Composition

Time-domain counterparts of
[Subcircuits/Behaviors/Noise.cs](../SpiceSharp/Components/Subcircuits/Behaviors/Noise.cs) and
`ParallelComponents/Behaviors/Noise.cs`. Simpler than the AC versions, since there is no
adjoint solve to mirror through a local solver — sources just stamp into the parent RHS. The
per-source RNG streams of §4.6 are what make the parallel case safe.

The shared state needs one thought here: `TimeNoisePoint` is a readonly struct published once
per timepoint before any behavior runs, so concurrent readers are safe by construction. The
mutable per-source shaping state is not shared, so it is safe too. Nothing in §4.3 needs a lock.

### Phase 5 — Monte-Carlo driver and statistics

A thin driver looping `Rerun` with `seed = f(CurrentRun)`, plus ensemble exports (mean,
variance, percentile envelopes) and a Welch PSD estimator for validation. Keep it optional —
users doing jitter extraction will want the raw sample paths.

**Statistics must be computed on a uniform resample grid, not on raw accepted timepoints.**
See §6, timepoint-selection bias.

**And they must be accumulated relative to a reference near the mean.** A node at a bias of volts
carrying microvolts of noise loses the whole answer to cancellation in `E[v²] − E[v]²`; see the end
of Phase 2, where this went wrong first.

### 5.3 Flicker noise

The target density is

```
S(f) = KF · |I|^AF / f^β
```

with **two independent exponents**, and the document is careful to keep them apart because
SPICE's naming does not: `AF` is the exponent on the bias current (SPICE's `af`,
`FlickerNoiseExponent`) and `β` is the roll-off exponent on frequency (SPICE's `ef` where it
exists at all).

`1/f^β` has unbounded power at DC and long-range correlation, so it cannot be produced by
scaling white samples. The generator is a **sum of OU sections with logarithmically spaced
poles** — which is to say, the same primitive as the band limit in §3, instantiated `N` times
with different `τ` and weights. It inherits arbitrary-`Δt` exactness for free, and `β` enters
only through the weights.

#### Why `β` is a parameter rather than 1

Nothing in the framework needs it today. All five AC flicker sources divide by the frequency
literally — [Diodes/Noise.cs:94](../SpiceSharp/Components/Semiconductors/Diodes/Noise.cs:94),
[Bipolars/Noise.cs:136](../SpiceSharp/Components/Semiconductors/Bipolars/Noise.cs:136) and the
three mosfet levels — and there is no `ef` model parameter anywhere in the repo. But BSIM-class
models carry one, the next mosfet levels will want it, and retrofitting it later is not a
one-line change: the weight law, the sections-per-decade rule, the guard-decade budget and the
validation criterion all move together. Going in now, it is one scalar and one cached array
(§4.3), so it goes in now. **`β` defaults to 1**, which is what every present device asks for.

#### Poles and weights

Poles are log-spaced from `f_min = 1/StopTime` to `f_max`: `λ_i = λ_min·rⁱ`, `i = 0 … N−1`.
Section `i` is an OU with rate `λ_i` and stationary variance `σ_i²`, so its one-sided PSD is
`4σ_i²λ_i/(λ_i² + ω²)`. Put

```
σ_i² = A · w_i,      w_i = (ln r) · sin(πβ/2) · (2π)^(β−1) · λ_i^(1−β),      A = KF·|I|^AF
```

Replacing the sum by `(1/ln r)∫dλ/λ` and using `∫₀^∞ x^(1−β)/(1+x²) dx = π/(2·sin(πβ/2))`,

```
Σ_i 4σ_i²λ_i/(λ_i² + ω²)  ≈  A / f^β
```

which is the target. Three things follow directly:

- **`β = 1` is the equal-weight ladder.** `sin(π/2) = 1` and `λ_i^0 = 1`, so `w_i = ln r`,
  independent of `i` — the ladder as it was originally specified. The generalization is strictly
  additive; it does not change the default behaviour by a bit.
- **`β` factors out of the runtime.** `w_i` depends on the pole positions and `β` alone, both
  fixed for the run, so it is the `FlickerWeights` array of §4.3 and never touches the per-step
  arithmetic. Per timepoint a flicker source still walks `FlickerLadder`, multiplies each section
  by its `√w_i`, and scales the sum by one amplitude.
- **`0 < β < 2`, strictly.** The integral above is the Mellin transform of the Lorentzian kernel
  and diverges outside that interval. `β = 2` is a genuine integrator, not a ladder; `β = 0` is
  white noise and belongs to §3. Both endpoints degrade gradually rather than failing sharply —
  see the band-edge note below — so range-check the parameter rather than trusting the arithmetic
  to blow up.

The shared amplitude is `√A = √KF · |I|^(AF/2)`, applied once to the summed ladder rather than
per section — one `Log` and one `Exp` per source per timepoint via
`|I|^(AF/2) = exp(0.5·AF·log|I|)`. Special-case the common exponents: `AF = 1` is `√|I|`,
`AF = 2` is `|I|`, both free of transcendentals.

**`TimeNoiseFlicker` must not use `SetNoiseDensity`.** That helper applies
`Amplitude = AmplitudeScale·√S` (§4.5), and `AmplitudeScale = √(k_n·f_max)` is the
equivalent-noise-bandwidth factor for a *white* source pushed through the band-limit shaper. A
flicker source's variance is already fully determined by `A` and the weights, so applying it
again would scale every flicker source by `√(k_n·f_max)`. The override of `Probe()` that walks
the ladder is also the override that sets `Amplitude` directly.

#### What changes when `β ≠ 1`, and what does not

The in-band PSD is `A/f^β` to within the ripple, at any admissible `β`. What moves is everything
about the band *edges*, and it moves asymmetrically, because the log-domain kernel
`λ^(2−β)/(λ² + ω²)` decays as `λ^(2−β)` below the corner and `λ^(−β)` above it — symmetric only
at `β = 1`.

- **The power piles up at the opposite end.** For `β < 1` the band integral is dominated by
  `f_max`, for `β > 1` by `f_min`. So the guard decades go on whichever side is shallow: above
  `f_max` for `β < 1`, below `f_min` for `β > 1`. Extending below `f_min` is cheap in flops and
  is the only lever available, because `f_min = 1/StopTime` is fixed by the run.
- **`β > 1` makes the finite-window limitation a first-order effect.** The note below has always
  said a run of length `T` contains no `1/f` power below `1/T`. At `β = 1` that is a footnote —
  the missing power grows as `ln T`. At `β > 1` it grows as `T^(β−1)`, so the observed variance
  becomes a visible function of run length rather than a rounding error. This is physics of the
  window, not a defect, but it has to be stated in the user-facing docs and it earns a row in §6.
- **Total variance stops being a valid check.** `Σσ_i² = A·Σw_i` works out to `sin(πβ/2)` times
  the naive band integral `∫_{f_min}^{f_max} A·f^(−β) df`. At `β = 1` the factor is 1 and the two
  agree exactly — the log-domain kernel is symmetric, so the band-edge deficit and the
  out-of-band leakage cancel. At `β = 0.5` or `1.5` it is 0.707, a 29 % discrepancy that is
  *not* an error: the ladder has soft band edges and the integral has hard ones, and for `β ≠ 1`
  most of the power sits in the decade nearest an edge. **Test the in-band PSD, not the total
  variance** (§7.9). Getting this backwards would look like a broken weight law.
- **Re-fit the sections-per-decade rule.** The existing "one decade per 1.5 sections for a few
  percent" was measured at `β = 1`, where the kernel's kink is symmetric. Expect the ripple to
  grow as `β` leaves 1, roughly with the shallow side's reciprocal decay rate, so treat 1.5 as a
  `β = 1` number and fit the rest.

**Decide the ladder's section order when Phase 3 starts.** A ladder of first-order sections has
its fastest pole at `f_max`, so the flicker contribution is Hölder-½ and drags the whole
solution back to the order-1 LTE behaviour of §3 even when `BandLimitOrder = 2`. Two ways out:
give each section the configured order — which is free in the code, since `FlickerLadder`
already holds full `TimeNoisePoint`s, but changes the weights that fit `1/f^β` — or pass the
summed first-order ladder through the band-limiting shaper. Both are cheap; the second keeps
the fit unchanged and is the likely answer, but it needs its transfer function worked out
before the weights are fitted, not after. Note that the choice interacts with `β`: the first
option's correction to `w_i` is `β`-dependent, so it would have to be re-derived per exponent
rather than tabulated once.

The FFT-filtering alternative considered in an earlier revision is dropped. It needed `T_sim`
known upfront and `O(M log M)` memory for the whole run, and bought spectral accuracy that the
OU sections now largely match without those constraints. Note that it handles arbitrary `β`
just as easily, so `β` is not an argument for revisiting it — the grid is still the objection.

**Initialize every section to its stationary variance** — with unit-variance states that is
just `u = Z` at `t = 0`. Zero-initialization biases the first ~10·τ_max of every run low, and it
looks like a settling transient rather than a bug.

Inherent limitation to document: a run of length `T` contains no flicker power below `1/T`.
This is physics of the finite window, not an implementation defect — but per the bullets above
its size depends on `β`, growing as `ln T` at `β = 1` and as `T^(β−1)` beyond it. The
user-facing wording has to say that, because at `β > 1` a user who doubles `StopTime` and sees
the variance move will otherwise read it as nondeterminism.

## 6. Error budget

| Source | Direction | Magnitude / mitigation |
|---|---|---|
| Bandwidth truncation | **low** | `≈ f_p/f_max` (order 1), `1.5·f_p/f_max` (order 2). Monotone in `f_max`, so bracket by running two values |
| Nonlinear rectification | **high** | `E[f(x+n)] ≠ f(E[x])`. Partly real physics, partly an artifact of over-injected out-of-band power in exponential devices. Opposes the row above, so raising `f_max` is not free |
| Timepoint-selection bias | low, fixable | Rejection is triggered more often after large noise excursions, so accepted timepoints are correlated with noise history. Does not bias the source's law; does bias statistics computed on raw timepoints. Resample uniformly first |
| LTE order loss | cost, not bias | `√Δt` at order 1, `Δt^1.5` at order 2. See §3 |
| Flicker startup | low, transient | Initialize shaping sections to stationary variance (§5.3) |
| Flicker window truncation | **low**, `β`-dependent | No power below `f_min = 1/StopTime`. Grows as `ln T` at `β = 1`, as `T^(β−1)` at `β > 1`. Real physics of the finite window, but at `β > 1` it makes the variance a visible function of run length. Mitigate by extending the ladder below `1/StopTime` (§5.3) |
| Estimator variance | — | Relative standard error of `σ²` is `√(2/(N−1))`: 1 % needs `N ≈ 20 000` runs |
| Itô vs Stratonovich | `O(Δt)` drift | Only for multiplicative noise (PSD evaluated from the noisy operating point). Freezing `σ_∞` at the last accepted point, per §4.1, fixes the convention explicitly |

The nonlinear-rectification row is the reason `f_max` has a useful range rather than a
"higher is better" rule. Sanity check for it: compare the Monte-Carlo mean against the
noiseless transient. A shift that grows as `f_max` rises is the artifact, not the physics.

The timepoint-selection row is the one genuinely new failure mode introduced by dropping the
grid, and it is subtle: the noise process itself remains exactly distributed — `u(t+Δ₂) | u(t)`
is the exact OU transition for whatever `Δ₂` the controller settled on, and the discarded draw
from a rejected attempt is independent of the retry. What is biased is the *sampling grid* of
the output waveform, because the controller's choice of `Δ₂` did depend on the rejected draw.
Uniform resampling in Phase 5 removes it.

The estimator-variance row is what determines whether this feature is being used for its
intended purpose. If a user wants a number to 1 %, point them at the AC `Noise` analysis.

## 7. Validation

1. **Exactness of the transition.** No circuit. Drive the shaping state over a deliberately
   irregular step sequence spanning several decades of `λΔt`, and check the sample variance
   against 1 and the sample autocorrelation at lag `Δt` against `e^{−λΔt}`. Also exercise the
   closed-form/series crossover of §4.7 from both sides. **Write this first, in Phase 0** — it
   catches a wrong variance law, a wrong propagator, and the `z³` cancellation, all without any
   of the rest of the feature existing. `TimeNoisePoint` being a pure struct over `(z, order)`
   is what makes this test a plain unit test with no simulation in it.
2. **`kT/C` on an RC lowpass.** Monte-Carlo output variance must converge to `kT/C` times the
   captured fraction of §3 — `kT/C · f_max/(f_max + f_p)` at order 1. Exact closed form, so this
   is a tight test rather than an asymptotic one.
3. **PSD overlay.** Welch estimate of the Monte-Carlo output must overlay the `Noise` analysis
   curve **multiplied by the shaping filter's `|H(f)|²`**. Note the change from the earlier
   revision: the injected noise is deliberately not white, so a bare overlay against `Noise` is
   expected to diverge above `f_max` and is not a valid test.
4. **Step-size independence.** The central correctness test. Run with tolerances and `MaxStep`
   forcing substantially different step sequences (and hence many rejections in one case) and
   confirm the statistics agree after uniform resampling. `σ_∞` is step-free by construction, so
   this test exists to catch a `Δt` dependence accidentally reintroduced later.
5. **Bandwidth convergence.** Raising `f_max` must move the result toward the white-noise
   analytic answer, monotonically, along the §3 curve.
6. **Reproducibility.** Same seed and same `CurrentRun` must reproduce bit-identically across
   `Rerun`, mirroring `When_NoiseRerun_Expect_Same` in
   [NoiseTests.cs:13](../SpiceSharpTest/Simulations/NoiseTests.cs:13).
7. **Shot noise on a diode.** Independent check of a second source type against AC noise.
8. **Sharing is coefficients, not state.** `M` identical resistors in parallel must give each
   source an independent realization with the correct individual statistics, and a total
   variance `M×` a single one. The failure this guards against is specific to §4.3: sharing a
   `TimeNoisePoint` is correct, accidentally sharing an RNG stream or a shaping state is not,
   and both mistakes still produce plausible-looking noise. Also assert that inserting an
   unrelated device leaves every other source's realization bit-identical, which is what §4.6's
   hash-by-name buys.
9. **Flicker ladder against `1/f^β`.** No circuit — the weight law of §5.3 is a pure function of
   the poles and `β`, so evaluate `Σ 4σ_i²λ_i/(λ_i² + ω²)` analytically on a log-`f` grid and
   check the slope and the ripple **strictly in-band**, over `β ∈ {0.8, 1.0, 1.2, 1.5}`. Assert
   `β = 1` reproduces equal weights exactly, which is what pins the generalization to the
   default. Deliberately **do not** assert total variance against the band integral: §5.3 shows
   the two differ by `sin(πβ/2)` by construction, so that test would fail at every `β ≠ 1` for
   no reason. A separate Monte-Carlo Welch estimate then confirms the realization matches the
   analytic ladder PSD, which is the part that catches a wrong `√w_i` ordering between
   `FlickerWeights` and `FlickerLadder` (§4.3) — the two arrays are indexed in lockstep and
   nothing in the type system says so.

## 8. Cost

Injected noise perturbs tracked charge states and inflates the LTE estimate, so the integrator
takes smaller steps than the noiseless run. This is a cost, not a bias — the injected process
does not depend on the step size — but it is real, and the reduced LTE order of §3 makes it
worse than it would be for a smooth forcing function. Order-2 shaping is the main lever.

Expect to loosen `AbsoluteTolerance` / `ChargeTolerance`, and expect single runs meaningfully
slower than the deterministic equivalent, on top of the `N`-run Monte-Carlo factor. The
Monte-Carlo loop parallelizes across simulations; `ConcurrentSimulationsTests` already covers
that ground.

`MinStep` exhaustion should no longer be a systematic failure mode — the step-size feedback is
negative (§3) — but stiff circuits can still hit it for ordinary reasons. Worth surfacing a
clear diagnostic distinguishing "noise injection drove the step down" from a convergence
failure.

## 9. Open questions

- **Per-source `f_max`.** A single global value is assumed, and §4.3 now leans on it: the shared
  `TimeNoisePoint` is only shareable because `λ` is global. A per-source override would still
  work — the shaping state is per-source already — but it would move that source off the shared
  point and onto its own `Exp`/`Sqrt` per step, exactly the cost §4.4 exists to avoid. The
  natural shape is a small cache keyed by `f_max`, so a handful of distinct bandwidths still
  costs a handful of points rather than one per source. Worth doing only if devices with wildly
  different bandwidths turn out to be common.
- **Inferring `f_max`.** Less pressing than inferring the old `Δt_n`, because `f_max` is a
  physical specification a user can state directly rather than a solver tuning number. A default
  derived from a pole estimate would still be friendlier, but it needs an eigenvalue estimate
  the framework does not currently produce.
- **Shaping order above 2.** The `n`-pole propagator generalizes (`e^{AΔ}` stays closed-form for
  a repeated real pole), but `Q(Δ)` grows and the small-`z` cancellation worsens with each order.
  Order 3 is probably the practical ceiling without a more careful reformulation. Cheap to try,
  at least: the change is contained in the `TimeNoisePoint` constructor and the loop bound in
  `TimeNoiseSource.Probe`.
- **Where `β` comes from.** The weight law of §5.3 takes any `β ∈ (0, 2)`, but no device can
  currently supply one: there is no `ef` model parameter in the framework, so every source
  constructs with the default 1. The exponent becomes reachable either when a mosfet level with
  a BSIM-style noise model lands, or — sooner and for free — through the `TimeNoiseCurrentSource`
  extension point, since a user-defined device can already pass its own `β`. Adding `ef` to the
  existing SPICE3 models is *not* proposed: they are ports of a reference implementation that
  hardcodes `1/f`, and the AC `Noise` analysis would then disagree with `NoiseTransient` on the
  same netlist.
- **Arbitrary target densities.** `1/f^β` is a one-parameter family, and the ladder does not
  actually need it to be: fitting `{w_i}` by non-negative least squares against a tabulated
  target on a log-`f` grid costs nothing extra at runtime, and covers piecewise slopes,
  generation-recombination bumps and measured data. The analytic law stays as the closed form
  and as the initial guess. Worth doing only once a device asks for a shape a single exponent
  cannot express — but §4.3's split between the shared `FlickerLadder` and the per-`β`
  `FlickerWeights` is already the shape this needs, so nothing has to be undone first.
- **Correlated sources.** `ITimeNoiseSource` yields a scalar and there is no covariance between
  sources, so induced gate noise has no representation. Per §4.2 the vector-`u` shape leaves
  room for a per-device Cholesky factor at stamping time; retrofitting correlation onto
  independent streams is much harder than leaving room for it. Note this factor is a *device*
  property, not a step property, so it belongs on the source next to `σ_∞` and not on the
  shared state.
- **Reproducibility across tolerance changes.** Explicitly not promised (§1). If users turn out
  to want it — e.g. to compare a fast and an accurate run of the same realization — it requires
  going back to a time-indexed construction, which for OU means a bridge rather than a grid.
  Worth reconsidering only if the demand is real.
