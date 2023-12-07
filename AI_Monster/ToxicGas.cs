// 3D 생태계 기획 - 메어_보스몹(민달팽이) 해당 스크립트는 생성된 독가스를 10초 후 삭제하기 위함. 독가스 이속 감소 기능은 디버프 완료 후 추가해야 함.
// 여채현, 2023.07.18

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicGas : MonoBehaviour
{
    public float lifeTime = 10f; 

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
