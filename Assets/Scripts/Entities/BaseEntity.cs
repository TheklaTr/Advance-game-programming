using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    public EntityEvent onEntityDeath;
    public EntityDamageTaken onEntityDamageTaken;
    
    public Action<float> onChangeHealth;
    
    public delegate void EntityEvent();
    public delegate void EntityDamageTaken(BaseEntity damageSource, float damageDealt);

    [Header("Basic stats")]
    public float currentHealth = 100;
    public float maxHealth = 100;
    public bool dead = false;
    internal bool _isInvulnerable = false;

    [Header("Weapon related things")]
    public WeaponBase currentWeapon;
    
    #region Initialization and unity base functions

    protected virtual void Start()
    {
        EntitySetup();
    }

    
    protected virtual void EntitySetup()
    {
        this.currentHealth = this.maxHealth;
        this.dead = false;
    }

   
  #endregion


    #region Modifying values

    /// <summary>
    /// Use this to modify the health instead of modifying it directly.
    /// </summary>
    /// <param name="changeAmount">+ or - change to health.</param>
    /// <param name="changeReason">what object caused the change</param>
    internal virtual void ChangeHealth<T>(float changeAmount, T changeReason)
    {
        if (this.dead)
        {
            return;
        }

        if (changeAmount < 0 && this._isInvulnerable)
        {
            return;
        }

        if (changeAmount < 0)
        {

            if (onEntityDamageTaken != null)
            {
                if (changeReason.GetType() == typeof(BaseEntity))
                {
                    BaseEntity dmgSource = (BaseEntity)(object)changeReason;
                    this.onEntityDamageTaken(dmgSource, changeAmount);
                }
                else if (changeReason.GetType() == typeof(WeaponBase))
                {
                    WeaponBase dmgSource = (WeaponBase)(object)changeReason;
                    this.onEntityDamageTaken(dmgSource.OwnerEntity, changeAmount);
                }
            }

         
        }

        this.currentHealth += changeAmount;

        if (this.onChangeHealth != null)
        {
            this.onChangeHealth(this.currentHealth);
        }
        
        if (this.currentHealth > this.maxHealth)
        {
            this.currentHealth = this.maxHealth;
        }
        else if (this.currentHealth <= 0)
        {
            
            EntityDeath((object)changeReason);
        }
    }
   #endregion



    protected virtual void EntityDeath(object causeOfDeath = null)
    {
        this.dead = true;

        if (this.onEntityDeath != null)
        {
            this.onEntityDeath();
        }
    }

   
}
