# Transient noise analysis — design plan

Status: proposal, nothing implemented.

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

Relevant properties of the existing code:

- `Transient.Probe()` calls `_method.Probe()` *before* the behaviors, so `Time` and `BaseTime`
  are both current and `Δt = Time − BaseTime` is available inside a behavior's `Probe()`.
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
| Flicker | `KF·I^AF / f` | sum of OU sections, §5.3 |

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
selection rule, and no counter-based RNG (§4.4).

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
see §4.5.

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

### 4.1 New behavior interface

```csharp
[SimulationBehavior]
public interface ITimeNoiseBehavior : IBehavior
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

### 4.3 Time-domain noise source primitives

Mirroring `NoiseSource` / `NoiseThermal` / `NoiseShot` / `NoiseGain`, but operating on
`IVariable<double>` from `IBiasingSimulationState` and owning an `ElementSet<double>`:

```
TimeNoiseSource            (abstract: shaping state, RNG stream, stamping)
├── TimeNoiseThermal       Compute(conductance, temperature)
├── TimeNoiseShot          Compute(current)
├── TimeNoiseGain          Compute(density)          // caller supplies S directly
└── TimeNoiseFlicker       Compute(coefficient, exponent, current)   // §5.3
```

Note the deliberate difference from the AC sources: these produce a **raw PSD**, with no
transfer-function factor. In the AC path, `NoiseThermal.Compute` multiplies by the adjoint
gain `|ΔV|²`, fusing PSD and transfer function into one number
([NoiseThermal.cs:36](../SpiceSharp/Components/Noise/NoiseThermal.cs:36)). That fusion is why
the AC sources cannot be reused here.

### 4.4 Random number generation

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

### 4.5 Exact discretization, order 2

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

### 4.6 New simulation

```csharp
public partial class NoiseTransient : Transient,
    IBehavioral<ITimeNoiseBehavior>,
    IStateful<ITimeNoiseSimulationState>,
    IParameterized<NoiseTransientParameters>
