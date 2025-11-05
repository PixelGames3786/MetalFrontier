using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScareCrowControl : UnitBase, IDamageable, ITargetable
{
    private MeshRenderer[] allMeshes;

    // Start is called before the first frame update
    void Start()
    {
        //‘SƒƒbƒVƒ…‚ğæ“¾
        allMeshes = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void UnitActivate()
    {
        
    }

    public override void UnitDeactivate()
    {
        
    }

    public override void UnitSetUp()
    {
        
    }

    public bool CanHit()
    {
        return true;
    }

    public void Damage(AttackData attack)
    {
        
    }

    public bool CanTarget()
    {
        return true;
    }

    public bool IsVisible()
    {
        foreach (MeshRenderer renderer in allMeshes)
        {
            if (renderer.gameObject.layer == LayerMask.NameToLayer("MiniMap")) continue;

            if (renderer.isVisible)
            {
                return true;
            }
        }

        return false;
    }
}
