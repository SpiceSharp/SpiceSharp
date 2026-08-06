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

## 2. What already exists

| Piece | Location | Reused how |
|---|---|---|
| Variable-step integration, LTE control, rejection | `Trapezoidal` / `Gear` | unchanged |
| `ITruncatingBehavior.Prepare()/Evaluate()` | [Sampler/Accept.cs](../SpiceSharp/Components/Sampler/Accept.cs) | land the integrator on noise-grid points |
| `IAcceptBehavior.Probe()/Accept()` | [IAcceptBehavior.cs](../SpiceSharp/Simulations/Implementations/Time/IAcceptBehavior.cs) | freeze the noise sample for a timepoint |
| `ElementSet<double>` RHS stamping | `CurrentSource.Biasing` | inject the noise current |
| `Rerun` / `Repeat` / `CurrentRun` | [Simulation.cs](../SpiceSharp/Simulations/Simulation.cs) | Monte-Carlo loop reusing setup |
| AC `Noise` analysis | [Noise.cs](../SpiceSharp/Simulations/Implementations/Noise/Noise.cs) | ground truth for validation |

Two properties of the existing code shape the design:

- `TruncateNodes` defaults to `false` ([SpiceMethod.cs:80](../SpiceSharp/Simulations/Implementations/Time/IntegrationMethods/Spice/SpiceMethod.cs:80)),
  so raw node voltages are not in the LTE estimator. Tracked capacitor charges still are.
- `MinStep` defaults to `MaxStep × 1e-9` and `Reject()` divides the step by 8 before
  throwing `TimestepTooSmallException`. Runaway step reduction terminates, but it terminates
  the simulation.

## 3. The central design decision

**The noise grid is fixed and independent of the integration grid.**

Each source is defined on a uniform grid of spacing `Δt_n` (configurable). The sample for a
given time is a pure function of `(seed, sourceId, floor(t / Δt_n))`. The integrator then
takes whatever variable steps it likes, constrained to land on grid points.

Sample variance for a one-sided PSD `S` (units²/Hz), zero-order hold over the grid:

```
σ² = S / (2·Δt_n)
```

with the resulting continuous-time process having one-sided PSD `2σ²Δt_n·sinc²(πfΔt_n)`,
i.e. flat at `S` for `f ≪ 1/Δt_n`, and effective noise bandwidth exactly `1/(2Δt_n)`.

Concretely:

| Source | One-sided PSD | Sample variance |
|---|---|---|
| Thermal | `S_i = 4kTG` | `σ² = 2kTG / Δt_n` |
| Shot | `S_i = 2q·|I|` | `σ² = q·|I| / Δt_n` |
| Flicker | `S_i = KF·I^AF / f` | via shaping filter, §5.3 |

### Why not scale by the integration step

Drawing a fresh sample per accepted step with `σ² = S/(2Δt_k)` is the intuitive
implementation and it is wrong in two ways:

1. **The noise floor becomes correlated with circuit activity.** Injected bandwidth is
   `1/(2Δt_k)`, so fine steps inject a wider band and coarse steps a narrower one. The
   result is more apparent noise near switching edges — physically plausible-looking, and
   therefore very hard to catch by inspection.

2. **Positive feedback with the LTE controller.** Noise perturbs the tracked charge states;
   the divided-difference estimator in
   [Trapezoidal.Instance.DerivativeInstance.cs:91](../SpiceSharp/Simulations/Implementations/Time/IntegrationMethods/Spice/Trapezoidal/Trapezoidal.Instance.DerivativeInstance.cs:91)
   sees the jaggedness and shrinks the step; a smaller step means a *larger* injected
   amplitude, which means more jaggedness. This converges on `MinStep` and throws.

A fixed noise grid eliminates both, and additionally makes step rejection harmless: the
waveform is a function of time, so a retried step at a different `Δt` sees a consistent
realization. This is the same property `Pwl` relies on.

### Choosing Δt_n

For a first-order pole with time constant `τ`, the ZOH-injected process captures a fraction
`(1/a²)·[a − (1−e^{−2a})/2]` of the total noise power, with `a = Δt_n/(2τ)`. Expanding:

> **relative noise-power deficit ≈ Δt_n / (3·τ_fastest)**

| `Δt_n` | power captured |
|---|---|
| `τ/10` | 96.7 % |
| `τ/20` | 98.3 % |
| `τ/33` | 99.0 % |
| `τ/40` | 99.2 % |

Halve the deficit if quoting σ rather than power. The `sinc²` rolloff of the hold is already
included; do not apply it again as a separate correction.

`Δt_n` must be chosen from the fastest pole whose noise contribution matters, **not** from the
accuracy needed for the deterministic trajectory. These are usually different by an order of
magnitude, and that gap is the dominant cost of the analysis.

## 4. Architecture

### 4.1 New behavior interface

