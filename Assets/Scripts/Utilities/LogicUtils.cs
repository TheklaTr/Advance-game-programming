using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

	public class LogicUtils : MonoBehaviour
	{
		public static bool NullCheck(Object obj, Component owner)
		{
			if (ReferenceEquals(owner, null)|| owner == null)
			{
				string variable = "empty component";
				if (ReferenceEquals(obj, null) == false)
				{
					variable = obj.GetType().ToString();
				}
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            Debug.LogError("tried to nullcheck " + variable + " on an empty owner while executing " /* +trace*/);
				return false;
			}

			if (ReferenceEquals(obj, null) || obj == null)
			{
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
            Debug.LogError("some variable missing on " + owner.name + " while executing " /* +trace*/);
				return false;
			}

			return true;
		}

		
		public static bool MultiNullCheck(Component owner, params Object[] args)
		{
			foreach (var obj in args)
			{
				if (NullCheck(obj, owner) == false)
				{
					return false;
				}
			}

			return true;
		}

		public static bool IsPlayer(Component obj)
		{
			if (obj == null)
			{
				return false;
			}

			return obj.CompareTag("Player");
		}
		public static bool IsPet(Component obj)
		{
			if (obj == null)
			{
				return false;
			}

			return obj.CompareTag("Pet");
		}

		public static bool IsEnemy(Component obj)
		{
			if (obj == null)
			{
				return false;
			}

			return obj.CompareTag("Enemy");
		}

		public static bool IsHealingArea(Component obj)
		{
			if (obj == null)
			{
				return false;
			}

			return obj.name.Contains("Healing");
		}

		public static bool IsTrigger(Component obj)
		{
			if (obj == null)
			{
				return false;
			}

			return obj.gameObject.layer == ReferenceManager.TriggerLayer;
		}

		public static bool IsShootable(Component other)
		{
			bool isShootable = IsHealingArea(other) == false && IsTrigger(other) == false && other.CompareTag("Collectible") == false;

			return isShootable;
		}



		public static bool CanBeHitByBullet(BodyPart bodyPart, bool playerBullet = true)
		{
			if (bodyPart == null)
			{
				return false;
			}

			BaseEntity parentEntity = bodyPart.parentEntity;

			return CanBeHitByBullet(parentEntity, playerBullet);
		}

		public static bool CanBeHitByBullet(BaseEntity entityHit, bool playerBullet = true)
		{
			if (entityHit == null)
			{
				return false;
			}

			bool playerHit = ((!playerBullet) && entityHit is PlayerEntity);
			bool enemyHit = playerBullet && entityHit is BasicAi && entityHit.dead == false;

			return playerHit || enemyHit;
		}
		
		public static bool CanBeHitByBullet(Collider hitCollider,bool playerBullet = true)
		{
			BodyPart bodyPart = hitCollider.GetComponent<BodyPart>();
			if (bodyPart)
			{
				return CanBeHitByBullet(bodyPart, playerBullet);
			}
			BaseEntity entityHit = hitCollider.GetComponent<BaseEntity>();
			if(entityHit)
			{
				return CanBeHitByBullet(entityHit,playerBullet);
			}
            
            return true;
		}

		public static float GetAnimationLength(Animator anim, string animationName)
		{
			float time = -1;

			RuntimeAnimatorController ac = anim.runtimeAnimatorController;

			foreach (AnimationClip clip in ac.animationClips)
			{
				if (clip.name == animationName)
				{
					time = clip.length;
				}
			}

			return time;
		}

		public static float GetCurrentAnimationLength(Animator anim)
		{
			float length = anim.GetCurrentAnimatorStateInfo(0).length / anim.GetCurrentAnimatorStateInfo(0).speedMultiplier;
			return HasValue(length) ? length : 0;
		}

		public static bool HasValue(float value)
		{
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}

		public static Dictionary<T, T2> FillDictionary<T, T2>(string dataPath, Func<T2, T> keyFunc) where T2 : UnityEngine.Object
		{
			T2[] data = Resources.LoadAll<T2>(dataPath);
			Dictionary<T, T2> dictionary = new Dictionary<T, T2>();
			if (data != null && data.Length > 0)
			{

				foreach (T2 obj in data)
				{
					if (obj == null)
					{
						continue;
					}


					if (!dictionary.ContainsKey(keyFunc(obj)))
					{
						dictionary.Add(keyFunc(obj), obj);
					}
				}
			}

			return dictionary;
		}

		public static List<GameObject> GetAllChildrenOf(Transform parent)
		{
			List<GameObject> children = new List<GameObject>();
			foreach (Transform child in parent)
			{
				children.Add(child.gameObject);
				children.AddRange(GetAllChildrenOf(child));
			}

			return children;
		}

        public static void CheckColliderForEntityHit(List<BaseEntity> alreadyHitEntities, List<BaseEntity> ignoredEntities, Collider other)
        {
            BodyPart hitBodyPart = other.GetComponent<BodyPart>();

            if (hitBodyPart != null)
            {
                if (hitBodyPart.parentEntity != null)
                {
                    if (alreadyHitEntities.Contains(hitBodyPart.parentEntity) || ignoredEntities.Contains(hitBodyPart.parentEntity))
                    {
                        return;
                    }
                    else
                    {
                        alreadyHitEntities.Add(hitBodyPart.parentEntity);
                    }
                }
            }
            else
            {
                BaseEntity hitEntity = other.GetComponent<BaseEntity>();

                if (hitEntity)
                {
                    if (ignoredEntities.Contains(hitEntity) || alreadyHitEntities.Contains(hitEntity))
                    {
                        return;
                    }

                    alreadyHitEntities.Add(hitEntity);
                }
            }
        }

        public static int FloorToNearestTen(int number)
        {
            int numOfDivisions = 0;
            float divisibleNumber = number;

            while (true)
            {
                divisibleNumber /= 10;
                if (divisibleNumber < 1)
                {
                    break;
                }
                numOfDivisions++;
            }

            divisibleNumber *= 10;
            divisibleNumber = Mathf.Floor(divisibleNumber);

            for (int i = 0; i < numOfDivisions; i++)
            {
                divisibleNumber *= 10;
            }

            return (int)divisibleNumber;
        }

        public static string RandomString(int length)
		{
			string path = Path.GetRandomFileName();
			path = path.Replace(".", ""); // Remove period.
			return path;
		}

	
	}


   

	
	