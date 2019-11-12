namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Specifies that the class uses behaviors of a certain type.
    /// </summary>
    /// <typeparam name="B">The behavior type that is used by the class.</typeparam>
    public interface IBehavioral<B> where B : IBehavior
    {
    }
}
