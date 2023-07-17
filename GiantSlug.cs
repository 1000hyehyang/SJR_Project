// 3D 생태계 기획 - 메어_보스몹(민달팽이) 민달팽이가 지나가는 곳에 10초간 유지되는 독가스를 남기는 기능.
// 여채현, 2023.07.18

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantSlug : MonoBehaviour
{
    public GameObject toxicGasPrefab;
    public float shootingInterval = 2f;

    private void Start()
    {
        StartCoroutine(ShootToxicGasCoroutine());
    }

    private IEnumerator ShootToxicGasCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootingInterval);

            ShootToxicGas();
        }
    }

    private void ShootToxicGas()
    {
        GameObject newToxicGas = Instantiate(toxicGasPrefab, transform.position, transform.rotation);

        ToxicGas toxicGasScript = newToxicGas.GetComponent<ToxicGas>();
        if (toxicGasScript != null)
        {
            toxicGasScript.lifeTime = 10f;
        }
    }
}
