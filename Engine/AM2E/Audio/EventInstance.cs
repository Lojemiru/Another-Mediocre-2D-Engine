using System;
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
    private bool stopped = false;
    public event Action<EventInstance, bool> OnStop = (_, _) => { };
    public readonly string Name;

    internal void DoOnStop(bool force)
    {
        if (stopped)
            return;
        
        stopped = true;
        OnStop(this, force);
    }
    
    // Underlying FMOD event object
    private FMOD.Studio.EventInstance myEvent;
    

    /// <summary>
    /// Default constructor, creates the event
    /// </summary>
    /// <param name="myEvent"></param>
    public EventInstance(FMOD.Studio.EventInstance myEvent, string name) 
    {
        this.myEvent = myEvent;
        Name = name;
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
        DoOnStop(immediate);
    }
    
    public void HardStop() {
        Stop(true);
        myEvent.setUserData(IntPtr.Zero);
        myEvent.release();
        // myEvent.setCallback(null); hmm, something still funky here
    }

    /// <summary>
    /// Pauses the event
    /// </summary>
    public void Pause() 
    {
        myEvent.setPaused(true);
    }
    
    public void Resume() {
        myEvent.setPaused(false);
    }

    public void SetParameter(string parameterName, float value) 
    {
        Audio.FMODCall(myEvent.setParameterByName(parameterName, value));
    }

    public void SetParameter(string parameterName, string value) 
    {
        Audio.FMODCall(myEvent.setParameterByNameWithLabel(parameterName, value));
    }
    
    // Callback function expects signature of type
    // BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePointer,
    // IntPtr parameterPtr)
    public void SetCallback(EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackType) {
        myEvent.setCallback(callback, callbackType);
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
    
    public int GetPosition() {
        myEvent.getTimelinePosition(out var timelinePosition);
            
        return timelinePosition;
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
        
        // aaaah
        return path.Split(":/")[1];
    }
    
    public LOADING_STATE GetLoadingState() {
        myEvent.getDescription(out var description);
        description.getSampleLoadingState(out var l);
        return l;
    }

    #endregion

}