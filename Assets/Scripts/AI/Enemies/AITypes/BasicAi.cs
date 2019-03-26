using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BasicAi : BaseEntity
{
    private EnemySpawner _mySpawner;

    internal EnemySpawner MySpawner
    {
        get { return this._mySpawner; }
        set { this._mySpawner = value; }
    }

    private Rigidbody _rigidbody;

    internal Rigidbody Rigidbody
    {
        get
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            return _rigidbody;
        }
    }

    public Transform PlayerTransform
    {
        get
        {
            if (this._myVision != null)
            {
                return this._myVision.PlayerTransform;
            }
            else
            {
                return ReferenceManager.GetPlayerTransform();
            }
        }
    }

    public PlayerEntity PlayerEntity
    {
        get
        {
            if (this._myVision != null)
            {
                return this._myVision.PlayerEntity;
            }
            else
            {
                return ReferenceManager.GetPlayer();
            }
        }
    }


    public int weaponType;

    public BodyPart[] bodyParts;

    [SerializeField] private float timeBeforeDeath = 7;

    public EntityEvent onAiReset;

    protected EnemyMover _myMover;
    protected EnemyVision _myVision;

    
    private void Awake()
    {
        this.bodyParts = gameObject.GetComponentsInChildren<BodyPart>();

        foreach (BodyPart bodyPart in this.bodyParts)
        {
            bodyPart.parentEntity = this;
        }
    
    }
  
    private void OnEnable()
    {
        GetComponents();
        ResetComponents();
    }

    private void GetComponents()
    {
        this._myMover = GetComponent<EnemyMover>();
        this._myVision = GetComponent<EnemyVision>();
    }

    internal virtual void ResetComponents()
    {
        if (this._myVision)
        {
            this._myVision.enabled = true;
        }

        SwitchDeathTriggers(false);

        if (this._myMover && this._myMover.GetType() != MyMoverType())
        {
            SwitchMover(MyMoverType());
        }

        EntitySetup();

        if (this.onAiReset != null)
        {
            this.onAiReset();
        }

    }

    protected virtual Type MyMoverType()
    {
        return typeof(ChaseMover);
    }

    internal virtual void SwitchDeathTriggers(bool state)
    {
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider)
        {
            enemyCollider.isTrigger = state;
        }
    }

   
    protected override void EntityDeath(object causeOfDeath = null)
    {
        base.EntityDeath();

        if (this._myVision)
        {
            this._myVision.enabled = false;
        }

        if (this._myMover)
        {
            this._myMover.Stop();
        }
    }


    private void RemoveAiFromManagers()
    {
        if (ReferenceManager.Instance != null)
        {
            ReferenceManager.Instance.RemoveEnemyReference(this);
        }

        if (MySpawner != null)
        {
            MySpawner.RemoveDeadAi(this);
        }
    }

   
    public virtual EnemyMover SwitchMover(System.Type switchTo)
    {
        if (switchTo == null)
        {
            Debug.LogError("Switching to empty mover is nono");
            return null;
        }

        EnemyMover[] currentMovers = GetComponents<EnemyMover>();
        if (currentMovers == null)
        {
            Debug.LogError("Something went wrong while switching movers");
        }
        else
        {
            foreach (EnemyMover mover in currentMovers)
            {
                Destroy(mover);
            }
        }

        EnemyMover newMover = CreateMover(switchTo);
        this._myMover = newMover;

        return newMover;
    }


    private EnemyMover CreateMover(System.Type moverType)
    {
        if (moverType == null)
        {
            Debug.LogError("empty parameters in createmover");
            return null;
        }

        BasicAi AI = gameObject.GetComponent<BasicAi>();
        if (AI == null)
        {
            Debug.LogError("cannot attach movers to nonAi");
            return null;
        }

        EnemyMover mover = (EnemyMover) gameObject.AddComponent(moverType);
        
        return mover;
    }


    internal void StopMoving()
    {
        if (this._myMover != null)
        {
            this._myMover.Stop();
        }
    }


    internal virtual void StartMoving()
    {
        if (this._myMover != null)
        {
            this._myMover.StartMoving();
        }
    }

    public void SetMoveTarget(Transform newTransform)
    {
        if (this._myMover != null)
        {
            this._myMover.SetTarget(newTransform);
        }
    }

    public void Despawn()
    {
        if (gameObject.activeInHierarchy)
        {
            RemoveAiFromManagers();

            PoolManager.ReturnObjectToPoolOrDestroyIt(gameObject);
        }
        else
        {
            Debug.LogError("Trying to kill a dead guy " + gameObject.name);
        }
    }

   
    internal virtual void RotateToTarget()
    {
        this._myMover.RotateToTarget();
    }

    internal bool CanSeePlayer()
    {
        return this._myVision.CanSeePlayer(360);
    }
}