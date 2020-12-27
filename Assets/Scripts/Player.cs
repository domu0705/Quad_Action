using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    float hAxis;
    float vAxis;
    bool wDown;//walk down

    Vector3 moveVec;
    Animator anim;
    // Start is called before the first frame update
    void Awake ()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // Axis 값을 정수로 변환하는 함수
        vAxis = Input.GetAxisRaw("Vertical"); // horizontal, vertical 은 edit>project settings에서 있음.
        wDown = Input.GetButton("Walk");
      
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if (wDown)
            transform.position += moveVec * speed *0.3f* Time.deltaTime;
        else
            transform.position += moveVec * speed * Time.deltaTime;

        anim.SetBool("isRun",moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);

        transform.LookAt(transform.position + moveVec);
    }
}
