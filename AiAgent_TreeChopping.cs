using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AnimationEnum
{
    AttachLogToHand,
    DropLog
}

public delegate void AiAnimationEvent(AnimationEnum ae);

public class AiAgent_TreeChopping : MonoBehaviour
{
    public event AiAnimationEvent OnAnimationEvent;
    public AiStateMachine stateMachine;
    public AiStateId initialState;
    public NavMeshAgent navMeshAgent;
    public Animator anim;
    public AiMovement aiMovement;
    public GameObject equippedItem;
    public Transform leftHand;
    public Transform rightHand;
    public Transform slot1;
    public CharacterController controller;
    public AiStateId _currentState;
    public List<GameObject> tempInvalidTargets = new List<GameObject>();
    public GameObject target;

    public GameObject AssignTarget
    {
        set
        {
            aiMovement.targetObject = value;
            aiMovement.target = value?.transform;
            aiMovement.SetDestination();
        }
    }

    public GameObject GetTargetObject
    {
        get { return aiMovement.targetObject ?? null; }
    }

    public Transform GetTargetPosition
    {
        get { return aiMovement.target ?? null; }
    }
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        stateMachine = new AiStateMachine(this);

        GameObject tcsGO = new GameObject("TreeChopping_State");
        TreeChopping_State tcs = tcsGO.AddComponent<TreeChopping_State>();

        // Register all class states here!!
        stateMachine.RegisterState(new Idle_State());
        stateMachine.RegisterState(tcs);
        stateMachine.RegisterState(new RetrieveLog_State());
        stateMachine.RegisterState(new LogDropoff_State());

        stateMachine.ChangeState(initialState);

        navMeshAgent.updatePosition = false;

        aiMovement = new AiMovement();
        aiMovement.agent = navMeshAgent;
        aiMovement.anim = anim;
        aiMovement.stateMachine = stateMachine;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
        _currentState = stateMachine.currentState;

        tempInvalidTargets = aiMovement.invalidTargets;
        if (aiMovement.target != null)
            target = aiMovement.target.gameObject;
    }

    public void AttachAxeToHand()
    {
        if (equippedItem == null) return;
        equippedItem.transform.SetParent(rightHand);
        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void AttachAxeToSlot()
    {
        if (equippedItem == null) return;
        equippedItem.transform.SetParent(slot1);
        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void AttachLogToHand()
    {
        //OnAnimationEvent?.Invoke(AnimationEnum.AttachLogToHand);
        var state = stateMachine.GetState(AiStateId.RetrieveLog_State) as RetrieveLog_State;
        if (state._currentLog == null) return;

        MeshCollider collider = state._currentLog.GetComponentInParent<MeshCollider>();
        if (collider != null)
            collider.enabled = false;

        Rigidbody rigidbody = state._currentLog.GetComponentInParent<Rigidbody>();
        if (rigidbody != null)
            rigidbody.isKinematic = true;

        state._currentLog.transform.parent.SetParent(this.rightHand);
        SlotLocation loc = state._currentLog.GetComponentInParent<SlotLocation>();
        if (loc.localPosition == Vector3.zero && loc.localRotation == Quaternion.Euler(0, 0, 0)) return;
        state._currentLog.transform.parent.localPosition = loc.localPosition;
        state._currentLog.transform.parent.localRotation = loc.localRotation;
        state._currentLog.transform.parent.localScale = new Vector3(1f, 1f, 1f);

        var navMeshObstacle = state._currentLog.GetComponentInParent<NavMeshObstacle>();
        if (navMeshObstacle != null)
            navMeshObstacle.enabled = false;

        return;
    }

    public void DropLog()
    {
        //OnAnimationEvent?.Invoke(AnimationEnum.DropLog);
        var state = stateMachine.GetState(AiStateId.RetrieveLog_State) as RetrieveLog_State;
        Destroy(state._currentLog.transform.parent.gameObject);
    }

    public void SetLayerWeight(string arg)
    {
        var split = arg.Split('_');
        int index = -1;
        float amt = -1;
        int.TryParse(split[0], out index);
        float.TryParse(split[1], out amt);
        if (index != -1 && amt != -1)
            anim.SetLayerWeight(index, amt);
    }

    public void SwitchLayers(string arg)
    {
        var split = arg.Split('_');
        int index = -1;
        float amt = -1;
        int index2 = -1;
        float amt2 = -1;
        int.TryParse(split[0], out index);
        float.TryParse(split[1], out amt);
        int.TryParse(split[2], out index2);
        float.TryParse(split[3], out amt2);
        if (index != -1 && amt != -1 && index2 != -1 && amt2 != -1)
        {
            anim.SetLayerWeight(index, amt);
            anim.SetLayerWeight(index2, amt2);
        }
    }

    public void ToggleController(string status)
    {
        bool toggle;
        bool.TryParse(status, out toggle);
        controller.enabled = toggle;
    }

    public void SwitchStates(string newState)
    {
        AiStateId state;
        if (!Enum.TryParse(newState, out state)) return;

        stateMachine.ChangeState(state);
    }

    private void OnDestroy()
    {
        stateMachine.GetState(stateMachine.currentState)?.Exit(this);
    }

    private void OnDrawGizmos()
    {
        if (navMeshAgent == null) return;
        if (!navMeshAgent.hasPath) return;
        if (aiMovement.currentWayPoint == null) return;
        Vector3 prev = navMeshAgent.transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(prev, aiMovement.currentWayPoint);
        foreach (var waypoint in aiMovement.wayPoints)
        {
            Gizmos.DrawSphere(waypoint, .1f);
            prev = waypoint;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(aiMovement.currentWayPoint, .3f);
    }
}