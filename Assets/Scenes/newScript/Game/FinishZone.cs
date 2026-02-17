using UnityEngine;
using System.Collections.Generic;

public class FinishZone : MonoBehaviour
{
    public float destroyDelay = 0.5f;

    [SerializeField] private int soldiersArrived = 0;
    [SerializeField] private List<GameObject> arrivedSoldiersList = new List<GameObject>();

    public bool showDetailedLogs = true;

    public int SoldiersArrived => soldiersArrived;
    
    void OnTriggerEnter(Collider other)
    {

        SoldierAgent soldier = other.GetComponent<SoldierAgent>();
        
        if (soldier == null)
        {
            soldier = other.GetComponentInParent<SoldierAgent>();
        }
        
        if (soldier == null)
        {
            return;
        }
        if (arrivedSoldiersList.Contains(other.gameObject))
        {
            return;
        }
        ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
        if (timer != null && timer.IsDead())
        {
            return;
        }
        SaveSoldier(soldier, other.gameObject);
    }
    
    void SaveSoldier(SoldierAgent soldier, GameObject soldierObj)
    {
        arrivedSoldiersList.Add(soldierObj);
        soldiersArrived++;
        ExposureTimer timer = soldier.GetComponent<ExposureTimer>();
        if (timer != null)
        {
            timer.enabled = false;
        }
        MovementController movement = soldier.GetComponent<MovementController>();
        if (movement != null)
        {
            movement.Stop();
            movement.enabled = false;
        }
        SoldierStateMachine stateMachine = soldier.GetComponent<SoldierStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.enabled = false;
        }

        if (destroyDelay > 0f)
        {
            StartCoroutine(DestroyAfterDelay(soldierObj, destroyDelay));
        }
        else
        {
            Destroy(soldierObj);
        }
    }
    
    System.Collections.IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            Color originalColor = mat.color;
            
            float elapsed = 0f;
            while (elapsed < delay)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / delay);
                Color newColor = originalColor;
                newColor.a = alpha;
                mat.color = newColor;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }
        if (obj != null)
        {
            Destroy(obj);
        }
    }
    public List<GameObject> GetArrivedSoldiers()
    {
        return new List<GameObject>(arrivedSoldiersList);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}