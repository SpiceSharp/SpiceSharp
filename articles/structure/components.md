# Components

Components implement **[IEntity](xref:SpiceSharp.Entities.IEntity)** and are used as a replacement for typical Spice "instances". A component is an entity that can be connected to nodes and can have a model (which is analogeous to Spice "models").

Components can be connected to nodes using the *[Connect](xref:SpiceSharp.Components.IComponent#SpiceSharp_Components_IComponent_Connect_System_String___)* method. The terminals or pins of the component can be given more information by using attributes, which in turn can be used (if they aren't overridden) for circuit validation.

| Attribute | Description |
|:----------|:------------|
| **[ConnectedAttribute](xref:SpiceSharp.Attributes.ConnectedAttribute)** | Specifies two pins to have a resistive path between them. |
| **[IndependentSourceAttribute](xref:SpiceSharp.Attributes.IndependentSourceAttribute)** | Specifies the component to be an independent source (it can inject energy in the circuit by itself). |
| **[VoltageDriverAttribute](xref:SpiceSharp.Attributes.VoltageDriverAttribute)** | Specifies two pins to have a fixed relative voltage. This can be used to detect voltage loops. |
| **[PinAttribute](xref:SpiceSharp.Attributes.PinAttribute)** | Gives a pin/terminal a name. Only used for information during runtime. |