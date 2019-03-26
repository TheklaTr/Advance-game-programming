using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public MeshRenderer bulletRenderer;
    public Collider bulletCollider;
    public Rigidbody bulletRigidBody;

    public float bulletHitRemovalTime = 3f;

    internal WeaponBase weaponShotFrom;

    private bool hitAlready = false;

    private bool _isPlayerWeapon;

    internal Vector3 shootLocation;

    private float bulletLifeTime = 20;

    internal virtual void InitalizeBullet(WeaponBase weaponShooting)
    {
        this.weaponShotFrom = weaponShooting;
        this.shootLocation = weaponShotFrom.transform.position;

        hitAlready = false;

        if (this.weaponShotFrom && bulletLifeTime > 0)
        {
            Invoke("GetRidOfBullet", bulletLifeTime);
        }

        if (this.bulletRenderer)
        {
            this.bulletRenderer.enabled = true;
        }

        if (this.bulletCollider)
        {
            this.bulletCollider.enabled = true;
        }

        if (this.bulletRigidBody)
        {
            this.bulletRigidBody.isKinematic = false;
        }

        this._isPlayerWeapon = this.weaponShotFrom.shooter.CompareTag("Player");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (LogicUtils.IsShootable(other) == false || LogicUtils.CanBeHitByBullet(other,_isPlayerWeapon) == false)
        {
            return;
        }
        this.BulletHitSomething(other, shootLocation);
    }

    protected virtual void BulletHitSomething(Collider other, Vector3 bulletForward)
    {
        if (hitAlready)
        { 
            return;
        }

        BulletBase bulletBase = other.GetComponent<BulletBase>();
        if (bulletBase != null && bulletBase.weaponShotFrom == this.weaponShotFrom)
        {
            return;
        }

        if (this.weaponShotFrom)
        {
            this.weaponShotFrom.BulletHit(other, bulletForward);
        }

        ShutOffBulletComponents();
      
        hitAlready = true;
    }

    protected virtual void ShutOffBulletComponents()
    {
        if (this.bulletRenderer)
        {
            this.bulletRenderer.enabled = false;
        }

        if (this.bulletCollider)
        {
            this.bulletCollider.enabled = false;
        }

        if (this.bulletRigidBody)
        {
            this.bulletRigidBody.isKinematic = true;
        }

        CancelInvoke();
        Invoke("GetRidOfBullet", this.bulletHitRemovalTime);
    }

    protected void GetRidOfBullet()
    {
        if (gameObject.activeInHierarchy)
        {
            PoolManager.ReturnObjectToPoolOrDestroyIt(gameObject);
        }
    }

}
