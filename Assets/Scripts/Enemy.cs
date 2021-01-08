using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;//navigator 쓰려면 이거 꼭 추가해주기!
public class Enemy : MonoBehaviour
{
    public enum Type { A,B,C};
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public bool isChase;
    public bool isAttack;//지금 공격을 하고있는지아닌지
    public BoxCollider meleeArea;
    public GameObject bullet;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav; // NavMesh: navagent가 경로를 그리기 위한 바탕(길을 그릴 스케치북 같은 거)
    /*
     * navMesh는 static으로 체크된 object만 bake가능.
     * 그래서 world의 wall과 floor을 static으로 체크해주어야 함
     */
    Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }

    void FreezeVelocity()//주기적으로 속도, 회전속도를 0으로 만들어 자동으로 돌아가거나 충돌해서 밀려나는 것을 방지.
    {
        rigid.velocity = Vector3.zero; 
        rigid.angularVelocity = Vector3.zero; //angularVelocity 는 물리 회전 속도를 의미

    }

    void targeting()
    {
        /* sphereCast의 반지름,길이를 조정할 변수*/
        float targetRadius = 0;
        float targetRange = 0;

        switch (enemyType)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRange = 3f;
                break;
            case Type.B:
                targetRadius = 1f;
                targetRange = 12f;
                break;
            case Type.C:
                targetRadius = 0.5f;
                targetRange = 25f;
                break;
        }
        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius,
            transform.forward, targetRange, LayerMask.GetMask("Player"));

        if((rayHits.Length > 0) && !isAttack)//공격 중이면 하던 공격 끝내고 해야 됨.
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        Debug.Log("attack에 들어왔어");
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);


        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);// 공격! 인데 attack의 animation이 실행되기까지 약간의 delay가 있으니까 그걸 맞춰주려고 0.2초 기다리게 함.
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f); //공격이 끝났다면 다시 적앞에 공격범위를 없애줌.
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet,transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }
        
        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);

       

    }

    void FixedUpdate()
    {
        if(isChase)
            FreezeVelocity();
        targeting();
    }


    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    private void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);//도착할 목표 위치 지정 함수
            nav.isStopped = !isChase;   //isStipped로 enemy가 완벽하게 멈추도록 작성.
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;//몬스터가 맞았을때 넉벡을 위해서.
            
            StartCoroutine(OnDamage(reactVec, false));
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject); // 총알 삭제
            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec,true));
    }
    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if(curHealth > 0)
        {
            mat.color = Color.white;
        }
        else//적의 체력이 다 닳아서 죽었다면
        {
            mat.color = Color.gray;
            gameObject.layer = 14;
            isChase = false;
            nav.enabled = false;//적이 플레이어를 따라가는걸 활성화 해놓으면 y축(위방향)으로 움직이는 모션을 할 수가 없기때문에 꺼둠,
            anim.SetTrigger("doDie");

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up*3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
                Destroy(gameObject, 4);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec*5,ForceMode.Impulse);
                Destroy(gameObject, 4);
            }
            
        }
    }
}
