using System;
using System.Collections.Generic;

namespace AM2E;

/// <summary>
/// An alarm class that will execute a custom function once its timer has reached zero, with possibilities for looping and a different restart timer.
/// </summary>
/// <remarks>
/// This is an alarm class, that allows one to manually specify when a custom function will be executed.
/// In contrast to other alarms, <see cref="Alarm"/>s timer will only decrease by manually calling <see cref="Run(Alarm)"/>.
/// This allows for greater flexibility and is useful in cases where you don't want the alarm's timer to count down in certain situations. <br/>
/// <example>
/// Here is an example usage:
/// <code>
/// Alarm sixAlarm = new Alarm(6, () => { Console.WriteLine("You rolled 6 six times! How lucky!" };
/// while (true)
/// {
///     int rolledNumber = RollDice(); // some user implemented function that imitates a dice roll
///     if (rolledNumber == 6) Alarm.Run(sixAlarm)
/// }
/// </code>
/// This example will print <c>You rolled 6 six times! How lucky!</c> after the user rolled a 6 six times.
/// </example>
/// </remarks>>
public sealed class Alarm
{
    /// <summary>
    /// The time, in <see cref="Run(Alarm)"/> calls, before the callback should run.
    /// </summary>
    public int Time { get; private set; }
        
    /// <summary>
    /// The callback function that gets executed when either <see cref="Time"/>, or <see cref="RestartTime"/> depending on the circumstances, hit 0.
    /// </summary>
    private readonly Action callback;

    /// <summary>
    /// Whether the Alarm should loop after <see cref="Time"/> has reached 0. <br/>
    /// If this is set to <see langword="true"/>, then the Alarm will use <see cref="RestartTime"/> for each loop.
    /// </summary>
    public bool Loop { get; set; }
        
    /// <summary>
    /// The value the timer should be reset to when calling <see cref="Restart()"/> or looping.
    /// </summary>
    public int RestartTime { get; set; }
        
        
    /// <summary>
    /// Instantiates a new <see cref="Alarm"/>.
    /// </summary>
    /// <param name="time">The time, in <see cref="Run(Alarm)"/> calls, before the <paramref name="callback"/> should run.</param>
    /// <param name="callback">The callback to run when the timer hits 0.</param>
    /// <param name="loop">Whether or not the <see cref="Alarm"/> should loop upon hitting 0.</param>
    /// <param name="restartTime">The value the timer should be reset to when calling <see cref="Restart()"/> or looping. If left as null, this will default to <paramref name="time"/>.</param>
    public Alarm(int time, Action callback, bool loop = false, int? restartTime = null)
    {
        this.Time = time;
        this.callback = callback;
        this.Loop = loop;
        this.RestartTime = restartTime ?? time;
    }

    /// <summary>
    /// Restarts the timer.
    /// <para>
    /// This will restart <see cref="Time"/> to <see cref="RestartTime"/>. 
    /// </para>
    /// </summary>
    public void Restart()
    {
        Time = RestartTime;
    }

    /// <summary>
    /// Stops the timer completely, even when <see cref="Loop"/> is <see langword="true"/>. <br/>
    /// To make the stopped Alarm run again, call <see cref="Restart()"/>;
    /// </summary>
    public void Stop()
    {
        Time = -1;
    }

    /// <summary>
    /// Safely runs the given <see cref="Alarm"/>.
    /// Should <paramref name="alarm"/> be <see langword="null"/>, then this will do nothing.
    /// </summary>
    /// <param name="alarm">The <see cref="Alarm"/> to run.</param>
    public static void Run(Alarm alarm)
    {
        alarm?.Run();
    }

    /// <summary>
    /// Safely runs the given enumerable of <see cref="Alarm"/>s.
    /// Should any entry within <paramref name="alarms"/> be <see langword="null"/>, then this will do nothing for that entry.
    /// </summary>
    /// <param name="alarms">The enumerable of <see cref="Alarm"/>s to run.</param>
    public static void Run(IEnumerable<Alarm> alarms)
    {
        foreach (var alarm in alarms)
            Run(alarm);
    }

    /// <summary>
    /// Runs this Alarm.
    /// </summary>
    public void Run()
    {
        if (Time > -1)
            Time -= 1;

        if (Time != 0)
            return;

        Time = Loop ? RestartTime : -1;

        callback();
    }
}