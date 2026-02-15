using UnityEngine;

/// <summary>
/// État d'inactivité - le soldat ne bouge pas
/// </summary>
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
        
        // Ne rien faire - le soldat reste immobile
        // (Le mouvement est déjà arrêté dans OnEnter)
    }
}