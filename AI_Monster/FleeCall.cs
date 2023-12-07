// 3D 생태계 기획 - 메어_임정현(도마뱀 인간) 체력이 20% 이하가 되었을 때, 가장 가까운 동료 오브젝트의 위치로 도망간 후 일정 범위 내에 있는 동료 오브젝트가 Player를 공격하도록 함.
// 여채현, 2023.07.21

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    public class FleeCall : MonoBehaviour
    {
        public float healthThreshold = 0.2f;
        private EmeraldAISystem emeraldComponent;
        private EmeraldAIEventsManager eventsManager;
        private Transform closestTarget;
        private bool isFled = false;
        private bool reinforcementsCalled = false;


        private void Start()
        {
            emeraldComponent = GetComponent<EmeraldAISystem>();
            eventsManager = emeraldComponent.EmeraldEventsManagerComponent;

            StartCoroutine(FleeUpdateCoroutine());
        }

        private IEnumerator FleeUpdateCoroutine()
        {
            while (true)
            {
                if (emeraldComponent.CurrentHealth <= emeraldComponent.StartingHealth * healthThreshold && !isFled)
                {
                    FindClosestTargetAndFlee();
                }

                if (isFled && closestTarget != null && !reinforcementsCalled)
                {
                    float distanceToClosestTarget = Vector3.Distance(transform.position, closestTarget.position);
                    if (distanceToClosestTarget <= 10.0f)
                    {
                        CallReinforcements();
                        reinforcementsCalled = true; 
                    }
                }

                yield return null;
            }
        }

        private void FindClosestTargetAndFlee()
        {
            FleeCall[] fleeScripts = FindObjectsOfType<FleeCall>();

            float closestDistance = Mathf.Infinity;

            foreach (FleeCall script in fleeScripts)
            {
                if (script.gameObject == gameObject)  
                    continue;

                float distance = Vector3.Distance(transform.position, script.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = script.transform;
                }
            }

            if (closestTarget != null)
            {
                isFled = true;

                eventsManager.OverrideCombatTarget(closestTarget);
            }
        }

        private void CallReinforcements()
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 20f); // 도망친 오브젝트의 주변 범위 20f 내에 있는 모든 도마뱀 동료들을 부를 수 있음. - 여채현 2023.07.21
            foreach (Collider collider in nearbyColliders)
            {
                FleeCall fleeScript = collider.GetComponent<FleeCall>();

                if (fleeScript != null)
                {
                    EmeraldAISystem emeraldAI = collider.GetComponentInParent<EmeraldAISystem>();

                    if (emeraldAI != null)
                    {
                        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                        Transform playerTransform = playerObject.transform;

                        emeraldAI.EmeraldEventsManagerComponent.OverrideCombatTarget(playerTransform);
                        Debug.Log("공격해 얘들아!"); // 도망친 후 Player를 공격하도록 불러오는지 알기 위함. 삭제해도 무방. - 여채현 2023.07.21
                    }
                }
            }
        }
    }
}