```

`NoiseTransientParameters` carries `MaximumNoiseFrequency` (`f_max`), `BandLimitOrder`
(default 2), `Seed`, and flicker configuration. `ITimeNoiseSimulationState` owns the master
seed and the source registry.

`Execute` hooks `AfterLoad` to call `Inject()` on every behavior, and calls `Probe()` on each
behavior from the probe phase, passing `Δt = Time − BaseTime`. Each source registers its
shaping state as a `StateValue<double>` via `IIntegrationMethod.RegisterState`, which gives
rollback on rejection for free and — because `StateValue<T>` is not `ITruncatable` — keeps the
noise state out of the LTE estimator. Everything else is inherited.

## 5. Implementation phases

### Phase 0 — Shaping state, RNG, and the validation harness

No device changes, no circuit. Unit-test the OU update directly against its analytic stationary
variance and autocorrelation over a deliberately irregular step sequence (§7.1) — this is the
sharpest and cheapest test in the plan and it needs none of the rest of the feature. Then a
hand-placed noise current source on an RC for the `kT/C` test (§7.2).

### Phase 1 — Vertical slice

`ITimeNoiseBehavior`, `TimeNoiseSource` + `TimeNoiseThermal`, `NoiseTransient`,
`NoiseTransientParameters`, and the `Resistor` time-noise behavior. Order-1 shaping only.
End-to-end on one device.

### Phase 2 — Order-2 shaping and device coverage

The 2×2 propagator of §4.5, then Diode, BJT, MOSFET levels 1/2/3 — the set that currently has
an `INoiseBehavior`. Thermal and shot only at this stage; flicker stubbed.

### Phase 3 — Flicker

See §5.3.

### Phase 4 — Composition

Time-domain counterparts of
[Subcircuits/Behaviors/Noise.cs](../SpiceSharp/Components/Subcircuits/Behaviors/Noise.cs) and
`ParallelComponents/Behaviors/Noise.cs`. Simpler than the AC versions, since there is no
adjoint solve to mirror through a local solver — sources just stamp into the parent RHS. The
per-source RNG streams of §4.4 are what make the parallel case safe.

### Phase 5 — Monte-Carlo driver and statistics

A thin driver looping `Rerun` with `seed = f(CurrentRun)`, plus ensemble exports (mean,
variance, percentile envelopes) and a Welch PSD estimator for validation. Keep it optional —
users doing jitter extraction will want the raw sample paths.

**Statistics must be computed on a uniform resample grid, not on raw accepted timepoints.**
See §6, timepoint-selection bias.

### 5.3 Flicker noise

`1/f` has unbounded power at DC and long-range correlation, so it cannot be produced by scaling
white samples. The generator is a **sum of OU sections with logarithmically spaced poles** —
which is to say, the same primitive as the band limit in §3, instantiated `N` times with
different `τ` and weights. Gives `1/f` to within a few percent over roughly one decade per 1.5
sections, and inherits arbitrary-`Δt` exactness for free.

The FFT-filtering alternative considered in the earlier revision is dropped. It needed `T_sim`
known upfront and `O(M log M)` memory for the whole run, and bought spectral accuracy that the
OU sections now largely match without those constraints.

**Initialize every section to its stationary variance** — with unit-variance states that is
just `u = Z` at `t = 0`. Zero-initialization biases the first ~10·τ_max of every run low, and it
looks like a settling transient rather than a bug.

Inherent limitation to document: a run of length `T` contains no `1/f` power below `1/T`. This
is physics of the finite window, not an implementation defect.

## 6. Error budget

| Source | Direction | Magnitude / mitigation |
|---|---|---|
| Bandwidth truncation | **low** | `≈ f_p/f_max` (order 1), `1.5·f_p/f_max` (order 2). Monotone in `f_max`, so bracket by running two values |
| Nonlinear rectification | **high** | `E[f(x+n)] ≠ f(E[x])`. Partly real physics, partly an artifact of over-injected out-of-band power in exponential devices. Opposes the row above, so raising `f_max` is not free |
| Timepoint-selection bias | low, fixable | Rejection is triggered more often after large noise excursions, so accepted timepoints are correlated with noise history. Does not bias the source's law; does bias statistics computed on raw timepoints. Resample uniformly first |
| LTE order loss | cost, not bias | `√Δt` at order 1, `Δt^1.5` at order 2. See §3 |
| Flicker startup | low, transient | Initialize shaping sections to stationary variance (§5.3) |
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
   closed-form/series crossover of §4.5 from both sides. **Write this first, in Phase 0** — it
   catches a wrong variance law, a wrong propagator, and the `z³` cancellation, all without any
   of the rest of the feature existing.
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

- **Per-source `f_max`.** A single global value is assumed, but nothing in the design requires
  it — the shaping state is per-source already, so a per-source override is parameter plumbing
  rather than a design change. Worth doing if devices with wildly different bandwidths turn out
  to be common.
- **Inferring `f_max`.** Less pressing than inferring the old `Δt_n`, because `f_max` is a
  physical specification a user can state directly rather than a solver tuning number. A default
  derived from a pole estimate would still be friendlier, but it needs an eigenvalue estimate
  the framework does not currently produce.
- **Shaping order above 2.** The `n`-pole propagator generalizes (`e^{AΔ}` stays closed-form for
  a repeated real pole), but `Q(Δ)` grows and the small-`z` cancellation worsens with each order.
  Order 3 is probably the practical ceiling without a more careful reformulation.
- **Correlated sources.** `INoiseSource` yields a scalar and there is no covariance between
  sources, so induced gate noise has no representation. Per §4.2 the vector-`u` shape leaves
  room for a per-device Cholesky factor at stamping time; retrofitting correlation onto
  independent streams is much harder than leaving room for it.
- **Reproducibility across tolerance changes.** Explicitly not promised (§1). If users turn out
  to want it — e.g. to compare a fast and an accurate run of the same realization — it requires
  going back to a time-indexed construction, which for OU means a bridge rather than a grid.
  Worth reconsidering only if the demand is real.
