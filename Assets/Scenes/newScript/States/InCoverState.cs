using UnityEngine;

public class InCoverState : SoldierState
{
    private CoverObject currentCover;
    
    public InCoverState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        movement.Stop();

        currentCover = soldier.CurrentCover;

        if (currentCover != null && !currentCover.isOccupied)
        {
            currentCover.SetOccupied(soldier);
            soldier.StopAndIdle();
        }
        
        Debug.Log($"{soldier.name} enter cover");
    }
    
    public override void Execute()
    {
        base.Execute();
    }
    
    public override void OnExit()
    {
        base.OnExit();

        if (currentCover != null && currentCover.isOccupied)
        {
            currentCover.SetFree();
        }
        
        Debug.Log($"{soldier.name} leave cover");
    }
}