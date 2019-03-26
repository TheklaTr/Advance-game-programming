using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public GameObject shooter;
    public GameObject shootFrom;

    public string bulletPoolString;
    public GameObject bulletPrefab;

    private float _lastFiringTime;
    public float fireRatePerSecond = 1;
    protected float timeBetweenShots
    {
        get
        {
            return 1f / this.fireRatePerSecond;
        }
    }

    public BaseEntity OwnerEntity => this.shooter.GetComponent<BaseEntity>();

    public float damagePerBullet;
    public float shootForce = 5;
    internal bool Shoot()
    {
        if (Time.timeSinceLevelLoad - this._lastFiringTime < timeBetweenShots) // Cant fire yet, quit.
        {
            return false;
        }        

        CreateBullet();
        this._lastFiringTime = Time.timeSinceLevelLoad;
        return true;
    }

    protected virtual void CreateBullet()
    {
        if (PoolManager.Instance == null)
        {
            return;
        }

        PoolObjectSettings settings = new PoolObjectSettings()
        {
            timeBeforeReturningToPool = 5f,
            appearanceTime = 0.01f
        };



        PoolObjectSettings settingsToSet = new PoolObjectSettings();

        DetermineNewBulletSettings(settingsToSet);
        SetBulletSettings(settings, settingsToSet);

        GameObject bulletObject = null;

        if (!string.IsNullOrEmpty(bulletPoolString))
        {
            bulletObject = PoolManager.Instance.GetPooledObject(bulletPoolString, settings);
        }
        else if (bulletPrefab)
        {
            bulletObject = PoolManager.Instance.GetPooledObject(bulletPrefab, settings);
        }

        if (bulletObject)
        {
            BulletBase bulletBase = bulletObject.GetComponent<BulletBase>();
            if (bulletBase)
            {
                bulletBase.InitalizeBullet(this);
            }
        }
    }

    protected virtual void DetermineNewBulletSettings(PoolObjectSettings settingsToSet)
    {
        Vector3 forward = this.shooter.transform.forward;

        Vector3 forceToSet = new Vector3(forward.x * this.shootForce, 0, forward.z*this.shootForce);

        settingsToSet.setRigidBodyForce = true;
        settingsToSet.rigidbodyForceToSet = forceToSet;

        Vector3 shootPosition = this.shootFrom.transform.position;
        settingsToSet.positionToSet = shootPosition;
        
        Vector3 rotationToSet = this.shootFrom.transform.eulerAngles;
        rotationToSet.x = 0;
        rotationToSet.z = 0;
        
        settingsToSet.rotationToSet = Quaternion.Euler(rotationToSet);
    }
    
    private void SetBulletSettings(PoolObjectSettings settings, PoolObjectSettings newSettings)
    {
        settings.setRigidBodyForce = true;
        settings.rigidbodyForceToSet = newSettings.rigidbodyForceToSet;

        settings.positionToSet = newSettings.positionToSet;
        settings.rotationToSet = newSettings.rotationToSet;
    }

    internal void BulletHit(Collider hitObject, Vector3 bulletForward)
    {
        if (hitObject == null)
        {
            return;
        }

        BodyPart hitBodyPart = hitObject.GetComponent<BodyPart>();
        if (hitBodyPart != null && this.shooter != null)
        {
            bool isPlayerWeapon = this.shooter.CompareTag("Player");
            if (LogicUtils.CanBeHitByBullet(hitBodyPart, isPlayerWeapon))
            {
                hitBodyPart.TakeDamage(-this.damagePerBullet, this);
            }
        }
        else
        {
            BaseEntity hitEntity = hitObject.GetComponent<BaseEntity>();
            if (hitEntity)
            {
                bool isPlayerWeapon = this.shooter.CompareTag("Player");
                if (LogicUtils.CanBeHitByBullet(hitEntity, isPlayerWeapon))
                {
                    hitEntity.ChangeHealth(-this.damagePerBullet, this);
                }
            }
        }
    } 
    
}
