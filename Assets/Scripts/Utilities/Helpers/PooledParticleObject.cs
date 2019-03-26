using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledParticleObject : PooledObject
{
    public List<ParticleSystem> particleSystemsToStop = new List<ParticleSystem>();

    public override void ReturnPooledObjectBackToPool()
    {
        if (particleSystemsToStop == null)
        {
            base.ReturnPooledObjectBackToPool();
        }
        else if (particleSystemsToStop.Count == 0)
        {
            base.ReturnPooledObjectBackToPool();
        }
        else
        {
            StartCoroutine(returnToPoolWhenAllParticlesHaveEnded());
        }
    }

    IEnumerator returnToPoolWhenAllParticlesHaveEnded()
    {
        if (particleSystemsToStop == null)
        {
            base.ReturnPooledObjectBackToPool();
            yield break;
        }

        List<ParticleSystem> waitingOnTheseSystems = new List<ParticleSystem>(particleSystemsToStop);

        foreach (ParticleSystem particle in waitingOnTheseSystems)
        {
            if (particle != null)
            {
                particle.Stop();
            }
        }

        yield return null;

        while (waitingOnTheseSystems.Count > 0)
        {
            for (int i = waitingOnTheseSystems.Count - 1; i >= 0; i--)
            {
                ParticleSystem waitingOnThis = waitingOnTheseSystems[i];

                if (waitingOnThis != null)
                {
                    if (!waitingOnThis.IsAlive())
                    {
                        waitingOnTheseSystems.RemoveAt(i);
                    }
                }
                else
                {
                    waitingOnTheseSystems.RemoveAt(i);
                }
            }

            yield return null;
        }

        base.ReturnPooledObjectBackToPool();
    }

}