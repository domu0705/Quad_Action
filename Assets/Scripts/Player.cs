using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    float hAxis;
    float vAxis;
    bool wDown;//walk down
    bool jDown;//jump down

    bool isJump; //jump중인지를 확인
    bool isDodge; //Dodge중인지를 확인

    Vector3 moveVec;
    Vector3 dodgeVec;
    Animator anim;
    Rigidbody rigid;

    
    void Awake ()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
    }
    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // Axis 값을 정수로 변환하는 함수
        vAxis = Input.GetAxisRaw("Vertical"); // horizontal, vertical 은 edit>project settings에서 있음.
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if (isDodge)
        {
            moveVec = dodgeVec;
        }
        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun",moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);

    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse); // Impulse는 즉각적인 힘을 줌
            anim.SetBool("isJump", true); // Land에니메이션 시작 안하기
            anim.SetTrigger("doJump"); // jump에니메이션
            isJump = true;
        }
    }

    void Dodge()
    {
        
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge"); // jump에니메이션
            isDodge  = true;
            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")//점프 착지
        {
            anim.SetBool("isJump", false); // Land 에니메이션 시작 하기
            isJump = false;
        }
    }
}
