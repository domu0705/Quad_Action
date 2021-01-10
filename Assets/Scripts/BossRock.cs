using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : Bullet
{

    Rigidbody rigid;
    float angularPower = 2;
    float scaleValue = 0.1f;
    bool isShoot;//기를 모으고 쏘는 타이밍을 관리

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTImer());
        StartCoroutine(GainPower());
    }

    IEnumerator GainPowerTImer()//기를 모드는 시간
    {
        yield return new WaitForSeconds(2.2f);
        isShoot = true;
    }

    IEnumerator GainPower()//기를 모드는 시간
    {
        while (!isShoot)
        {
            angularPower += 0.2f;
            scaleValue += 0.005f;
            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);//회전
            yield return null;//이거 꼭 넣어줘야지 게임이 정지 안됨.
        }
    }
}
