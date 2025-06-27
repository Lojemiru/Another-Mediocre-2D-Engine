#nullable enable
using System.Runtime.InteropServices;
using AM2E.IO;
using AM2E.Levels;
using FMOD;
using Microsoft.Xna.Framework.Content;

#region Design Notes

/*
 * I'm an FMOD noob. This wrapper was initially written by M3D!
 */

#endregion

namespace AM2E;

public static class Audio 
{
    // Constants
    private const int MAX_CHANNELS = 128;

    // FMOD engine constants
    private const FMOD.Studio.INITFLAGS FMOD_STUDIO_INIT_FLAGS = EngineCore.DEBUG ? FMOD.Studio.INITFLAGS.LIVEUPDATE : FMOD.Studio.INITFLAGS.NORMAL;
    private const INITFLAGS FMOD_INIT_FLAGS = INITFLAGS.NORMAL;
    private static FMOD.Studio.System studio;
    private static Dictionary<string, EventDescription> eventDictionary = new();
    private static List<EventInstance> playingEvents = new();


    private static bool initialized = false;

    /// <summary>
    /// Initalizes the audio engine
    /// </summary>
    public static void Load()
    {
        Logger.Engine("Initializing audio engine... thanks M3D!");

        if (initialized)
            throw new ContentLoadException("Audio banks have already been loaded! Please call Audio.Unload() first.");
        
        initialized = true;
        
        // "Temporary" hack for a .net issue: https://github.com/dotnet/runtime/issues/96337
        // do not ask me how this works it's M3D's code aaaaaaaaa
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            Factory.System_Create(out var tempSystem);
            uint fmodVersion = 0;
            tempSystem.getVersion(out fmodVersion);
            tempSystem.close();
        }
        
