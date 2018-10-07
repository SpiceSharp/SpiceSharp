# Home

<p align="center"><img width="150px" src="api/images/logo_full.svg" alt="SpiceSharp" /></p>

SpiceSharp is a circuit simulator originally based on Spice 3f5 by Berkeley University - hence the resemblance of the logo. This is also the ancestor for a lot of modern commercially available spice-based simulators (PSpice, HSpice, SmartSpice, etc.). Spice-based simulators still remain the industry standard for circuit simulation today.

These simulators have typically been continued in C++. This had the following unfortunate consequences:
- Some obsolete/buggy code from the original Berkeley Spice can still be found in for example ngSpice.
- It is *not* object-oriented.
- It is *not* easily understood and expanded.
- It is usually not cross-platform.
I originally wanted to learn more about circuit simulation and their techniques. Transforming the original Spice project to a completely managed, completely object-oriented piece of software definitely proved to be a challenge. A lot of bugs have been sorted out, and the project in general has evolved quite nicely.

## Tutorial
You can find a tutorial for using Spice# [here](articles/gettingstarted.md)

## API
The API can be found [here](api/index.md).
