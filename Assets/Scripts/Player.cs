using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons; // 망치,총1,총2 를 가지고 있는지 아닌지를 알려줌

    public GameObject[] grenades;
    public int hasGrenades;

    /*player가 현재 가지고 있는 각 아이템의 갯수*/
    public int ammo;
    public int coin;
    public int health;

    /*각 아이템의 최대값*/
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;
    float vAxis;

    bool wDown;//walk down
    bool jDown;//jump down
    bool iDown;//무기 입수
    bool isJump; //jump중인지를 확인
    bool isDodge; //Dodge중인지를 확인
    bool isSwap; //현재 swap중인지를 확인
    bool sDown1; //무기 1번
    bool sDown2; //무기 2번
    bool sDown3; //무기 3번

    Vector3 moveVec;
    Vector3 dodgeVec;
    Animator anim;
    Rigidbody rigid;

    GameObject nearObject;
    GameObject equipWeapon;//현재 장착중인 무기

    int equipWeaponIndex = -1; // 현재 들고있는 무기 (이건 숫자로)
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
        Interaction();
        Swap();
    }
    void GetInput(){
        hAxis = Input.GetAxisRaw("Horizontal"); // Axis 값을 정수로 변환하는 함수
        vAxis = Input.GetAxisRaw("Vertical"); // horizontal, vertical 은 edit>project settings에서 있음.
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");

    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if (isDodge)
        {
            moveVec = dodgeVec;
        }
        if (isSwap)
            moveVec = Vector3.zero;
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
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse); // Impulse는 즉각적인 힘을 줌
            anim.SetBool("isJump", true); // Land에니메이션 시작 안하기
            anim.SetTrigger("doJump"); // jump에니메이션
            isJump = true;
        }
    }

    void Dodge()
    {
        
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap)
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
    void Swap()
    {
        if (sDown1 && ((!hasWeapons[0]) || equipWeaponIndex == 0))// 무기가 없는데 바꾸려고하거나 들고있는 무긴데 그걸로 또 바꾸러고 하면 안함.
            return;
        if (sDown2 && ((!hasWeapons[1]) || equipWeaponIndex == 1))// 무기가 없는데 바꾸려고하거나 들고있는 무긴데 그걸로 또 바꾸러고 하면 안함.
            return;
        if (sDown3 && ((!hasWeapons[2]) || equipWeaponIndex == 2))// 무기가 없는데 바꾸려고하거나 들고있는 무긴데 그걸로 또 바꾸러고 하면 안함.
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;
        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if(equipWeapon != null)
                equipWeapon.SetActive(false);
            equipWeapon = weapons[weaponIndex];
            equipWeaponIndex = weaponIndex;
            weapons[weaponIndex].SetActive(true); //선택한 종류의 무기가 보이도록 함
            anim.SetTrigger("doSwap");
            isSwap = true;
            Invoke("SwapOut",0.4f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }
    void Interaction()
    {
        if (iDown && nearObject !=null && !isJump && !isDodge)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();//item 스크립트 가져오기
                int weaponIndex = item.value;// 해단 item의 value 값 (1,2,3..)을 가져온다
                hasWeapons[weaponIndex] = true; // 아이템 먹기!
                Destroy(nearObject);
            }
        }
    }

    


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")//점프 착지
        {
            anim.SetBool("isJump", false); // Land 에니메이션 시작 하기
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item"){
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo) ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if ( coin > maxCoin) coin = maxCoin ;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth) health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades) hasGrenades = maxHasGrenades;

                    break;

            }
            Destroy(other.gameObject);
        }

    }
    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon")
        {
            nearObject = other.gameObject;
            Debug.Log(nearObject);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
    }
}
