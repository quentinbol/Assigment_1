using UnityEngine;

/// <summary>
/// État "en cover" - le soldat est immobile derrière un cover
/// </summary>
public class InCoverState : SoldierState
{
    private CoverObject currentCover;
    
    public InCoverState(SoldierAgent soldier) : base(soldier) { }
    
    public override void OnEnter()
    {
        base.OnEnter();
        
        // Arrêter le mouvement
        movement.Stop();
        
        // Récupérer le cover actuel
        currentCover = soldier.CurrentCover;
        
        // Marquer le cover comme occupé
        if (currentCover != null && !currentCover.isOccupied)
        {
            currentCover.SetOccupied(soldier);
        }
    }
    
    public override void Execute()
    {
        base.Execute();
        
        // Le soldat reste immobile
        // Optionnel : ajouter ici des micro-mouvements, animations, etc.
    }
    
    public override void OnExit()
    {
        base.OnExit();
        
        // Libérer le cover
        if (currentCover != null && currentCover.isOccupied)
        {
            currentCover.SetFree();
        }
    }
}