using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range} // Melee 는 근접공격, 
    public Type type;
    public int damage;//무기의 공격력
    public float rate;//공격 속도
    public BoxCollider meleeArea; //근접 공격의 범위
    public TrailRenderer trailEffect;//무기 휘두를 때 효과


   public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing"); // 코루틴을 끝내는 함수. 이전에 진행중인 코루틴과 섞이지 않게하는,,그런거래
            StartCoroutine("Swing");//코루틴을 시작하는 함수
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
        yield return new WaitForSeconds(0.1f); //입력한 시간만큼 대기.
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        yield return new WaitForSeconds(0.3f); //입력한 시간만큼 대기.
        meleeArea.enabled = false;
        yield return new WaitForSeconds(0.3f); //입력한 시간만큼 대기.
        trailEffect.enabled = false;
    }
    
}
