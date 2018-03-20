# Spice# <img src="https://github.com/svenboulanger/SpiceSharp/blob/master/SpiceNetIcon.png?raw=true" width="45px" />
Spice# is a Spice circuit simulator written in C#. The framework is made to resemble the original Berkeley Spice version, but some parts have been altered/improved to fit into the .NET framework.

Unit tests to compare Spice# to Spice 3f5 are used to verify the models. Nevertheless, some small numerical errors may be present as a direct consequence of bug fixes and structural differences.

The main differences between Spice 3f5 and Spice# are:
- Spice 3f5 is not object oriented, Spice# is.
- Spice 3f5 has a bugged NEWTRUNC feature, Spice#'s method will work. Set the integration method's configuration `TruncationMethod` to `PerNode`. The default is `PerDevice` where each device can truncate the timestep.

Please note that this project is by no means meant to compete with existing commercial Spice simulators, although its performance may be similar. I wanted to know more about the Spice simulator, and I wanted to be able to extend its functionality in useful ways (eg. automating simple designs, modeling custom components, etc.)

Spice# is available as a **NuGet Package**.

[![NuGet Badge](https://buildstats.info/nuget/spicesharp)](https://www.nuget.org/packages/SpiceSharp/) SpiceSharp <br />

[![Build status](https://ci.appveyor.com/api/projects/status/hhg89ejd795ykmvh?svg=true)](https://ci.appveyor.com/project/svenboulanger/spicesharp)
[![Build Status](https://travis-ci.org/SpiceSharp/SpiceSharp.svg?branch=development)](https://travis-ci.org/SpiceSharp/SpiceSharp)

You can find the API documentation here:<br />
SpiceSharp: https://svenboulanger.github.io/SpiceSharp/index.html<br />
