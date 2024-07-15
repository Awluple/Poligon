using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Poligon.EvetArgs;

public interface IKillable
{
    public void ApplyDamage(BulletData bulletData);
    public event EventHandler<CharacterEventArgs> OnDeath;
    public event EventHandler<BulletDataEventArgs> OnHealthLoss;
}
