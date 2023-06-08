// 3D 생태계 기획 - 메어_임정현(괴물새) 괴물새를 기준으로 하는 구 콜라이더에 충돌하는 AI 오브젝트가 플레이어를 공격할 수 있도록 함.
// 여채현, 2023.06.08

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    public class OverlapSphereTest : MonoBehaviour
    {
        EmeraldAISystem emeraldComponent;
        EmeraldAIEventsManager eventsManager;

        public float radius = 10f;
        public float detectionCooldown = 10f;
        private bool isCooldown = false;

        void Start()
        {
            emeraldComponent = GetComponent<EmeraldAISystem>();
            eventsManager = emeraldComponent.EmeraldEventsManagerComponent;
        }

        void OnDrawGizmosSelected() // Scene에서 괴물새 어그로 범위를 시각화하기 위해 추가함. 만약 충돌 범위가 고정되면 삭제해도 됨. - 여채현 2023.06.08
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, radius);
        }


        void Update()
        {
            if (!isCooldown) // 괴물새가 플레이어를 인식할 때 마다 실행되면, OverrideCombatTarget 메소드가 매 프레임 마다 호출되어 전투 모드 돌입 애니메이션을 반복하기 때문에 쿨타임을 주었음. - 여채현 2023.06.08
            {
                Collider[] playerDetection = Physics.OverlapSphere(transform.position, 15f);
                bool isPlayerDetected = false;

                foreach (Collider col in playerDetection)
                {
                    if (col.gameObject.CompareTag("Player"))
                    {
                        isPlayerDetected = true;
                        break;
                    }
                }

                if (isPlayerDetected)
                {
                    int layerMask = 1 << LayerMask.NameToLayer("Hitbox"); // AI의 Layer를 모두 Hitbox로 놓고 진행하였기 때문에 감지 Layer를 Hitbox로 하였음. - 여채현 2023.06.08
                    Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask);

                    foreach (Collider col in colliders)
                    {
                        if (col.gameObject.layer == LayerMask.NameToLayer("Hitbox"))
                        {
                            EmeraldAISystem emeraldAI = col.gameObject.GetComponentInParent<EmeraldAISystem>();

                            if (col.name == "Densoptere") // 등대처럼 계속 돌아다녀야 하는 괴물새도 감지되므로, 전투 상태가 되지 않도록 괴물새는 제외함. - 여채현 2023.06.08
                            {
                                continue;
                            }

                            AttackPlayer(col.gameObject);
                        }
                    }

                    StartCoroutine(StartCooldown());
                }
            }
        }


        IEnumerator StartCooldown()
        {
            isCooldown = true;
            yield return new WaitForSeconds(10f);
            isCooldown = false;
        }

        void AttackPlayer(GameObject AIObject)
        {
            EmeraldAISystem emeraldAI = AIObject.GetComponentInParent<EmeraldAISystem>();

            if (emeraldAI != null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                Transform playerTransform = playerObject.transform;

                Debug.Log("플레이어를 공격하라!");
                emeraldAI.EmeraldEventsManagerComponent.OverrideCombatTarget(playerTransform); // 이미 있는 기능이므로 EmeraldAI에서 제공하는 메소드를 사용함. - 여채현 2023.06.08
            }
        }
    }
}
