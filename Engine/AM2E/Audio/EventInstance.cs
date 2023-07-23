using FMOD.Studio;

namespace AM2E;

#region Design Notes

/*
 * I'm an FMOD noob. This wrapper was initially written by M3D!
 */

#endregion

/// <summary>
/// Container class for an instance of a fmod event
/// </summary>
public class EventInstance 
{
    // Underlying FMOD event object
    private FMOD.Studio.EventInstance myEvent;
    

    /// <summary>
    /// Default constructor, creates the event
    /// </summary>
    /// <param name="myEvent"></param>
    public EventInstance(FMOD.Studio.EventInstance myEvent) 
    {
        this.myEvent = myEvent;
    }


    #region Mutators

    /// <summary>
    /// Plays the given event
    /// </summary>
    public void Start() 
    {
        myEvent.start();
    }

    /// <summary>
    /// Stops the event.
    /// </summary>
    /// <param name="immediate">Whether or not to let the ASDR Faders/etc. play out. When set to true, will hard cutoff the sound </param>
    public void Stop(bool immediate = false) 
    {
        var stopMode = immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT;
        myEvent.stop(stopMode);
    }

    /// <summary>
    /// Pauses the event
    /// </summary>
    public void Pause() 
    {
        myEvent.setPaused(true);
    }

    public void SetParameter(string parameterName, float value) 
    {
        Audio.FMODCall(myEvent.setParameterByName(parameterName, value), "Param set: ");
    }

    public void SetParameter(string parameterName, string value) 
    {
        Audio.FMODCall(myEvent.setParameterByNameWithLabel(parameterName, value), "Param set str: ");
    }
    #endregion


    #region Accessors

    /// <summary>
    /// Checks if a given event is playing
    /// </summary>
    /// <returns>True if the event is playing, false if it is not</returns>
    public bool GetPaused() 
    {
        // Check if the event is playing
        myEvent.getPlaybackState(out var currentState);

        return (currentState == PLAYBACK_STATE.PLAYING);
    }

    public bool GetStopped() 
    {
        // Check if the event is playing
        myEvent.getPlaybackState(out var currentState);

        return currentState is PLAYBACK_STATE.STOPPED or PLAYBACK_STATE.STOPPING;
    }

    /// <summary>
    /// Gets the pitch of the event
    /// </summary>
    /// <returns>Pitch's multiplier value</returns>
    public float GetPitch() 
    {
        myEvent.getPitch(out var pitch);
        return pitch;
    }

    /// <summary>
    /// Gets the volume of a instance
    /// </summary>
    /// <returns>The volume as a float</returns>
    public float GetVolume() 
    {
        myEvent.getVolume(out var volume);

        return volume;
    }

    public string GetPath() 
    {
        // Get the event's description
        myEvent.getDescription(out var description);
        description.getPath(out var path);
        
        // We remove the first 7 characters to get rid of the "event:/" prefix
        return path.Remove(0,7);
    }

    #endregion

}