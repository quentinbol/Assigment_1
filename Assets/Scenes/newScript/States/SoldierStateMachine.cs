using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Gère les états d'un soldat et les transitions entre états
/// </summary>
public class SoldierStateMachine : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebug = true;
    
    private SoldierAgent soldier;
    private SoldierState currentState;
    private SoldierState previousState;
    private Dictionary<Type, SoldierState> stateCache = new Dictionary<Type, SoldierState>();
    
    // Propriétés publiques pour l'inspection
    public string CurrentStateName => currentState?.GetStateName() ?? "None";
    public string PreviousStateName => previousState?.GetStateName() ?? "None";
    public float TimeInCurrentState => currentState?.GetTimeInState() ?? 0f;
    
    void Awake()
    {
        soldier = GetComponent<SoldierAgent>();
    }
    
    void Start()
    {
        // État initial
        TransitionTo<IdleState>();
    }
    
    void Update()
    {
        // Exécuter l'état actuel
        currentState?.Execute();
        
        // Mettre à jour le mouvement après que l'état ait appliqué ses forces
        MovementController movement = GetComponent<MovementController>();
        if (movement != null)
        {
            movement.UpdateMovement();
        }
    }
    
    /// <summary>
    /// Transition vers un nouvel état (generic version)
    /// </summary>
    public void TransitionTo<T>() where T : SoldierState
    {
        Type stateType = typeof(T);
        
        Debug.Log($"[{gameObject.name}] TransitionTo<{stateType.Name}> appelé");
        
        // Récupérer ou créer l'instance de l'état
        if (!stateCache.ContainsKey(stateType))
        {
            Debug.Log($"[{gameObject.name}] Création de {stateType.Name}");
            
            // CORRECTION : Passer le SoldierAgent en paramètre
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
    
    /// <summary>
    /// Transition vers un nouvel état (instance version)
    /// </summary>
    public void TransitionTo(SoldierState newState)
    {
        // Ne rien faire si on est déjà dans cet état
        if (currentState != null && currentState.GetType() == newState.GetType())
        {
            return;
        }
        
        // Sortir de l'état actuel
        if (currentState != null)
        {
            currentState.OnExit();
            previousState = currentState;
        }
        
        // Entrer dans le nouvel état
        currentState = newState;
        currentState.OnEnter();
        
        if (showDebug)
        {
            Debug.Log($"[{soldier.name}] State: {PreviousStateName} → {CurrentStateName}");
        }
    }
    
    /// <summary>
    /// Retourner à l'état précédent
    /// </summary>
    public void RevertToPreviousState()
    {
        if (previousState != null)
        {
            TransitionTo(previousState);
        }
    }
    
    /// <summary>
    /// Vérifie si on est dans un état spécifique
    /// </summary>
    public bool IsInState<T>() where T : SoldierState
    {
        return currentState != null && currentState.GetType() == typeof(T);
    }
    
    /// <summary>
    /// Obtient l'état actuel
    /// </summary>
    public SoldierState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// Force l'arrêt complet (pour le debug)
    /// </summary>
    public void ForceStop()
    {
        TransitionTo<IdleState>();
    }
}