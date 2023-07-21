// 3D 생태계 기획 - 메어_임정현(괴물새) 괴물새를 기준으로 하는 구 콜라이더에 충돌하는 AI 오브젝트가 플레이어를 공격할 수 있도록 함.
// 여채현, 2023.06.08

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    public class AggroSound : MonoBehaviour
    {
        EmeraldAISystem emeraldComponent;
        EmeraldAIEventsManager eventsManager;

        public float radius = 10f;
        public float detectionCooldown = 5f;
        public AudioSource audioSource;
        public AudioClip detectionSound;
        private bool isCooldown = false;
        private Coroutine detectionCoroutine;

        void Start()
        {
            emeraldComponent = GetComponent<EmeraldAISystem>();
            eventsManager = emeraldComponent.EmeraldEventsManagerComponent;
            detectionCoroutine = StartCoroutine(DetectPlayer());
        }

        IEnumerator DetectPlayer()
        {
            while (true)
            {
                if (!isCooldown)
                {
                    Collider[] playerDetection = Physics.OverlapSphere(transform.position, 15f);
                    bool isPlayerDetected = false;

                    foreach (Collider col in playerDetection)
                    {
                        if (col.gameObject.CompareTag("Player")) 
                        {
                            isPlayerDetected = true;
                            Debug.Log("플레이어를 발견했다!"); // 괴물새가 플레이어를 주기적으로 인식하는지 알기 위함. 삭제해도 무방. - 여채현 2023.07.04
                            break;
                        }
                    }

                    if (isPlayerDetected)
                    {
                        int layerMask = 1 << LayerMask.NameToLayer("Hitbox");
                        Collider[] aggro = Physics.OverlapSphere(transform.position, radius, layerMask);

                        foreach (Collider col in aggro)
                        {
                            if (col.gameObject.layer == LayerMask.NameToLayer("Hitbox"))
                            {
                                EmeraldAISystem emeraldAI = col.gameObject.GetComponentInParent<EmeraldAISystem>();

                                if (col.gameObject == gameObject)
                                {
                                    continue;
                                }

                                AttackPlayer(col.gameObject);

                            }
                        }

                        StartCoroutine(StartCooldown());

                        if (audioSource != null && detectionSound != null)
                        {
                            audioSource.PlayOneShot(detectionSound);
                        }
                    }
                }

                yield return null;
            }
        }

        IEnumerator StartCooldown()
        {
            isCooldown = true;
            yield return new WaitForSeconds(detectionCooldown); 
            isCooldown = false;
        }

        void AttackPlayer(GameObject AIObject)
        {
            EmeraldAISystem emeraldAI = AIObject.GetComponentInParent<EmeraldAISystem>();

            if (emeraldAI != null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                Transform playerTransform = playerObject.transform;

                Debug.Log("플레이어를 공격하라!"); // 괴물새가 플레이어를 인식하여 어그로 범위 내 AI 오브젝트에 신호를 주는지 확인하기 위함. 삭제해도 무방. - 여채현 2023.06.08
                emeraldAI.EmeraldEventsManagerComponent.OverrideCombatTarget(playerTransform);
            }
        }

        void OnDestroy()
        {
            if (detectionCoroutine != null)
            {
                StopCoroutine(detectionCoroutine);
            }
        }
    }
}
