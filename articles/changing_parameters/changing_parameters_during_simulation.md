# Changing parameters during simulation

Spice# makes it possible to change parameters during simulations. This allows you to customize
behaviors with much more freedom.

In order to use this feature, it is first important to realize its limitations.
- When changing something during simulation, there is a chance that convergence becomes more difficult or even impossible. Depending on the situation, you should avoid making changes that are too drastic.
- You will need a good understanding of the [structure](../structure/structure.md) of Spice#, as well as details about the parameters that you want to change. For example:
  - If there are behaviors that modify the parameter before using it, you will have to call those behaviors in order for the change to take effect.
  - You will also need to know about the simulation in question. You need to find out *how* and *where* you can tap into the simulation to change parameters.

<div class="pull-left">[Previous: Base behaviors](../custom_components/base_behaviors.md)</div> <div class="pull-right">[Next: Example](example.md)</div>
