using UnityEngine;

public class IdleState : SoldierState
{
    public IdleState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        movement.Stop();
    }
    
    public override void Execute()
    {
        base.Execute();
        //delete tihs
    }
}