```csharp
[SimulationBehavior]
public interface ITimeNoiseBehavior : IBehavior
{
    /// Refresh noise densities from the last accepted operating point and
    /// draw the realization for the currently probed timepoint. Called once
    /// per probed timepoint, never inside the Newton loop.
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

Devices implementing this also implement `ITruncatingBehavior`, returning the distance to the
next grid point from `Prepare()` and `+∞` from `Evaluate()` — exactly the `Sampler` pattern.
This needs no change to `Transient`, which already collects `ITruncatingBehavior` and applies
`_method.Truncate()` over them.

### 4.2 Time-domain noise source primitives

Mirroring `NoiseSource` / `NoiseThermal` / `NoiseShot` / `NoiseGain`, but operating on
`IVariable<double>` from `IBiasingSimulationState` and owning an `ElementSet<double>`:

```
TimeNoiseSource            (abstract: grid, RNG stream, stamping)
├── TimeNoiseThermal       Compute(conductance, temperature)
├── TimeNoiseShot          Compute(current)
├── TimeNoiseGain          Compute(density)          // caller supplies S directly
└── TimeNoiseFlicker       Compute(coefficient, exponent, current)   // shaped, §5.3
```

Note the deliberate difference from the AC sources: these produce a **raw PSD**, with no
transfer-function factor. In the AC path, `NoiseThermal.Compute` multiplies by the adjoint
gain `|ΔV|²`, fusing PSD and transfer function into one number
([NoiseThermal.cs:37](../SpiceSharp/Components/Noise/NoiseThermal.cs:37)). That fusion is why
the AC sources cannot be reused here.

### 4.3 New simulation

```csharp
public partial class NoiseTransient : Transient,
    IBehavioral<ITimeNoiseBehavior>,
    IStateful<ITimeNoiseSimulationState>,
    IParameterized<NoiseTransientParameters>
