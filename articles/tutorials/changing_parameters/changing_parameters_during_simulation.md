# Changing parameters during simulation

Spice# makes it possible to change parameters during simulations by registering to events. This allows you to customize behaviors with much more freedom.

In order to use this feature, it is first important to realize its limitations:
- When changing something during simulation, there is a chance that convergence becomes more difficult or even impossible. Depending on the situation, you should avoid making changes that are too drastic.
- You will need a good understanding of the flows of the [structure](../../structure/flow.md) of Spice#, as well as details about the parameters that you want to change. For example:
  - It is not uncommon that parameters are used to compute derived properties in some properties. You may have to call those behaviors manually if the simulator doesn't do this automatically.
  - You will also need to know *how* and *where* you can tap into the simulation to change parameters. Understanding the simulation flow can help with this.
