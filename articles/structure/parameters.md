# Parameters

Parameters and properties are your main way of configuring the behavior of an entity in the circuit. They are typically specified in a **[ParameterSet](xref:SpiceSharp.ParameterSet)**.

## Parameter attributes
It is possible to provide parameters with attributes containing more runtime meta-information.

- **[ParameterNameAttribute](xref:SpiceSharp.Attributes.ParameterNameAttribute)**: Tags the property, method or field with a specific name. Multiple names can be specified..
- **[ParameterInfoAttribute](xref:SpiceSharp.Attributes.ParameterInfoAttribute)**: Adds more information about the parameter.
  - *[Description](xref:SpiceSharp.Attributes.ParameterInfoAttribute#SpiceSharp_Attributes_ParameterInfoAttribute_Description)* gives more information about the parameter.
  - *[Interesting](xref:SpiceSharp.Attributes.ParameterInfoAttribute#SpiceSharp_Attributes_ParameterInfoAttribute_Interesting)* indicates whether the parameter is interesting to be shown as a parameter (legacy from Spice 3f5).
  - *[IsPrincipal](xref:SpiceSharp.Attributes.ParameterInfoAttribute#SpiceSharp_Attributes_ParameterInfoAttribute_IsPrincipal)* indicates that this parameter is the *principal* design parameter of the entity. Examples are the resistance, capacitance and inductance of a resistor, capacitor and inductor. Using this flag allows you to find the parameter without specifying the parameter name.

The biggest advantage that these attributes provide, is that they can be used in conjunction with the **[ParameterHelper](xref:SpiceSharp.ParameterHelper)** extension methods. These methods allow you to set or get parameters based on their **[ParameterNameAttribute](xref:SpiceSharp.Attributes.ParameterNameAttribute)**.
- You can set or get a parameter immediately by its name, using the *[SetParameter](xref:SpiceSharp.ParameterHelper.SetParameter(System.Object,System.String,System.Collections.Generic.IEqualityComparer{System.String}))* and *[GetParameter](xref:SpiceSharp.ParameterHelper.GetParameter``1(System.Object,System.String,System.Collections.Generic.IEqualityComparer{System.String}))*. You can also use the *TrySetParameter* and *TryGetParameter* variants if want to test whether or not the parameter exist.
- You can create a getter or setter using the *[CreateSetter](xref:CreateSetter<T>(String, IEqualityComparer<String>))* and *[CreateGetter](xref:SpiceSharp.Behaviors.Behavior.CreateGetter``1(SpiceSharp.Simulations.Simulation,System.String,System.Collections.Generic.IEqualityComparer{System.String}))* methods. This gives fast access, and bypasses slower reflection (although it is cached).

## Parameter objects
Any member (property, field, method) can be named using the above mentioned attributes. Furthermore, the system also supports deep cloning of objects using reflection, which may be necessary when running multiple simulations on multiple threads. However, when it encounters a member that cannot be cloned, then the member will be copied by reference if possible. If the property implements **[ICloneable](xref:SpiceSharp.ICloneable)**, then the member will either be cloned (if the member is publicy settable), or the property will be copied using *[CopyFrom](xref:SpiceSharp.ICloneable.CopyFrom(SpiceSharp.ICloneable))*. In other cases, the member is ignored and should be implemented manually.

Spice# provides a basic **[Parameter<T>](xref:SpiceSharp.Parameter`1)** class for generic parameters that implement **[ICloneable](xref:SpiceSharp.ICloneable)**. It implements getting and setting a *[Value](xref:SpiceSharp.Parameter`1.Value)* property, for which custom logic can be implemented. The most common use of this class is the **[GivenParameter](xref:SpiceSharp.GivenParameter`1)** class. Many parameters in Spice also track whether or not they have been specified by the user or not. This class also exposes a *[Given](xref:SpiceSharp.GivenParameter`1.Given)* property that will resolve to *true* if the value has been set. This is often used to find out if a default property has to be calculated.

<div class="pull-left">[Previous: Entities, components and models](entities.md)</div> <div class="pull-right">[Next: Custom models](../custom_components/custom_models.md)</div>
