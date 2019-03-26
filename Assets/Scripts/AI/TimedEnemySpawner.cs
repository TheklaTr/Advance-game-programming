using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEnemySpawner : EnemySpawner
{

	public float timer;

	private void Start()
	{
		InvokeRepeating("SpawnEnemies", 5, this.timer);
	}

}
