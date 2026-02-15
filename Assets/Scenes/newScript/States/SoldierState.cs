using UnityEngine;

/// <summary>
/// Classe de base abstraite pour tous les états d'un soldat
/// Chaque état implémente sa propre logique de mouvement et de comportement
/// </summary>
public abstract class SoldierState
{
    protected SoldierAgent soldier;
    protected MovementController movement;
    protected SteeringBehaviors steering;
    protected Transform transform;
    
    // Temps passé dans cet état
    protected float timeInState = 0f;
    
    public SoldierState(SoldierAgent soldier)
    {
        this.soldier = soldier;
        this.movement = soldier.GetComponent<MovementController>();
        this.steering = soldier.GetComponent<SteeringBehaviors>();
        this.transform = soldier.transform;
    }
    
    /// <summary>
    /// Appelé une fois quand on entre dans cet état
    /// </summary>
    public virtual void OnEnter()
    {
        timeInState = 0f;
        Debug.Log($"{soldier.name} entered {GetType().Name}");
    }
    
    /// <summary>
    /// Appelé chaque frame pendant que l'état est actif
    /// </summary>
    public virtual void Execute()
    {
        timeInState += Time.deltaTime;
    }
    
    /// <summary>
    /// Appelé une fois quand on sort de cet état
    /// </summary>
    public virtual void OnExit()
    {
        Debug.Log($"{soldier.name} exited {GetType().Name} after {timeInState:F2}s");
    }
    
    /// <summary>
    /// Nom de l'état pour le debug
    /// </summary>
    public virtual string GetStateName()
    {
        return GetType().Name;
    }
    
    /// <summary>
    /// Temps passé dans cet état
    /// </summary>
    public float GetTimeInState()
    {
        return timeInState;
    }
}