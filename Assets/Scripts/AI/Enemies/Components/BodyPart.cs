using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
	public float takenDamageModifier;
	public ParticleSystem damageParticles;
	public Collider myCollider;
	public Animator animator;
	public string animatorBool;
	

	
	internal BaseEntity parentEntity;

	private bool logStuff = false;
	
	private void Start()
	{
		if (this.parentEntity == null)
		{
			this.parentEntity = GetComponentInParent<BaseEntity>();
		}

		if (this.animator == null)
		{
			if (this.parentEntity != null)
			{
				this.animator = this.parentEntity.GetComponent<Animator>();
			}
		}

		if (this.myCollider == null)
		{
			this.myCollider = GetComponent<Collider>();
		}
		
	}

	internal void TakeDamage<T>(float damage, T dmgSource)
	{
		if (this.parentEntity == null)
		{
			if(this.logStuff)
			Debug.Log("No parent entity on " + name);
			return;
		}
		
		this.parentEntity.ChangeHealth(-(damage + damage * this.takenDamageModifier), dmgSource);		
		
		if (this.damageParticles == null)
		{			
			return;
		}
		this.damageParticles.Play();

		AnimateDamage();
	}
	
	private void AnimateDamage()
	{
		if (string.IsNullOrEmpty(this.animatorBool) == false && this.animator != null)
		{
			this.animator.SetBool(this.animatorBool, true);
		}
	}

	public void SwitchTrigger(bool newState)
	{
		if (this.myCollider)
		{
			this.myCollider.isTrigger = newState;
		}
	}
}
