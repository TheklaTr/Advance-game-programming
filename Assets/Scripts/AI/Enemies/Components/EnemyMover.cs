using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMover : MonoBehaviour
{
	protected BasicAi AI;

	internal NavMeshAgent _navMeshAgent;

	protected Transform _target;
	
	protected Vector3 originalPos;


	protected Transform PlayerTransform
	{
		get { return this.AI.PlayerTransform; }
	}
	
	protected PlayerEntity PlayerEntity
	{
		get { return this.AI.PlayerEntity; }
	}

	protected virtual float RotationSpeed
	{
		get { return 2; }
	}

	internal void SetTarget(Transform newTarget)
	{
		this._target = newTarget;
	}

	protected virtual void GoBackToSpawn()
	{
		if (this.AI)
		{
			SetTarget(this.AI.MySpawner.transform);
		}
	}

	protected virtual void Start()
	{
		GetComponent<BasicAi>().onAiReset += Initialize;
		Initialize();
	}

	protected virtual void Initialize()
	{
		if (this.AI == null)
		{
			this.AI = GetComponent<BasicAi>();
		}

		if (this._navMeshAgent == null)
		{
			this._navMeshAgent = GetComponent<NavMeshAgent>();
		}

		StartMoving();
	}

	protected virtual void Update()
	{
        if (this._navMeshAgent && this._navMeshAgent.enabled)
		{
			SetDestination();
		}

	}

	protected virtual void SetDestination()
	{
		if (this._target == null)
		{
			return;
		}

		DetermineMovePosition();
	}


	protected virtual void DetermineMovePosition()
	{
		SetMovePosition(this._target.position);
	}

	protected void SetMovePosition(Vector3 position)
	{
		if (this._navMeshAgent != null)
		{
			this._navMeshAgent.SetDestination(position);
		}
	}

	public virtual void Stop()
	{
		if(LogicUtils.NullCheck(this._navMeshAgent,this) == false)
		{
			return;
		}

		this._navMeshAgent.enabled = false;
	}

	public virtual void StartMoving()
	{
		if(LogicUtils.NullCheck(this._navMeshAgent,this) == false)
		{
			return;
		}
		
		this._navMeshAgent.enabled = true;
	}

	
	private void OnDestroy()
	{
		if (this.AI)
		{
			this.AI.onAiReset -= Initialize;
		}
	}

	internal void RotateToTarget()
	{
		Vector3 targetDir = this._target.position - transform.position;

		Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir,this.RotationSpeed * Time.deltaTime, 0.0f);

		transform.rotation = Quaternion.LookRotation(newDir);
	}
	
	internal void RotateTo(Vector3 target,float time)
	{
		Vector3 targetDir = target - transform.position;

		StartCoroutine(RotateToDir(targetDir, time));
	}

	private IEnumerator RotateToDir(Vector3 newDir, float time)
	{
		float currentTime = 0;
		Quaternion startRotation = transform.rotation;
		while (currentTime < time)
		{
			currentTime += Time.deltaTime;
			transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(newDir, Vector3.up), currentTime / time);
			yield return null;
		}
	}
}

