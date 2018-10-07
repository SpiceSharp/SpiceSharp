# Parameters

Parameters and properties are your main way of configuring the behavior of an entity in the circuit. They are typically specified in a **[ParameterSet](xref:SpiceSharp.ParameterSet)**.

## Parameter objects
Spice# provides a basic **[Parameter<T>](xref:SpiceSharp.Parameter`1)** class for generic parameters. It implements getting and setting a value, for which custom logic can be implemented.

The most common use of this class is the **[GivenParameter](xref:SpiceSharp.GivenParameter`1)** class. Many parameters in Spice also track whether or not they have been specified by the user or not. This class also exposes a *Given* property that will resolve to *true* if the value has been set.

## Parameter attributes
It is possible to provide parameters with attributes containing more meta-information.

- **[ParameterNameAttribute](xref:SpiceSharp.Attributes.ParameterNameAttribute)**: Tags the property, method or field with a specific name. Multiple names can be specified. Using this attribute enables accessing these members using reflection. These can generally be accessed because of the following classes:
  - **[NamedParameterized](xref:SpiceSharp.Attributes.NamedParameterized)**
    - *[CreateGetter<T>()](xref:SpiceSharp.Attributes.NamedParameterized#SpiceSharp_Attributes_NamedParameterized_CreateGetter__1)* creates a delegate that gets the member value of type *T* tagged by the specified name. Example:

      [!code-csharp[CreateGetter example](../SpiceSharpTest/BasicExampleTests.cs#example_parameters_mos1_creategetter)]
    - *[CreateSetter<T>()](xref:SpiceSharp.Attributes.NamedParameterized#SpiceSharp_Attributes_NamedParameterized_CreateSetter__1)* creates a delegate that sets the member value of type *T* tagged by the specified name. Example:

      [!code-csharp[CreateSetter example](../SpiceSharpTest/BasicExampleTests.cs#example_parameters_mos1_createsetter)]
  - **[ParameterSet](xref:SpiceSharp.ParameterSet)** (also implements **[NamedParameterized](xref:SpiceSharp.Attributes.NamedParameterized)**)
    - *[GetParameter<T>()](xref:SpiceSharp.ParameterSet#SpiceSharp_ParameterSet_GetParameter__1_System_String_System_Collections_Generic_IEqualityComparer_System_String__)* gets a **[Parameter<T>](xref:SpiceSharp.Parameter`1)** object tagged by the specified name. Example:

      [!code-csharp[GetParameter example](../SpiceSharpTest/BasicExampleTests.cs#example_parameters_mos1_getparameter)]
    - *[SetParameter(string)](xref:SpiceSharp.ParameterSet#SpiceSharp_ParameterSet_SetParameter_System_String_)* calls a method without parameters that is tagged by the specified name. Can be useful for activating certain flags.
    - *[SetParameter(string, object)](xref:SpiceSharp.ParameterSet#SpiceSharp_ParameterSet_SetParameter_System_String_System_Object_System_Collections_Generic_IEqualityComparer_System_String__)* will set a property or method with one argument, tagged by the specified name. This method can be used to pass classes to a named parameter.
    - *[SetParameter<T>()](xref:SpiceSharp.ParameterSet#SpiceSharp_ParameterSet_SetParameter__1___0_)* sets a property, field or method, or **[Parameter<T>](xref:SpiceSharp.Parameter`1)**-object tagged by the specified name with the specified value. Example:

      [!code-csharp[SetParameter example](../SpiceSharpTest/BasicExampleTests.cs#example_parameters_mos1_setparameter)]

  - **[Behavior](xref:SpiceSharp.Behaviors.Behavior)** (also implements **[NamedParameterized](xref:SpiceSharp.Attributes.NamedParameterized)**)
    - *[CreateGetter(Simulation,string)](xref:SpiceSharp.Behaviors.Behavior#SpiceSharp_Behaviors_Behavior_CreateGetter_SpiceSharp_Simulations_Simulation_System_String_System_Collections_Generic_IEqualityComparer_System_String__)* creates a delegate for extracting a behavior parameter from the specified simulation. The simulation argument needs to be the simulation that owns the behavior.
    - *[CreateSetter<T>(Simulation,string)](xref:SpiceSharp.Behaviors.Behavior#SpiceSharp_Behaviors_Behavior_CreateGetter__1_SpiceSharp_Simulations_Simulation_System_String_System_Collections_Generic_IEqualityComparer_System_String__)* creates a delegate for extracting a behavior parameter from the specified simulation. The simulation argument needs to be the simulation that owns the behavior.
- **[ParameterInfoAttribute](xref:SpiceSharp.Attributes.ParameterInfoAttribute)**: Adds more information about the parameter.
  - *[Description](xref:SpiceSharp.Attributes.ParameterInfoAttribute#SpiceSharp_Attributes_ParameterInfoAttribute_Description)* gives more information about the parameter.
  - *[Interesting](xref:SpiceSharp.Attributes.ParameterInfoAttribute#SpiceSharp_Attributes_ParameterInfoAttribute_Interesting)* indicates whether the parameter is interesting to be shown as a parameter (legacy from Spice 3f5).
  - *[IsPrincipal](xref:SpiceSharp.Attributes.ParameterInfoAttribute#SpiceSharp_Attributes_ParameterInfoAttribute_IsPrincipal)* indicates that this parameter is the *principal* design parameter of the entity. Examples are the resistance, capacitance and inductance of a resistor, capacitor and inductor. Using this flag allows you to find the parameter without specifying the parameter name. For example:

    [!code-csharp[SetParameter IsPrincipal example](../SpiceSharpTest/BasicExampleTests.cs#example_parameters_res_setparameter)]

<div class="pull-left">[Previous: Entities, components and models](entities.md)</div> <div class="pull-right">[Next: Custom models](custom_models.md)</div>
