using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface IKillable
{
    public void ApplyDamage(BulletData bulletData);
    public event EventHandler OnDeath;
    public event EventHandler<BulletDataEventArgs> OnHealthLoss;
}
