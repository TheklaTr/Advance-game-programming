using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    private static ReferenceManager instance;
    internal static ReferenceManager Instance
    {
        get
        {
            return instance;
        }
    }

    internal static int ENEMYLAYER = 12;
    internal static int RAGDOLLLAYER = 16;
    internal static int PLAYERLAYER = 9;

    [SerializeField]
    private LayerMask playerLayer;
    internal LayerMask PlayerLayer
    {
        get { return this.playerLayer; }
    }

    [SerializeField]
    private LayerMask enemyLayer;
    internal LayerMask EnemyLayer
    {
        get { return this.enemyLayer; }
    }

    private Transform playerTransform;
    internal Transform PlayerTransform
    {
        get { return this.playerTransform; }
        set { this.playerTransform = value; }
    }

    private PlayerController playerController;
    internal PlayerController PlayerController
    {
        get
        {
            if (this.playerController == null)
            {
                this.playerController = this.playerTransform.GetComponent<PlayerController>();
            }
            return this.playerController;
        }
    }

    public static int TriggerLayer
    {
        get { return LayerMask.NameToLayer("Trigger"); }
    }

    public static int GroundLayer
    {
        get { return LayerMask.NameToLayer("Default"); }
    }

    internal PlayerEntity[] playerEntities = new PlayerEntity[4];
    internal static PlayerEntity GetPlayer(int id = 0)
    {
        if (Instance && Instance.playerEntities!=null && Instance.playerEntities[id])
        {
            return Instance.playerEntities[id];
        }

        return null;
    }

    internal void AddPlayer(PlayerEntity player)
    {
        for (int i = 0; i < this.playerEntities.Length; i++)
        {
            if (this.playerEntities[i] != null)
            {
                continue;
            }

            this.playerEntities[i] = player;
            break;
        }
    }

    internal static int GetActivePlayerCount()
    {
        int count = 0;

        if (Instance && Instance.playerEntities != null)
        {
            foreach (var item in Instance.playerEntities)
            {
                if (item!=null && !item.dead)
                {
                    count++;
                }
            }
        }
        return count;
    }

    internal static Transform GetPlayerTransform()
    {
        if (Instance && Instance.playerEntities!=null)
        {
            return Instance.playerTransform;
        }

        return null;
    }
    internal static PlayerController GetPlayerController()
    {
        if (Instance && Instance.PlayerController)
        {
            return Instance.PlayerController;
        }

        return null;
    }
    private List<BasicAi> enemies = new List<BasicAi>();

    public List<BasicAi> GetAllEnemies()
    {
        return this.enemies;
    }

    private int _executionBloodLootID = 2;

    private void Awake()
    {
        if (!CreateSingleton())
        {
            return;
        }
    }

    private bool CreateSingleton()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return false;
        }
        if (!transform.parent)
        {
            DontDestroyOnLoad(gameObject);
        }

        instance = this;
        return true;
    }

    internal void AddEnemyReference(BasicAi newEnemy)
    {
        if (this.enemies == null)
        {
            Debug.LogError("ENEMIES NULL ON REFMANAGER");
            return;
        }
        if (newEnemy != null)
        {
            this.enemies.Add(newEnemy);
        }
    }

    internal void RemoveEnemyReference(BasicAi newEnemy)
    {
        if (this.enemies == null)
        {
            Debug.LogError("ENEMIES NULL ON REFMANAGER");
            return;
        }
        if (newEnemy != null)
        {
            this.enemies.Remove(newEnemy);
        }
    }


    public void ResetEnemyTarget(Transform newTransform)
    {
        if (this.enemies != null)
        {
            foreach (BasicAi enemy in this.enemies)
            {
                enemy.SetMoveTarget(newTransform);
            }
        }
    }

}
