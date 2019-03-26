using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionUtils : MonoBehaviour {

	public static Vector3 RandomPositionInArea(Vector3 area)
	{
		float randomX = Random.Range(0, area.x);
		float randomY = Random.Range(0, area.y);
		float randomZ = Random.Range(0, area.z);
        
		return new Vector3(randomX,randomY,randomZ);
	}
    
    public static Vector3 RandomPositionInArea(float radius)
    {
        float randomX = Random.Range(0, radius);
        float randomY = Random.Range(0, radius);
        float randomZ = Random.Range(0, radius);
        
        return new Vector3(randomX,randomY,randomZ);
    }

    public static Vector3 RandomPositionInCircle(float radius)
    {
        Vector3 randomPosition = RandomPositionInArea(radius);
        randomPosition.y = 0;

        return randomPosition;
    }


    /// <summary>
    /// IS A BIT SCARY, USE AT OWN RISK
    /// </summary>
    private static Collider[] colliders = new Collider[200];
    public static List<BasicAi> GetEnemiesInRadius(Vector3 position, float radius, bool getDead = false)
    {
        LayerMask layerMask = 1 << 12; //Sort out all colliders not tagged enemy
        Physics.OverlapSphereNonAlloc(position, radius, colliders, layerMask);
        List<BasicAi> enemies = new List<BasicAi>();
        
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
            {
                break; //First empty index in array, no need to loop empty ones
            }
            if (colliders[i] && colliders[i].CompareTag("Enemy"))
            {
                BasicAi hitAI = colliders[i].GetComponent<BasicAi>();

                if (hitAI && (!getDead && !hitAI.dead))
                {
                    if (!enemies.Contains(hitAI))
                    {
                        enemies.Add(hitAI);
                    }
                }
            }
        }

        return enemies;
    }
	
}