```

`NoiseTransientParameters` carries `NoiseTimestep` (`Δt_n`), `Seed`, and flicker
configuration. `ITimeNoiseSimulationState` owns the grid, the master seed, and the
time-to-sample-index mapping.

`Execute` hooks `AfterLoad` to call `Inject()` on every behavior, and calls `Probe()` on each
behavior from the probe phase. Everything else is inherited.

### 4.4 Random number generation

**Counter-based, not sequential.** The stream must be indexable as
`hash(seed, sourceId, sampleIndex)` — a splitmix64/philox-style mixer, with Gaussians from
Box-Muller or an inverse-CDF over the mixed bits.

A sequential `System.Random` is disqualified: its state advances with call order, which
depends on step rejections, on how many sources exist, and on the order behaviors are
visited. It would silently make results irreproducible and rejection-history-dependent.
`System.Random`'s algorithm has also changed across .NET versions, so it is not reproducible
across runtimes either.

Per-run seed for the Monte-Carlo driver: derive from `CurrentRun`, which `Simulation` already
exposes.

## 5. Implementation phases

### Phase 0 — Grid, RNG, and the validation harness

No device changes. A hand-placed noise current source on an RC, driven by the grid and RNG.
Ship the `kT/C` test (§7) first — it fails loudly on exactly the mistakes that are invisible
by inspection.

### Phase 1 — Vertical slice

`ITimeNoiseBehavior`, `TimeNoiseSource` + `TimeNoiseThermal`, `NoiseTransient`,
`NoiseTransientParameters`, and the `Resistor` time-noise behavior. End-to-end on one device.

### Phase 2 — Device coverage

Diode, BJT, MOSFET levels 1/2/3 — the set that currently has an `INoiseBehavior`. Thermal and
shot only at this stage; flicker stubbed.

### Phase 3 — Flicker

Shaping filter, see §5.3 below.

### Phase 4 — Composition

Time-domain counterparts of
[Subcircuits/Behaviors/Noise.cs](../SpiceSharp/Components/Subcircuits/Behaviors/Noise.cs) and
`ParallelComponents/Behaviors/Noise.cs`. Simpler than the AC versions, since there is no
adjoint solve to mirror through a local solver — sources just stamp into the parent RHS.

### Phase 5 — Monte-Carlo driver and statistics

A thin driver looping `Rerun` with `seed = f(CurrentRun)`, plus ensemble exports (mean,
variance, percentile envelopes) and a Welch PSD estimator for validation. Keep it optional —
users doing jitter extraction will want the raw sample paths.

### 5.3 Flicker noise

The awkward one. `1/f` has unbounded power at DC and long-range correlation, so it cannot be
produced by scaling white samples.

Two viable generators:

- **Sum of Lorentzians.** N first-order AR(1) sections with poles spread logarithmically,
  each recursed on the noise grid. Gives `1/f` to within a few percent over roughly one
  decade per 1.5 sections. Cheap, streaming, no run-length knowledge needed.
- **FFT filtering.** Pre-generate the whole sequence by shaping white noise with `1/√f` in
  the frequency domain. Better spectral accuracy, but needs `T_sim` known upfront and
  `O(M log M)` memory for the entire run.

Start with the Lorentzian sum. **Initialize every section to its stationary variance**, not
to zero — otherwise the first ~10·τ_max of every run is biased low, and it will look like a
settling transient rather than a bug.

Inherent limitation to document: a run of length `T` contains no `1/f` power below `1/T`.
This is physics of the finite window, not an implementation defect.

## 6. Error budget

| Source | Direction | Magnitude / mitigation |
|---|---|---|
| Bandwidth truncation | **low** | `≈ Δt_n/(3τ)`. Monotone in `Δt_n`, so bracket by running two spacings |
| Nonlinear rectification | **high** | `E[f(x+n)] ≠ f(E[x])`. Partly real physics, partly an artifact of over-injected out-of-band power in exponential devices. Opposes the row above, so shrinking `Δt_n` is not free |
| Flicker startup | low, transient | Initialize shaping filters to stationary variance (§5.3) |
| Estimator variance | — | Relative standard error of `σ²` is `√(2/(N−1))`: 1 % needs `N ≈ 20 000` runs |
| Itô vs Stratonovich | `O(Δt)` drift | Only for multiplicative noise (PSD evaluated from the noisy operating point). Freezing the PSD at the last accepted point, per §4.1, fixes the convention explicitly |

The nonlinear-rectification row is the reason `Δt_n` has a useful range rather than a
"smaller is better" rule. Sanity check for it: compare the Monte-Carlo mean against the
noiseless transient. A shift that grows as `Δt_n` shrinks is the artifact, not the physics.

The estimator-variance row is what determines whether this feature is being used for its
intended purpose. If a user wants a number to 1 %, point them at the AC `Noise` analysis.

## 7. Validation

The existing AC noise analysis is exact ground truth, which makes these sharp and cheap.

1. **`kT/C` on an RC lowpass.** Monte-Carlo output variance must converge to
   `kT/C · (1 − Δt_n/(3τ))`. Exercises the dominant bias directly and catches a wrong
   variance-scaling law immediately. **Write this first, in Phase 0.**
2. **PSD overlay.** Welch estimate of the Monte-Carlo output must overlay the `Noise`
   analysis curve across the band.
3. **Monotone convergence.** Halving `Δt_n` must move the result toward the analytic answer.
   If it does not, the noise grid and the integration step are still coupled somewhere.
4. **Reproducibility.** Same seed and same `CurrentRun` must reproduce bit-identically across
   `Rerun`, mirroring `When_NoiseRerun_Expect_Same` in
   [NoiseTests.cs](../SpiceSharpTest/Simulations/NoiseTests.cs).
5. **Rejection invariance.** A run with a tight `MaxStep` forcing many rejections must produce
   the same statistics as a relaxed one. This is the specific test for the §3 failure mode.
6. **Shot noise on a diode.** Independent check of a second source type against AC noise.

## 8. Cost

Even with a fixed noise grid, injected noise perturbs tracked charge states and inflates the
LTE estimate, so the integrator will take smaller steps than the noiseless run. This is a
cost, not a bias — the injected process no longer depends on the step size — but it is real.

Expect to loosen `AbsoluteTolerance` / `ChargeTolerance`, and expect single runs meaningfully
slower than the deterministic equivalent, on top of the `N`-run Monte-Carlo factor. The
Monte-Carlo loop parallelizes across simulations; `ConcurrentSimulationsTests` already covers
that ground.

Stiff circuits may still exhaust `MinStep`. Worth surfacing a clear diagnostic distinguishing
"noise injection drove the step down" from an ordinary convergence failure.

## 9. Open questions

- **Hold vs interpolation.** ZOH is the honest default: exact `sinc²`, simple, and the
  `Δt_n/(3τ)` rule is derived for it. Linear interpolation gives `sinc⁴` — smoother, less
  high-frequency content, but it changes the effective PSD and needs compensation. Deferred.
- **Correlated sources.** `INoiseSource` yields a scalar and there is no covariance between
  sources, so induced gate noise has no representation. `TimeNoiseSource` should be shaped to
  accept a per-device Cholesky factor later even though nothing needs it today; retrofitting
  correlation onto independent streams is much harder than leaving room for it.
- **Grid landing vs capping.** Landing exactly on grid points is preferred over capping below
  them: a ZOH source is discontinuous at grid boundaries, and trapezoidal integration across a
  straddled discontinuity is wrong. `ITruncatingBehavior` soft-limiting is the assumed
  mechanism; breakpoints are the alternative if that proves insufficient.
- **Per-source `Δt_n`.** A single global grid is assumed. Devices with wildly different
  bandwidths might justify per-source grids, at the cost of complicating the truncation logic.
- **Should `Δt_n` be inferable?** Deriving it from a pole estimate would be friendlier than
  making the user compute `τ_fastest/20`, but it needs an eigenvalue estimate the framework
  does not currently produce.
