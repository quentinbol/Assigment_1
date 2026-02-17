using UnityEngine;

public class SquadColorFixer : MonoBehaviour
{
    public bool fixColorsNow = false;
    public bool createUniqueMaterials = true;
    
    void Update()
    {
        if (fixColorsNow)
        {
            fixColorsNow = false;
            FixAllSquadColors();
        }
    }

    public void FixAllSquadColors()
    {
        Squad[] allSquads = FindObjectsByType<Squad>(FindObjectsSortMode.None);
        foreach (Squad squad in allSquads)
        {
            if (squad == null || squad.soldiers == null)
                continue;
            foreach (SoldierAgent soldier in squad.soldiers)
            {
                if (soldier == null)
                    continue;
                
                FixSoldierColor(soldier, squad.squadColor);
            }
        }
        
        //Debug.Log("colors changed");
    }
    
    void FixSoldierColor(SoldierAgent soldier, Color squadColor)
    {
        Renderer renderer = soldier.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = soldier.GetComponentInChildren<Renderer>();
        }
        
        if (renderer == null)
        {
            //Debug.Log("Not working");
            return;
        }
        
        if (createUniqueMaterials)
        {
            Material newMaterial = new Material(renderer.sharedMaterial);
            newMaterial.color = squadColor;
            renderer.material = newMaterial;
        }
        else
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", squadColor);
            renderer.SetPropertyBlock(mpb);
        }
    }
}
