using System;
using System.Collections.Generic;

namespace AM2E;

public class StateMachine <TKey, TState> where TState : State
{
    /// <summary>
    /// A key-state dictionary, which lists all possible states of the state machine. 
    /// </summary>
    private readonly Dictionary<TKey, TState> states;
        
    /// <summary>
    /// The current <see cref="TState"/> the state machine is currently in.
    /// </summary>
    public TState CurrentState => states[CurrentKey];

    /// <summary>
    /// The <see cref="TKey"/> of the current <see cref="TState"/> the state machine is currently in.
    /// </summary>
    public TKey CurrentKey { get; private set; }

    /// <summary>
    /// The <see cref="TKey"/> of the last <see cref="TState"/> the state machine was in.
    /// </summary>
    public TKey PreviousKey { get; private set; }

    /// <summary>
    /// How many times <see cref="Step()"/> has been executed since last calling <see cref="Change(TKey, int)"/>.
    /// </summary>
    public int StateTime { get; set; } = 0;
    // TODO:
    private bool changed = false;

    /// <summary>
    /// Initializes a new empty <see cref="StateMachine{TKey,TState}"/>. Enter event must be called manually for first state if instantiated this way!
    /// </summary>
    /// <param name="initialState">The key of the <see cref="TState"/> within which this <see cref="StateMachine{TKey,TState}"/> should start.</param>
    public StateMachine(TKey initialState) : this(initialState, null)
    {
        CurrentKey = initialState;
        PreviousKey = initialState;
    }
        
    /// <summary>
    /// Initializes a new <see cref="StateMachine{TKey,TState}"/> from the given <see cref="Dictionary{TKey,TState}"/>.
    /// </summary>
    /// <param name="initialState">The key of the <see cref="TState"/> within which this <see cref="StateMachine{TKey,TState}"/> should start.</param>
    /// <param name="states">The <see cref="Dictionary{TKey,TState}"/> this <see cref="StateMachine{TKey,TState}"/> should utilize.</param>
    public StateMachine(TKey initialState, Dictionary<TKey, TState> states)
    {
        this.states = states ?? new Dictionary<TKey, TState>();
    }

    /// <summary>
    /// Executes the <see cref="State.Step"/> event of <see cref="CurrentState"/> while tracking the current <see cref="StateTime"/>.
    /// </summary>
    /// <exception cref="KeyNotFoundException">If <see cref="CurrentKey"/> is null, due to the state not being added before via <see cref="Add(TKey, TState)"/>.</exception>
    public void Step()
    {
        changed = false;
        CurrentState.Step();

        // Don't increment StateTime if we just changed states in the Step above, as we'll never have StateTime 0 if that's the case.
        if (!changed) StateTime++;
    }
        
    /// <summary>
    /// Adds the given <paramref name="state"/> to this <see cref="StateMachine{TKey, TState}"/> with the given <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The <see cref="TKey"/> to which this <see cref="TState"/> should be assigned.</param>
    /// <param name="state">The <see cref="TState"/> that should be assigned to this <see cref="TKey"/>.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Add(TKey key, TState state)
    {
        states[key] = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>
    /// Changes the <see cref="CurrentState"/> based on the input <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The <see cref="TKey"/> of the desired <see cref="TState"/>.</param>
    /// <param name="stateTime">Override for the starting <see cref="StateTime"/> of this <see cref="TState"/>.</param>
    public void Change(TKey key, int stateTime = 0)
    {
        CurrentState.Leave();

        StateTime = stateTime;

        PreviousKey = CurrentKey;
        CurrentKey = key;
        changed = true;
            
        CurrentState.Enter();
    }
}