        // Create the fmod engine
        if (FMODCall(FMOD.Studio.System.create(out studio))) 
        {
            // Start fmod with advanced settings (stops the -inf optimization!!!)
            var fmodAdvancedSettings = new FMOD.ADVANCEDSETTINGS
            {
                // Set the advanced settings
                vol0virtualvol = -1
            };

            // Get an instance of the system, and apply the settings
            FMODCall(studio.getCoreSystem(out var system));
            FMODCall(system.setAdvancedSettings(ref fmodAdvancedSettings));

            if (FMODCall(studio.initialize(MAX_CHANNELS, FMOD_STUDIO_INIT_FLAGS, FMOD_INIT_FLAGS, 0))) 
            {
                // Load the strings bank first
                LoadBank(AssetManager.GetAudioPath() + "/Master.strings.bank");

                // Load all the banks
                var bankArray = Directory.GetFiles(AssetManager.GetAudioPath());
                foreach (var file in bankArray)
                {
                    // Skip if we're about to reload the strings bank.
                    if (file.Contains("Master.strings.bank"))
                        continue;
                    
                    LoadBank(file);
                }
            }
        }
    }

    public static void Unload()
    {
        StopAll();
        eventDictionary.Clear();
        playingEvents.Clear();
        studio.release();
        GC.Collect();
        initialized = false;
    }

    private static void ThrowIfUninitialized()
    {
        if (!initialized)
            throw new TypeUnloadedException("Audio banks have not been loaded! Please call Audio.Load() first.");
    }

    /// <summary>
    /// Helper function which prints the FMOD_RESULT of a given FMOD function, as well as returning true if there were no errors
    /// Useful for debugging weird-ass fmod issues
    /// </summary>
    /// <param name="statement"></param>
    internal static bool FMODCall(RESULT statement) {
        return statement == RESULT.OK;
    }

    /// <summary>
    /// Loads an FMOD bank
    /// </summary>
    /// <param name="bankPath"> path of the bank to load</param>
    private static void LoadBank(string bankPath)
    {
        // Load the bank into memory
        if (FMODCall(studio.loadBankFile(bankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out var bank))) 
        {
            // Load all the events from the bank into an array
            FMODCall(bank.getEventList(out var eventArray));

            // Load all the events from the name into a dictionary
            foreach (var fmodEvent in eventArray)
            {
                // Verify the event exists before adding it to the sound dictionary
                if (FMODCall(fmodEvent.getPath(out var eventPath)))
                {
                    var newEvent = new EventDescription(fmodEvent);
                    eventDictionary.Add(eventPath, newEvent);

                    Logger.Engine(eventPath.Contains("snapshot:/")
                        ? $"Snapshot loaded, FMOD Path: {eventPath}"
                        : $"Sound loaded, FMOD Path: {eventPath}");
                }
            }
        }
        else 
        {
            Logger.Warn($"Unable to load bank file! Bank path: {bankPath}");
        }
    }

    /// <summary>
    /// Play an FMOD event
    /// </summary>
    /// <param name="eventName">Name of the event.</param>
    /// <param name="level">The <see cref="Level"/> to require active to play this event. If null, will always play.</param>
    public static EventInstance? PlayEvent(string eventName, float x, float y, float z, Level? level, string eventPrefix = "event:/", bool dontStart = false)
    {
        ThrowIfUninitialized();
        
        // Cancel event if our target level exists and is NOT active.
        if (level is not null && !level.Active)
            return null;
        
        EventInstance? newInstance = null;
        var eventPath = eventPrefix + eventName;
        
        Logger.Engine("FMOD Event played: " + eventName);

        // Check to see if the event exists
        if (eventDictionary.TryGetValue(eventPath, out var value)) 
        {
            newInstance = value.CreateInstance(eventName);
            newInstance.Set3DPosition(x, y, z);
            if (!dontStart)
            {
                newInstance.Start();
                playingEvents.Add(newInstance);
            }
        }
        else 
        {
            // Log an error if it doesn't
            Logger.Warn($"FMOD Error: Event {eventName} (full path: {eventPath}) doesn't exist! Check the spelling/path or update the bank files.");
        }

        return newInstance;
    }
    
    public static EventInstance? PlaySnapshot(string snapshotName, float x, float y, float z, Level? level, bool dontStart = false) {
        return PlayEvent(snapshotName, x, y, z, level, "snapshot:/", dontStart);
    }
    
    public static bool IsPlaying(string eventName) {
        ThrowIfUninitialized();
        
        foreach (var e in playingEvents) {
            if (e.GetPath() == eventName)
                return true;
        }
            
        return false;
    }
    
    public static void StopSnapshot(string snapshotName) {
        ThrowIfUninitialized();
        
        StopEvent(snapshotName, true, "snapshot:/");
    }

    public static void StopEvent(string eventName, bool executeOnStop = true, string eventPrefix = "event:/")
    {
        ThrowIfUninitialized();
        
        // Since this is a brute-force cutoff anyway, we're not going to scan for an input room and just cancel everything instead.
        
        var eventPath = eventPrefix + eventName;

        // Check to see if the event exists
        if (eventDictionary.ContainsKey(eventPath)) 
        {
            foreach (var e in playingEvents) 
            {
                if (e.GetPath() == eventName) 
                {
                    if (executeOnStop)
                        e.Stop();
                    else
                        e.HardStop();
                }
            }
            
        } 
        else 
        {
            // Log an error if it doesn't
            Logger.Warn($"FMOD Error: Event {eventName} (full path: {eventPath}) doesn't exist! Check the spelling/path or update the bank files.");
        }
    }

    /// <summary>
    /// Sets a global fmod parameter
    /// </summary>
    /// <param name="parameterName">Name of the parameter</param>
    /// <param name="value">Value to set it to</param>
    public static void SetParameterGlobal(string parameterName, float value)
    {
        ThrowIfUninitialized();
        
        var r = studio.setParameterByName(parameterName, value);
        if (r == RESULT.ERR_EVENT_NOTFOUND) {
            Logger.Warn("Unable to set global parameter: ﬁ" + parameterName + "! Is the parameter local? Does it exist?");
        }
        if (!FMODCall(r)) {
            Logger.Warn($"{r}");
        }
    }

    /// <summary>
    /// Update FMOD callbacks. Should ALWAYS be called.
    /// </summary>
    internal static void Update()
    {
        if (!initialized)
            return;
        
        studio.update();

        playingEvents.RemoveAll(item =>
        {
            var stopped = item.GetStopped();

            if (stopped)
                item.DoOnStop(false);

            return stopped;
        });
    }

    public static void SetListenerPosition(int index, int x, int y, int z)
    {
        studio.getListenerAttributes(index, out var attribs);
        studio.setListenerAttributes(index, attribs with
        {
            position = new VECTOR { x = x, y = y, z = z }
        });
    }
    
    public static void StopAll() {
        ThrowIfUninitialized();
        
        foreach (var e in playingEvents) {
            e.HardStop();
        }
    }
}
