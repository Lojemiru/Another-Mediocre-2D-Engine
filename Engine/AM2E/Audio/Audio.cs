using System;
using System.Collections.Generic;
using System.IO;
using AM2E.IO;

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


    private const FMOD.INITFLAGS FMOD_INIT_FLAGS = FMOD.INITFLAGS.NORMAL;

    private static FMOD.Studio.System studio;
    private static Dictionary<string, EventDescription> eventDictionary;
    private static List<EventInstance> playingEvents;


    private static bool initialized = false;

    /// <summary>
    /// Initalizes the audio engine
    /// </summary>
    internal static void Init()
    {
        if (initialized)
            throw new Exception("AM2E Audio Engine has already been initialized!!!");

        initialized = true;
        
        // Create the fmod engine
        if (FMODCall(FMOD.Studio.System.create(out studio), "Create FMOD system... ")) 
        {

            // Start fmod with advanced settings (stops the -inf optimization!!!)
            var fmodAdvancedSettings = new FMOD.ADVANCEDSETTINGS
            {
                // Set the advanced settings
                vol0virtualvol = -1
            };

            // Get an instance of the system, and apply the settings
            FMODCall(studio.getCoreSystem(out var system), "Retrieving FMOD core system... ");
            FMODCall(system.setAdvancedSettings(ref fmodAdvancedSettings),"Applying advanced settings... ");

            if (FMODCall(studio.initialize(MAX_CHANNELS, FMOD_STUDIO_INIT_FLAGS, FMOD_INIT_FLAGS, (IntPtr)0), "Init FMOD system... ")) 
            {
                // Create a dictionary to store all the events
                eventDictionary = new Dictionary<string, EventDescription>();

                // Load the strings bank first
                LoadBank(AssetManager.GetAudioPath() + "/Master.strings.bank");

                // Load all the banks
                var bankArray = Directory.GetFiles(AssetManager.GetAudioPath());
                foreach (var file in bankArray) 
                {
                    LoadBank(file);
                }
            }
        }

        playingEvents = new List<EventInstance>();
    }

    /// <summary>
    /// Helper function which prints the FMOD_RESULT of a given FMOD function, as well as returning true if there were no errors
    /// Useful for debugging weird-ass fmod issues
    /// </summary>
    /// <param name="statement"></param>
    /// <param name="eventMessage"></param>
    public static bool FMODCall(FMOD.RESULT statement, string eventMessage = "")
    {
        var result = (statement == FMOD.RESULT.OK);
        if ((!string.IsNullOrWhiteSpace(eventMessage)) || (result != true)) 
        {
            Console.WriteLine(eventMessage + statement);
        }

        return result;
    }

    /// <summary>
    /// Loads an FMOD bank
    /// </summary>
    /// <param name="bankPath"> path of the bank to load</param>
    private static void LoadBank(string bankPath)
    {
        // Load the bank into memory
        if (FMODCall(studio.loadBankFile(bankPath, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out var bank), "Loading Bank... ")) 
        {
            // Load all the events from the bank into an array
            FMODCall(bank.getEventList(out var eventArray), "Getting Bank event list... ");

            // Load all the events from the name into a dictionary
            foreach (var fmodEvent in eventArray)
            {
                // Verify the event exists before adding it to the sound dictionary
                if (!FMODCall(fmodEvent.getPath(out var eventPath))) 
                    continue;
                
                var newEvent = new EventDescription(fmodEvent);
                eventDictionary.Add(eventPath, newEvent);
                    
                if (EngineCore.DEBUG) 
                {
                    Console.WriteLine("Sound loaded, FMOD Path: {0}", eventPath);
                }
            }
        }
        else 
        {
            Console.WriteLine("Unable to load bank file! Bank path: {0}", bankPath);
        }
    }

    /// <summary>
    /// Play an FMOD event
    /// </summary>
    /// <param name="eventName">Name of the event</param>
    public static EventInstance PlayEvent(string eventName)
    {
        EventInstance newInstance = null;
        var eventPath = "event:/" + eventName;
        
        //CDebug.Log("Event played: {0}", eventName);
        Console.WriteLine("Event played: {0}", eventName);

        // Check to see if the event exists
        if (eventDictionary.ContainsKey(eventPath)) 
        {
            newInstance = eventDictionary[eventPath].CreateInstance();
            newInstance.Start();
            playingEvents.Add(newInstance);
        }
        else 
        {
            // Log an error if it doesn't
            Console.WriteLine("FMOD Error: Event {0} (full path: {1}) doesn't exist! Check the spelling/path or update the bank files.", eventName, eventPath);
        }

        return newInstance;
    }

    public static void StopEvent(string eventName)
    {
        var eventPath = "event:/" + eventName;

        // Check to see if the event exists
        if (eventDictionary.ContainsKey(eventPath)) 
        {
            foreach (var e in playingEvents) 
            {
                if (e.GetPath() == eventName) 
                {
                    e.Stop();
                }
            }
            
        } 
        else 
        {
            // Log an error if it doesn't
            Console.WriteLine(
                "FMOD Error: Event {0} (full path: {1}) doesn't exist! Check the spelling/path or update the bank files.",
                eventName, eventPath);
        }
    }

    /// <summary>
    /// Sets a global fmod parameter
    /// </summary>
    /// <param name="parameterName">Name of the parameter</param>
    /// <param name="value">Value to set it to</param>
    public static void SetParameterGlobal(string parameterName, float value)
    {
        FMODCall(studio.setParameterByName(parameterName, value), "Param set: ");
    }

    /// <summary>
    /// Update FMOD callbacks. Should ALWAYS be called.
    /// </summary>
    public static void Update()
    {
        studio.update();

        playingEvents.RemoveAll(item => item.GetStopped());
    }
}
