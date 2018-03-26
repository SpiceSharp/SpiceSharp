# Home

<p align="center"><img src="api/images/logo_full.svg" alt="SpiceSharp" /></p>

SpiceSharp is a circuit simulator originally based on Spice 3f5 by Berkeley University. This was also the ancestor for a lot of commercially available spice-based simulators (PSpice, HSpice, SmartSpice, etc.). An open source project to improve and maintain Spice was also started called ngSpice. Spice-based simulators still remain the industry standard for circuit simulation today. 

These simulators have typically been continued in C++. In the case of ngSpice, many of the original bugs of Spice 3f5 still exist and it is not object-oriented.

This project was originally started as an attempt to learn how Spice works in my favorite language: C#. At the same time, I wanted to refactor it into a purely object-oriented design. After a while I started noticing inconsistencies, bugs and incomplete features that spurred me to try and improve it. At the same time I wanted to guarantee that the models were still accurate.

After a while, the project became very close to being usable, which is when I decided to make the project open source.

## API
The API can be found [here](api/index.md).
