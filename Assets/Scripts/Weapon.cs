﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range} // Melee 는 근접공격, 
    public Type type;
    public int damage;//무기의 공격력
    public float rate;//공격 속도
    public int maxAmmo;
    public int curAmmo;

    public BoxCollider meleeArea; //근접 공격의 범위
    public TrailRenderer trailEffect;//무기 휘두를 때 효과
    public Transform bulletPos;//총알의 시작 위치
    public GameObject bullet;//총알 프리펩을 저장할 변수
    public Transform bulletCasePos;//탄피의 시작 위치
    public GameObject bulletCase;//탄피 프리펩을 저장할 변수

    /*효과음들*/
    public AudioSource gunSound;
    public AudioSource hammerSound;
    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing"); // 코루틴을 끝내는 함수. 이전에 진행중인 코루틴과 섞이지 않게하는,,그런거래
            StartCoroutine("Swing");//코루틴을 시작하는 함수
        }
        if ((type == Type.Range) &&  curAmmo >0)
        {
            curAmmo--;
            //StopCoroutine("Swing"); // 코루틴을 끝내는 함수. 이전에 진행중인 코루틴과 섞이지 않게하는,,그런거래
            StartCoroutine("Shot");//코루틴을 시작하는 함수
        }
    }

    /* 코루틴이란? 
        일반적인 순서 : use() - > swing() -> Use() 
        use 메인루틴에서 swing을 호출
        이때 swing은 서브루틴. 
        swing이 끝나면 다시 use로 감.

        코루틴 : 메인루틴과 서브루틴이 동시에 함께 실행 됨.
        use() + swing()
    */

    /*yield란?
     결과를 전달하는 키워드.
     코루틴은 이 yeild가 한개 이상 꼭 필요함.
     여기서 1 frame을 쉼-> yield 키워드를 여러개 써서 시간차 로직 작성 가능함.
     yield break로 코루틴 탈출 가능
      */
    IEnumerator Swing() // 열거형 함수 클래스
    {
        hammerSound.Play();
        yield return new WaitForSeconds(0.1f); //입력한 시간만큼 대기.
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        yield return new WaitForSeconds(0.3f); //입력한 시간만큼 대기.
        meleeArea.enabled = false;
        yield return new WaitForSeconds(0.3f); //입력한 시간만큼 대기.
        trailEffect.enabled = false;
    }
    IEnumerator Shot() // 열거형 함수 클래스
    {
        /*1. 총알 발사*/
        GameObject instantBullet = Instantiate(bullet,bulletPos.position,bulletPos.rotation);//프리펩을 만들어 준다
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;//총알이 나가야하니까 속도를 붙여줌
        gunSound.Play();
        yield return null; //입력한 시간만큼 대기.
        //2. 탄피 배출
        GameObject instantCaseBullet = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);//프리펩을 만들어 준다
        Rigidbody bulletCaseRigid = instantCaseBullet.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);//총알이 나가야하니까 속도를 붙여줌
        bulletCaseRigid.AddForce(caseVec, ForceMode.Impulse);
        bulletCaseRigid.AddForce(Vector3.up * 10, ForceMode.Impulse);
    }

}
