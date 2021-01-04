using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{

    public GameObject meshObj;
    public GameObject effectObj;
    public Rigidbody rigid;
     
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Explosion());
        //StartCoroutine("Explosion"); 이것도 됨.
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(2.5f);
        rigid.velocity = Vector3.zero;//속도 없애기
        rigid.angularVelocity = Vector3.zero;//회전 없애기
        meshObj.SetActive(false);
        effectObj.SetActive(true);

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 15,
            Vector3.up, 0f, LayerMask.GetMask("Enemy"));//2:반지름,3:쏘는 방향,근데 상관없음 4:ray를 쏘는 길이(구체 형태를 그대로 위로 쏘아올리는 모양이 돼서 지금은 쓰면 안됨)
        foreach(RaycastHit hitObj in rayHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position); // 수류탄의 위치도 인자로 전달
            Debug.Log("적이 수류탄을 맞음");
        }

        Destroy(gameObject, 5);
    }
}
