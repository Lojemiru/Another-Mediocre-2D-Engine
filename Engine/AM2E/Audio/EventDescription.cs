namespace AM2E;

#region Design Notes

/*
 * I'm an FMOD noob. This wrapper was initially written by M3D!
 */

#endregion

/// <summary>
/// Representation of an fmod event description
/// </summary>
public class EventDescription 
{
    // The fmod event
    private FMOD.Studio.EventDescription myDescription;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="myDescription">FMOD description object to pass in</param>
    public EventDescription(FMOD.Studio.EventDescription myDescription) 
    {
        this.myDescription = myDescription;
    }

    /// <summary>
    /// Creates a new instance of an event
    /// </summary>
    /// <returns>Reference to instance of an event</returns>
    public EventInstance CreateInstance(string name) 
    {
        myDescription.createInstance(out var eventInstance);
        
        return new EventInstance(eventInstance, name);
    }

    /// <summary>
    /// Gets the number of instances currently active
    /// </summary>
    /// <returns>Number of instances currently active</returns>
    public int GetInstanceCount() 
    {
        myDescription.getInstanceCount(out var instanceCount);
        
        return instanceCount;
    }
}