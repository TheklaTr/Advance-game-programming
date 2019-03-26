using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoamMover : EnemyMover
{
    [SerializeField] private float _distanceLimit = 10;

    protected override  void Start()
    {
        base.Start();
        StartCoroutine(WaitForPlayer());
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        Roam();
    }

    private void Roam()
    {
        Vector3 positionToCheck = transform.position;
        Vector3 destination = this._navMeshAgent.destination;
        positionToCheck.y = destination.y;
        if (Vector3.Distance(positionToCheck, destination) <= this._navMeshAgent.stoppingDistance)
        {
            FindNewMovePoint();
        }
    }

    private void FindNewMovePoint()
    {
        float numOfTries = this._distanceLimit / 2;
        while (true)
        {
            if (numOfTries <= 0)
            {
                GoBackToSpawn();
                return;
            }

            Vector3 randomDirection = Random.insideUnitSphere * 2 * numOfTries;

            randomDirection += this.originalPos;
            UnityEngine.AI.NavMeshHit hit;
            UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, 20, 1);

            if (float.IsInfinity(hit.position.magnitude))
            {
                numOfTries--;
                continue;
            }

            Vector3 finalPosition = hit.position;
            this._navMeshAgent.SetDestination(finalPosition);
            break;
        }
    }

    private IEnumerator WaitForPlayer()
    {
        while (true)
        {
            if (this.AI.CanSeePlayer() == false)
            {
                yield return null;
            }
            else
            {
                break;
            }
        }

        AI.SwitchMover(typeof(ChaseMover));
    }

}
