using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public string poolTypeString;
    public GameObject poolBasePrefab;
    [Tooltip("This setting overrides anything set via the scripts, if this value is over 0.")]
    public float backToPoolTimeOverride = 0f;

    private float returnBackToPoolTime = 0f;
    private PoolObjectSettings poolObjectSettings;

    public virtual void SetPoolSettings(PoolObjectSettings setPoolObjectSettings)
    {
        poolObjectSettings = setPoolObjectSettings;

        if (poolObjectSettings != null)
        {
            SetReturnToPoolTime(poolObjectSettings.timeBeforeReturningToPool);
        }
    }

    public virtual void SetReturnToPoolTime(float setTo)
    {
        if (this.backToPoolTimeOverride > 0)
        {
            this.returnBackToPoolTime = backToPoolTimeOverride;
        }
        else
        {
            this.returnBackToPoolTime = setTo;
        }

        if (this.returnBackToPoolTime > 0)
        {
            StartCoroutine(TimedReturnThisBackToThePool());
        }
    }

    private void OnEnable()
    {
        if (this.backToPoolTimeOverride > 0)
        {
            returnBackToPoolTime = backToPoolTimeOverride;
            StartCoroutine(TimedReturnThisBackToThePool());
        }
    }

    public virtual void ReturnPooledObjectBackToPool()
    {
        if (PoolManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(poolTypeString))
            {
                if (!PoolManager.Instance.ReturnObjectToPool(poolTypeString, gameObject))
                {
                    Destroy(gameObject);
                }
            }
            else if (poolBasePrefab != null)
            {
                if (!PoolManager.Instance.ReturnObjectToPool(poolBasePrefab, gameObject))
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator TimedReturnThisBackToThePool()
    {
        yield return new WaitForSeconds(returnBackToPoolTime);
        ReturnPooledObjectBackToPool();
    }

}
