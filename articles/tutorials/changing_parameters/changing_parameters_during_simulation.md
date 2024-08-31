# Changing parameters during simulation

Spice# makes it possible to change parameters during simulations when the simulation is exporting data. This allows you to customize behaviors with much more freedom.

In order to use this feature, it is first important to realize its limitations:
- When changing something during simulation, there is a chance that convergence becomes more difficult or even impossible. Depending on the situation, you should avoid making changes that are too drastic.
- You will need a good understanding of the flows of the [structure](../../structure/flow.md) of Spice#, as well as details about the parameters that you want to change. For example:
  - It is not uncommon that parameters are used to compute (multiple) derived properties in some other behaviors. You may have to call those behaviors manually if the simulation flow doesn't do this for you automatically.
  - You will also need to know *how* and *where* you can tap into the simulation to change parameters.
