using UnityEngine;

public abstract class SoldierState
{
    protected SoldierAgent soldier;
    protected MovementController movement;
    protected SteeringBehaviors steering;
    protected Transform transform;

    protected float timeInState = 0f;
    
    public SoldierState(SoldierAgent soldier)
    {
        this.soldier = soldier;
        this.movement = soldier.GetComponent<MovementController>();
        this.steering = soldier.GetComponent<SteeringBehaviors>();
        this.transform = soldier.transform;
    }

    public virtual void OnEnter()
    {
        timeInState = 0f;
        Debug.Log($"{soldier.name} entered {GetType().Name}");
    }

    public virtual void Execute()
    {
        timeInState += Time.deltaTime;
    }

    public virtual void OnExit()
    {
        //Debug.Log("soldier left");
    }

    public virtual string GetStateName()
    {
        return GetType().Name;
    }

    public float GetTimeInState()
    {
        return timeInState;
    }
}