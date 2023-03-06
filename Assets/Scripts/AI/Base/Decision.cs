using UnityEngine;
public abstract class Decision : ScriptableObject
{
    public abstract bool Decide(StateMachine controller);
    // gives a response to a state logic dilemma
}

