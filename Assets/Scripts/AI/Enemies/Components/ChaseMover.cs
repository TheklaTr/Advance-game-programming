using System.Collections;
using UnityEngine;

public class ChaseMover : EnemyMover
{
	[SerializeField]private float _distanceLimit = 30;


	protected override void Initialize()
	{
		base.Initialize();

		SetTarget(PlayerTransform);
	}

	protected override void DetermineMovePosition()
	{
		if (this._target == PlayerTransform)
		{
			Vector3 movePosition = this._target.transform.position;
			Vector3 lookDirection = (transform.position - this._target.transform.position).normalized;

			float stoppingDistance = this._navMeshAgent.stoppingDistance;
			movePosition += lookDirection * (Random.Range(stoppingDistance * 1.2f, stoppingDistance*2));

			SetMovePosition(movePosition);
			RotateToTarget();
		}
		else
		{
			base.DetermineMovePosition();
		}
	}

	protected override void Update()
	{
		base.Update();
		CheckIfPlayerLost();
	}

	protected virtual void CheckIfPlayerLost()
	{
		if (PlayerTransform == null || gameObject.activeSelf == false)
		{
			return;
		}

		bool isChasing = this._target == PlayerTransform;
        float distance;

        if (isChasing == false)
		{
            distance = Vector3.Distance(AI.MySpawner.transform.position, transform.position);
            if (distance < _distanceLimit * 0.5f)
            {
                _target = null;
                AI.SwitchMover(typeof(RoamMover));
            }
            return;
		}
		
		distance = Vector3.Distance(PlayerTransform.position, transform.position);
		if (distance > this._distanceLimit)
		{
            GoBackToSpawn();
        }
	}
	
}
