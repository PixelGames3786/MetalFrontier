using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    //UŒ‚‚ª‚·‚è”²‚¯‚é‚©‚Ç‚¤‚©
    public bool CanHit();

    public void Damage(AttackData attack);
}
