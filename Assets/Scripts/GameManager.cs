using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Boss boss;
    public int stage;
    public float playTime;
    public bool isBattle;//지금 싸우고 있는지를 확인
    public int enemyCntA;//몬스터의 수
    public int enemyCntB;
    public int enemyCntC;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public Player player;

    /*menuPanel에 나올 최고기록*/
    public Text maxScoreTxt;

    /*game play 중 나올 UI정보들*/
    public Text scoreTxt;
    public Text stageTxt;
    public Text playTImeTxt;

    public Text playerHealthTxt;
    public Text playerAmmoTxt;
    public Text playerCoinTxt;

    /*중앙 하단의 4가지 무기이미지*/
    public Image weapon1Img;
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;

    /*우측하단의 몬스터 수*/
    public Text enemyATxt;
    public Text enemyBTxt;
    public Text enemyCTxt;

    /*보스 몬스터 체력 바*/
    public RectTransform bossHealthGroup;
    public RectTransform bossHealthBar;


    void Awake()
    {
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));    
    }

    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }

    void Update()
    {
        if (isBattle)
        {
            playTime += Time.deltaTime;
        }   
    }
    private void LateUpdate()//update()가 끝난 후 호출되는 생명주기. 이미 처리된 정보를 단순히 표시만 해줄거니까 UI로직에 안성맞춤! 
    {
        scoreTxt.text = string.Format("{0:n0}", player.score);
        stageTxt.text = "STAGE"+stage;

        /*play time*/
        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour*3600) / 60);
        int second = (int)(playTime %60);
        playTImeTxt.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);

        /*health, coin, ammo*/
        playerHealthTxt.text = player.health + "/" + player.maxHealth;
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);
        if (player.equipWeapon == null)
            playerAmmoTxt.text = "- /" + player.ammo;
        else if (player.equipWeapon.type == Weapon.Type.Melee)
            playerAmmoTxt.text = "- /" + player.ammo;
        else
            playerAmmoTxt.text = player.equipWeapon.curAmmo + "/" + player.ammo;

        /*무기 아이콘은 보유 상황에 따라 알파값을 변경*/
        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades>0 ? 1 : 0);

        /*몬스터 수*/
        enemyATxt.text = "x" + enemyCntA.ToString();
        enemyBTxt.text = "x" + enemyCntB.ToString();
        enemyCTxt.text = "x" + enemyCntC.ToString();

        /*boss 체력바*/
        bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);

    }
}
