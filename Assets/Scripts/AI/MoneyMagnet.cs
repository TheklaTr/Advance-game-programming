using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyMagnet : MonoBehaviour
{

	[SerializeField]private float _radius = 3;
	[SerializeField]private float _collectionTime = 1;
	[SerializeField]private LayerMask _collectibleMask;
	
	Collider[] colliders = new Collider[20];

	private void Update()
	{
		LookForMoney();
	}

	private void LookForMoney()
	{
		int moneys = Physics.OverlapSphereNonAlloc(transform.position, this._radius, colliders, this._collectibleMask);
		
		for (int i = 0; i < moneys; i++)
		{
			if (this.colliders[i] == null)
			{
				continue;
			}
			StartCoroutine(Collect(this.colliders[i]));
		}
	}

	private IEnumerator Collect(Collider collider)
	{
		Vector3 startPosition = collider.transform.position;
		
		float timePassed = 0;
		while (timePassed < this._collectionTime)
		{
			if (collider == null)
			{
				yield break;
			}
			timePassed += Time.deltaTime;
			collider.transform.position = Vector3.Lerp(startPosition,transform.position,timePassed/this._collectionTime);
			yield return null;
		}
	
	}
}
