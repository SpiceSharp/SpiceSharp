# Small Signal analysis

Small signal analysis assumes that a signal amplitude is so small that the circuit can be assumed to behave **linearly**. It starts out by calculating the **[Operating point](operatingpoint)** and then linearizing the circuit. It then calculates the complex output.

<p align="center"><img src="images/frequency.svg" /></p>

## Frequently encountered issues

| Symptom | Possibly due to ... |
|:--------|:--------------------|
| The solution is always 0 | There might not be an independent source with an AC magnitude and/or phase. |