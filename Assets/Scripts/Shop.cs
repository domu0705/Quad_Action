using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{

    public RectTransform uiGroup;
    public Animator anim;
    public GameObject[] itemObj;
    public int[] itemPrice;
    public Transform[] itemPos;
    public Text talkText;
    public string[] talkData;
    Player enterPlayer;//상점에 들어온 플레이어 정보 받기
    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero;//UI가 화면 정중앙에 오도록.
    }

    // Update is called once per frame
    public void Exit()
    {
        anim.SetTrigger("doHello");
        uiGroup.anchoredPosition = Vector3.down*1000;//UI가 화면밖으로 내려져 사라지도록.
    }

    public void Buy(int index)
    {
        int price = itemPrice[index];
        if(price > enterPlayer.coin)
        {
            StopCoroutine(Talk());
            StartCoroutine(Talk());
            return;
        }
        enterPlayer.coin -= price;
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3) + Vector3.forward * Random.Range(-3, 3);
        Instantiate(itemObj[index], itemPos[index].position + ranVec, itemPos[index].rotation);

    }

    IEnumerator Talk()//돈이 모라=자랄 때
    {
        talkText.text = talkData[1];
        yield return new WaitForSeconds(2f);
        talkText.text = talkData[0];
    }
}
