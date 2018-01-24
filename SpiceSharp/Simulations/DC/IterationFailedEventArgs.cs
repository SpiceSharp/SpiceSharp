using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Event arguments when an iteration has failed
    /// </summary>
    public class IterationFailedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// A delegate for when an iteration failed
    /// </summary>
    /// <param name="sender">The object sending the event</param>
    /// <param name="args">Arguments</param>
    public delegate void IterationFailedEventHandler(object sender, IterationFailedEventArgs args);
}
