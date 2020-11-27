# Dependency injection

Spice# has a basic implementation for inversion of control, which allows you to register your own behaviors for components. The class that tracks all of this is **[DI](xref:SpiceSharp.Entities.DependencyInjection.DI)**.

By default, the static **[DI](xref:SpiceSharp.Entities.DependencyInjection.DI)** class automatically searches the assembly for components and behaviors  the first time it is invoked. This is done using reflection, and if you want to keep the startup very light-weight you can choose to disable this behavior by clearing the **[ScanIfNotFound](xref:SpiceSharp.Entities.DependencyInjection.DI#SpiceSharp_Entities_DependencyInjection_DI_ScanIfNotFound)** flag, in which case you will need to add the necessary behaviors and components manually.
Searching the assembly is done by looking at the **[BehaviorForAttribute](xref:SpiceSharp.Attributes.BehaviorForAttribute)** on behavior classes. This allows you to flag a behavior to be used for a specific component.

The class allows you to manually register all the behaviors of an assembly, or add behaviors for entities manually using the **[RegisterBehaviorFor](xref:SpiceSharp.Entities.DependencyInjection.DI#SpiceSharp_Entities_DependencyInjection_DI_RegisterBehaviorFor_)** method. Overriding existing behaviors can be done by registering behaviors with a higher priority such that they are checked first.

## Deciding on the right behavior

Once the **[DI](xref:SpiceSharp.Entities.DependencyInjection.DI)** container needs to decide on behaviors for the specified simulation and component, it will go over each found behavior in order of decreasing *priority*. If the behavior implements all the necessary **[IBehavior](xref:SpiceSharp.Behaviors.IBehavior)** interfaces that are requested by the simulation, then the behavior is created and added to the simulation.

### Example: the diode

A transient simulation requests the following behaviors from a **[Diode](xref:SpiceSharp.Components.Diode)**:
- **[ITimeBehavior](xref:SpiceSharp.Behaviors.ITimeBehavior)**
- **[IBiasingBehavior](xref:SpiceSharp.Behaviors.IBiasingBehavior)**
- **[IConvergenceBehavior](xref:SpiceSharp.Behaviors.IConvergenceBehavior)**
- **[ITemperatureBehavior](xref:SpiceSharp.Behaviors.ITemperatureBehavior)**

The container checks the following behaviors:
1. **[Noise](xref:SpiceSharp.Components.Diodes.Noise)** has the highest priority so it is checked first. It implements the **[INoiseBehavior](xref:SpiceSharp.Behaviors.INoiseBehavior)** among others which isn't requested by the simulation so it *isn't* created.
2. **[Frequency](xref:SpiceSharp.Components.Diodes.Noise)** has the same priority as the next step, but since it implements **[IFrequencyBehavior](xref:SpiceSharp.Behaviors.INoiseBehavior)** which isn't requested by the simulation, it *isn't* created.
3. **[Time](xref:SpiceSharp.Components.Diodes.Time)** implements **[ITimeBehavior](xref:SpiceSharp.Behaviors.ITimeBehavior)**, and since it inherits from its biasing behavior it also implements **[IBiasingBehavior](xref:SpiceSharp.Behaviors.IBiasingBehavior)**, **[IConvergenceBehavior](xref:SpiceSharp.Behaviors.IConvergenceBehavior)** and **[ITemperatureBehavior](xref:SpiceSharp.Behaviors.ITemperatureBehavior)**. All of these are requested by the simulation so **this behavior is created**!
4. **[Biasing](xref:SpiceSharp.Components.Diodes.Biasing)** contains interfaces that are already created, so this behavior is skipped.
5. **[Temperature](xref:SpiceSharp.Components.Diodes.Biasing)** contains interfaces that are already created so this behavior is skipped.

In a lot of models, quantities and computations need to be shared between different behaviors of a component. The ordering by priority gives the freedom necessery to deal with inheritance to avoid code duplication.

<div class="WARNING">
  <h5>WARNING</h5>
  <p>Note that the **[DI](xref:SpiceSharp.Entities.DependencyInjection.DI)** does not need to concern itself with the order of *creation* of the behaviors. It only makes sure that behaviors of the same behavior type are not added twice.</p>
</div>
