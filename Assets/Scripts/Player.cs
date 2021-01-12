using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons; // 망치,총1,총2 를 가지고 있는지 아닌지를 알려줌

    public GameObject grenadeObj;//던질 수류탄을 저장할 변수 추가
    public GameObject[] grenades;
    public int hasGrenades;

    public Camera followCamera;

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
    bool fDown; //공격 키
    bool rDown;//총 장전 키
    bool iDown;//무기 입수
    bool isJump; //jump중인지를 확인
    bool isDodge; //Dodge중인지를 확인
    bool isSwap; //현재 swap중인지를 확인
    bool isFireReady = true;//공격 준비 됐는지
    bool isReload;//현재 장전중인지를 확인
    bool isBorder;//플레이어와 벽이 충돌했는지르르 확인
    bool gDown;//grenade를 던질건지
    bool isShop; //쇼핑중인지 확인
    bool sDown1; //무기 1번
    bool sDown2; //무기 2번
    bool sDown3; //무기 3번

    bool isDamage = false;//적에게 공격받고 난 뒤 무적 타임

    Vector3 moveVec;
    Vector3 dodgeVec;
    Animator anim;
    Rigidbody rigid;
    MeshRenderer[] meshes;
 
    GameObject nearObject;
    Weapon equipWeapon;//현재 장착중인 무기

    int equipWeaponIndex = -1; // 현재 들고있는 무기 (이건 숫자로)
    float fireDelay; //공격 딜레이
    void Awake ()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshes = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Interaction();
        Swap();
    }
    void GetInput(){
        hAxis = Input.GetAxisRaw("Horizontal"); // Axis 값을 정수로 변환하는 함수
        vAxis = Input.GetAxisRaw("Vertical"); // horizontal, vertical 은 edit>project settings에서 있음.
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        rDown = Input.GetButtonDown("Reload");
        fDown = Input.GetButton("Fire1"); //마우스 왼쪽
        gDown = Input.GetButtonDown("Fire2");//마우스 우클릭
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
        if (isSwap || !isFireReady || isReload)
        { // isSwap이나 망치를 휘두르는 중(이때는 ready가 false) , 재장전 중에는 이동을 멈춤
            moveVec = Vector3.zero; // 회전에서 moveVec사용중이라 이때는 회전도 할 수 없음.
        }
        if (!isBorder)
        {
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        }

        anim.SetBool("isRun",moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);

    }

    void Attack()
    {
        if (equipWeapon == null)
            return;
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay; // 공격을 이미 해서 공격 모션이 이루어지는 동안은 기다려야 함.
        if(fDown && isFireReady && !isDodge && !isSwap && !isShop)//공격!
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null)//손에 들린 무기가 없다면 
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (ammo == 0)//총알이 없다면
            return;
        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop){//총을 쏠 수 있는 상태
            anim.SetTrigger("doReload");//에니메이션 안에 있는 do Reload를 활성화!
            isReload = true;
            Invoke("ReloadOut", 2f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;//플레이어가 총알을 maxAmmo개보다 적게 가지고 있으면 지금 가지고 있는 것 만큼만 장전. 
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Turn()
    {
        //키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //마우스에 의한 회전
        if (fDown)//총을 쏜다면
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);//스크린에서 월드로 ray를 쏘는 함수.마우스로부터 월드로 레이를 쏴서 바닥에 닿는대!
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))//2번쨰 매개변수는 return처럼 반환ㄴ값을 주어진 변수에 저장하는 키워드임. 3번째 매개변수는 ray의 길이
            {
                Vector3 nextVec = rayHit.point - transform.position;  //rayHit.point는 ray가 바닥에 닿은 지점
                nextVec.y = 0;//ray가 벽위에 닿았을때 위를 보면서 축이 기울어지는 것을 방지하기 위함.
                transform.LookAt(transform.position + nextVec);
            }
        }
        
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

    void Grenade()
    {
        if (hasGrenades == 0)
            return;
        if(gDown && !isReload && !isDodge && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);//스크린에서 월드로 ray를 쏘는 함수.마우스로부터 월드로 레이를 쏴서 바닥에 닿는대!
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))//2번쨰 매개변수는 return처럼 반환ㄴ값을 주어진 변수에 저장하는 키워드임. 3번째 매개변수는 ray의 길이
            {
                Vector3 nextVec = rayHit.point - transform.position;  //rayHit.point는 ray가 바닥에 닿은 지점
                nextVec.y = 10;//수류탄을 좀 위로 던지기 위함

                GameObject instantGrenade = Instantiate(grenadeObj,transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back*10,ForceMode.Impulse);//회전

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
                
            }
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
                equipWeapon.gameObject.SetActive(false);
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
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
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();//item 스크립트 가져오기
                shop.Enter(this);
                isShop = true;

            }
        }
    }

    void FreezeRotation()//주기적으로 회전속도를 0으로 만들어 자동으로 돌아가는 것을 방지.
    {
        rigid.angularVelocity = Vector3.zero; //angularVelocity 는 물리 회전 속도를 의미

        /*또한 floor,player,탄피를 각각 다른 레이어로 설정해주고
         * edit>project setting> physics 의 맨 아래에서 어느 레이어가 어느 레이어에 충돌하는지를 설정해줌,
         * 이번 실습에서는 bulletCase는 floor에만 충돌하게 체크하여, case가 player에게 충돌하여 회전이 생기게 하는 것을 막음*/
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);//이작위치,쏘는방향*ray길이, ray색
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));//raycast 는 ray를 쏘아 닿는 오브젝트를 감지하는 함수임
        //Wall이라는 레이어를 가진 물체와 닿으면 bool값을 true로 바꿔줌,
    }
    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
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
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo) ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin) coin = maxCoin;
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
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {    
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAttack = other.name == "Boss Melee Area";//보스의 근접공격 오브젝트의 이름으로 보스의 공격을 인지
                StartCoroutine(OnDamage(isBossAttack));
            }
            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }       
    }

    IEnumerator OnDamage(bool isBossAttack)
    {
        isDamage = true;
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.yellow;
        }
        if (isBossAttack)
        {
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(1f);
        isDamage = false;
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.color = Color.white;
        }
        if (isBossAttack)
        {
            rigid.velocity = Vector3.zero;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon" || other.tag == "Shop")
        {
            nearObject = other.gameObject;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObject = null;
        }
        else if (other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }
}
