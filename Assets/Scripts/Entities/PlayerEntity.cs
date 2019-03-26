using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : BaseEntity
{
    protected override void Start()
    {
        base.Start();
        ReferenceManager.Instance.AddPlayer(this);
        ReferenceManager.Instance.PlayerTransform = transform;
    }
}
