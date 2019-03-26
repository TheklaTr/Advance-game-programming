using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerableEnemySpawner : EnemySpawner {

	public float dangerMultiplier;
	public float timer;
	
	public float enemiesPerWave = 15;
	public float cooldownBetweenWaves = 5;

	private bool _active;

	private void OnTriggerEnter(Collider other)
	{
		if (LogicUtils.IsPlayer(other) == false)
		{
			return;
		}
        
		PlayerEntity player = other.GetComponent<PlayerEntity>();
		
		player.onEntityDeath -= StopSpawning;
		player.onEntityDeath += StopSpawning;


		StartCoroutine(StartSpawning());
	}

	private IEnumerator StartSpawning()
	{
		this._active = true;
		float currentTime = 0;

		float enemiesSpawned = 0;
		while (this._active)
		{
			if (enemiesSpawned >= this.enemiesPerWave)
			{
				enemiesSpawned = 0;
				yield return new WaitForSeconds(this.cooldownBetweenWaves);
			}

			currentTime += Time.deltaTime;
			if (currentTime > timer)
			{
				currentTime = 0;
				enemiesSpawned++;
				SpawnEnemies();
			}

			yield return null;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (LogicUtils.IsPlayer(other) == false)
		{
			return;
		}

		StopSpawning();
	}

	private void StopSpawning()
	{
		this._active = false;
	}
}
