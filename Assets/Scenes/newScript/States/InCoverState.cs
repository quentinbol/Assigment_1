using UnityEngine;

/// <summary>
/// État "en cover" - attend jusqu'à ce qu'on lui ordonne de partir
/// Pas de timer automatique, c'est le SquadCoverCoordinator qui décide
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
        
        Debug.Log($"{soldier.name} : En cover (attend ordre de la squad)");
    }
    
    public override void Execute()
    {
        base.Execute();
        
        // Reste en cover jusqu'à ce qu'on lui ordonne de partir
        // Le SquadCoverCoordinator appellera JoinSquadMovement() quand c'est le moment
    }
    
    public override void OnExit()
    {
        base.OnExit();
        
        // Libérer le cover
        if (currentCover != null && currentCover.isOccupied)
        {
            currentCover.SetFree();
        }
        
        Debug.Log($"{soldier.name} : Quitte le cover");
    }
}