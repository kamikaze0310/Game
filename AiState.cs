using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AiStateId
{
    Idle,
    TreeChopping_State,
    RetrieveLog_State,
    LogDropoff_State,
    InvalidTargets_State
       
}

public interface AiState
{
    AiStateId GetId();
    void Enter(AiAgent_TreeChopping agent);
    void UpdateAgent(AiAgent_TreeChopping agent);
    void Exit(AiAgent_TreeChopping agent);
}
