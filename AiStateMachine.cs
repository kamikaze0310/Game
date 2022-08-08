using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiStateMachine
{
    public AiState[] states;
    public AiAgent_TreeChopping agent;
    public AiStateId currentState;

    public AiStateMachine(AiAgent_TreeChopping agent)
    {
        this.agent = agent;
        int numStates = System.Enum.GetNames(typeof(AiStateId)).Length;
        states = new AiState[numStates];
    }

    public void RegisterState(AiState state)
    {
        int index = (int)state.GetId();
        states[index] = state;
    }

    public AiState GetState(AiStateId stateId)
    {
        int index = (int)stateId;
        return states[index];
    }

    public void Update()
    {
        GetState(currentState)?.UpdateAgent(agent);
    }

    public void ChangeState(AiStateId newState)
    {
        GetState(currentState)?.Exit(agent);
        currentState = newState;
        GetState(currentState)?.Enter(agent);
    }

}
