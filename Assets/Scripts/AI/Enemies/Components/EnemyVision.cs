using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
	private Transform _playerTransform;
	private PlayerEntity _playerEntity;

	protected BasicAi AI;
	
	public float visualDistance = 10;
	private WeaponBase _weapon;

	private bool _isLooking = true;
	public float normalVisibilityAngle = 30f;

	public Transform PlayerTransform
	{
		get
		{
			if (this._playerTransform == null)
			{
				if (ReferenceManager.Instance != null)
				{
					this._playerTransform = ReferenceManager.Instance.PlayerTransform;
				}
			}
			return this._playerTransform;
		}
	}

	
	public PlayerEntity PlayerEntity
	{
		get
		{
			if (this._playerEntity == null)
			{
				if (ReferenceManager.Instance != null)
				{
                    this._playerEntity = ReferenceManager.GetPlayer(0);
				}
			}
			return this._playerEntity;
		}
	}
	protected virtual void Start()
	{
		this.AI = GetComponent<BasicAi>();
		StartCoroutine(AssignWeapon());
	}

	private IEnumerator AssignWeapon()
	{
		if (this.AI != null)
		{
			yield return new WaitUntil(() => this.AI.currentWeapon != null);		
			this._weapon = this.AI.currentWeapon;
			this.AI.onEntityDeath += StopLooking;
			this.AI.onAiReset += StartLooking;
		}
	}

	protected virtual void Update()
	{
		if (PlayerEntity && PlayerEntity.dead)
		{
			return;
		}
		
		if (this._isLooking && CanSeePlayer(normalVisibilityAngle))
		{
			Attack();
		}
	}

	protected virtual void Attack()
	{
        if (this._weapon == null)
        {
            Debug.Log("weapon is null on enemy " + this.name);
            return;
        }

		this._weapon.Shoot();
	}

	internal bool CanSeePlayer(float searchAngle)
	{
		if (PlayerTransform == null)
		{
			return false;
		}

        if (Physics.Linecast(transform.position, this.PlayerTransform.position) == false)
        {
            return false;
        }

        if (Vector3.Distance(PlayerTransform.position, transform.position) < this.visualDistance/3)
		{
			this.AI.RotateToTarget();
			return true;
		}
		
		Vector3 targetDir = PlayerTransform.position - transform.position;
		Vector3 cameraDir = transform.forward;

		float angle = Vector3.Angle(targetDir, cameraDir);
		
		if (angle > searchAngle)
		{
			return false;
		}

		if (Vector3.Distance(this.PlayerTransform.position, transform.position) > this.visualDistance)
		{
			return false;
		}	

		return true;
	}


	private void StopLooking()
	{
		this._isLooking = false;
	}
	private void StartLooking()
	{
		this._isLooking = true;
	}
}
