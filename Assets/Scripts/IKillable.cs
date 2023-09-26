using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public interface IKillable
{
    public void ApplyDamage(float damage);
    public event EventHandler OnDeath;
}
