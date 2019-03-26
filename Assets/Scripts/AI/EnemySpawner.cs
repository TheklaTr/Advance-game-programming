using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public Transform enemyHolder;
    [Header("If there is only 1 enemy type, it will be used regardless of weights.")]
    public List<EnemyTypeWeighted> enemyTypeList;
    public Vector3 spawnArea;
    
    public List<Transform> patrolPoints;

    public int enemiesToSpawn = 5;
    public List<BasicAi> enemiesActive = new List<BasicAi>();


    public float noSpawnAroundPlayerDistance = 5;
    public float preSpawnedEnemies = 3;

    protected void SpawnEnemies()
    {
        StartCoroutine(SpawnCoroutine());
    }
    
    public void PreSpawnEnemies()
    {
        for (int i = 0; i < this.preSpawnedEnemies; i++)
        {
            StartCoroutine(SpawnCoroutine());
        }
    }
    
    protected IEnumerator SpawnCoroutine()
    {
        if (NullCheck() == false)
        {
            yield break;
        }
        List<Transform> enemySpawnPositions = new List<Transform>();
        foreach (Transform child in this.enemyHolder)
        {
            enemySpawnPositions.Add(child);
        }
        for (int i = 0; i < this.enemiesToSpawn; i++)
        {
            Transform spawnPoint = RandomSpawnPoint(enemySpawnPositions);
            
            yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
            if (PoolManager.Instance == null)
            {
                continue;
            }

            Vector3 enemyPosition = spawnPoint.position + PositionUtils.RandomPositionInArea(this.spawnArea);

            PoolObjectSettings enemySettings = new PoolObjectSettings();
            enemySettings.positionToSet = enemyPosition;
            enemySettings.parentToSet = spawnPoint.gameObject;

            string randomEnemyType = GetRandomEnemyType(0);


            GameObject randomEnemy = PoolManager.Instance.GetPooledObject(randomEnemyType, enemySettings);

            AssignEnemySettings(randomEnemy);
        }
    }

    private string GetRandomEnemyType(float wantedLevel)
    {
        if (this.enemyTypeList.Count == 0)
        {
            Debug.LogError("Trying to spawn enemies with an empty enemy list!");
            return null;
        }

        if (this.enemyTypeList.Count == 1)
        {
            return this.enemyTypeList[0].enemyType;
        }

        float totalChance = 0f;

        foreach (EnemyTypeWeighted enemy in this.enemyTypeList)
        {
            totalChance += enemy.GetWeight();
        }

        if (totalChance <= 0)
        {
            Debug.LogError("Enemy weighted spawning got negative or 0 weight!");
            return null;
        }

        float randomNumber = Random.Range(0, totalChance);
        float currentlyAt = 0;
        int currentIndex = 0;
        int winnerIndex = -1;

        foreach (EnemyTypeWeighted enemy in this.enemyTypeList)
        {
            float maxLimitOfGettingIt = currentlyAt + enemy.GetWeight();

            if (randomNumber >= currentlyAt && randomNumber < maxLimitOfGettingIt)
            {
                winnerIndex = currentIndex;
                break;
            }

            currentIndex++;
            currentlyAt = maxLimitOfGettingIt;
        }

        if (winnerIndex < 0 || this.enemyTypeList.Count <= winnerIndex)
        {
            Debug.LogError("Something went wrong when trying to get a random enemy from enemy table!");
            return null;
        }

        return this.enemyTypeList[winnerIndex].enemyType;
    }

    private Transform RandomSpawnPoint(List<Transform> enemySpawnPositions, int numOfTries = 10)
    {
        while (true)
        {
            int randomSpawn = Random.Range(0, enemySpawnPositions.Count);
            Transform spawnPoint = enemySpawnPositions[randomSpawn];
            Transform player = ReferenceManager.GetPlayerTransform();
            if (player && Vector3.Distance(spawnPoint.position, player.position) < this.noSpawnAroundPlayerDistance)
            {
                enemySpawnPositions.Remove(spawnPoint);
                continue;
            }

            return spawnPoint;
        }
    }

    private void AssignEnemySettings(GameObject randomEnemy)
    {
        if (randomEnemy != null)
        {
            randomEnemy.name = LogicUtils.RandomString(7);
               
            BasicAi AI = randomEnemy.GetComponent<BasicAi>();
            if (AI == null)
            {
                Debug.LogError("Tried assigning settings to no ai");
                return;
            }
            CreateReferencesToEnemy(AI);
        }
    }

    private void CreateReferencesToEnemy(BasicAi AI)
    {
        AI.MySpawner = this;
        this.enemiesActive.Add(AI);
        if (ReferenceManager.Instance != null)
        {
            ReferenceManager.Instance.AddEnemyReference(AI);
        }
    }

    internal void RemoveDeadAi(BasicAi deadAi)
    {
        this.enemiesActive.Remove(deadAi);
    }

    private bool NullCheck()
    {
        if (this.enemyHolder == null || this.enemyHolder.childCount == 0)
        {
            Debug.LogError("EnemySpawner no positions");
            return false;
        }

        if (this.enemyTypeList == null)
        {
            Debug.LogError("EnemySpawner no enemies");
            return false;
        }

        return true;
    }

}

[System.Serializable]
public class EnemyTypeWeighted
{
    public string enemyType;
    public float baseWeight;

    internal float GetWeight()
    {
        return baseWeight;
    }

}
