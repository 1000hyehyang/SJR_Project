// 3D 생태계 기획 - 메어_임정현(군중) 가장 가까이에 있는 군중 리더를 따라가도록 설정한 기능. 
// 여채현, 2023.07.21

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI;

public class CompanionHumans : MonoBehaviour
{
    EmeraldAISystem EmeraldComponent;
    EmeraldAIEventsManager eventsManager;
    GameObject playerObject;
    float playerDetectionRadius = 10.0f;

    void Start()
    {
        EmeraldComponent = GetComponent<EmeraldAISystem>();
        eventsManager = EmeraldComponent.EmeraldEventsManagerComponent;
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        FindAndFollowLeader();

        float distanceToPlayer = Vector3.Distance(transform.position, playerObject.transform.position);
        if (distanceToPlayer <= playerDetectionRadius)
        {
            eventsManager.OverrideCombatTarget(playerObject.transform);
        }
    }

    void FindAndFollowLeader()
    {
        LeaderHumans[] leaderScripts = GameObject.FindObjectsOfType<LeaderHumans>();

        Transform closestLeaderTransform = null;
        float closestDistance = Mathf.Infinity;

        foreach (LeaderHumans leaderScript in leaderScripts)
        {
            float distanceToLeader = Vector3.Distance(transform.position, leaderScript.transform.position);

            if (distanceToLeader < closestDistance)
            {
                closestDistance = distanceToLeader;
                closestLeaderTransform = leaderScript.transform;
            }
        }

        if (closestLeaderTransform != null)
        {
            eventsManager.SetFollowerTarget(closestLeaderTransform);
        }
    }
}
