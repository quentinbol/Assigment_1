using UnityEngine;
using System;
using System.Collections.Generic;

public class SoldierStateMachine : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebug = true;
    
    private SoldierAgent soldier;
    private SoldierState currentState;
    private SoldierState previousState;
    private Dictionary<Type, SoldierState> stateCache = new Dictionary<Type, SoldierState>();

    public string CurrentStateName => currentState?.GetStateName() ?? "None";
    public string PreviousStateName => previousState?.GetStateName() ?? "None";
    public float TimeInCurrentState => currentState?.GetTimeInState() ?? 0f;
    
    void Awake()
    {
        soldier = GetComponent<SoldierAgent>();
    }
    
    void Start()
    {
        TransitionTo<IdleState>();
    }
    
    void Update()
    {
        currentState?.Execute();

        MovementController movement = GetComponent<MovementController>();
        if (movement != null)
        {
            movement.UpdateMovement();
        }
    }

    public void TransitionTo<T>() where T : SoldierState
    {
        Type stateType = typeof(T);

        if (!stateCache.ContainsKey(stateType))
        {
            try
            {
                stateCache[stateType] = (SoldierState)Activator.CreateInstance(stateType, new object[] { soldier });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{gameObject.name}] ERREUR création de {stateType.Name}: {e.Message}");
                return;
            }
        }
        
        TransitionTo(stateCache[stateType]);
    }

    public void TransitionTo(SoldierState newState)
    {
        if (currentState != null && currentState.GetType() == newState.GetType())
        {
            return;
        }
        if (currentState != null)
        {
            currentState.OnExit();
            previousState = currentState;
        }
        currentState = newState;
        currentState.OnEnter();
    }

    public void RevertToPreviousState()
    {
        if (previousState != null)
        {
            TransitionTo(previousState);
        }
    }

    public bool IsInState<T>() where T : SoldierState
    {
        return currentState != null && currentState.GetType() == typeof(T);
    }
    

    public SoldierState GetCurrentState()
    {
        return currentState;
    }

    public void ForceStop()
    {
        TransitionTo<IdleState>();
    }
}