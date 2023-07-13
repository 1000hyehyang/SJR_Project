// 3D 생태계 기획 - 메어_보스몹(개굴뚝이) 공격력 상승, 공격 속도 상승, 이동 속도 상승 기능
// 여채현, 2023.07.10

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    public class FeverState : MonoBehaviour
    {
        EmeraldAISystem emeraldComponent;
        EmeraldAIEventsManager eventsManager;

        // Start is called before the first frame update
        void Start()
        {
            emeraldComponent = GetComponent<EmeraldAISystem>();
            eventsManager = emeraldComponent.EmeraldEventsManagerComponent;

            StartCoroutine(UpdateFeverState());
        }

        IEnumerator UpdateFeverState()
        {
            while (true)
            {
                eventsManager.UpdateAIMeleeDamage(1, 10, 20);
                eventsManager.UpdateAIMeleeDamage(2, 10, 20);
                eventsManager.UpdateAIMeleeDamage(3, 10, 20); // UpdateAIMeleeDamage(index, min Damage, max Damage) - 여채현 2023.07.13
                eventsManager.UpdateAIMeleeAttackSpeed(0, 0); // EmeraldAI 기본 세팅이 (0,0)임. << 쿨타임 없이 공격하는 상태. 따라서 공격속도를 조절하기 위해서 기본 세팅을 변경해주어야 함. - 여채현 2023.07.13

                emeraldComponent.WalkSpeed += 0.5f;
                emeraldComponent.RunSpeed += 0.5f;

                yield return null; 
            }
        }
    }
}
