using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;//scene관련 함수를 사용하기 위해 필요함


public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Boss boss;
    public int stage;
    public float playTime;
    public bool isBattle;//지금 싸우고 있는지를 확인

    public AudioSource shopSound;
    public AudioSource stageSound;

    public int enemyCntA;//stage별 나오는 몬스터의 수
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Transform[] enemyZones;
    public GameObject[] enemies;
    public List<int> enemyList;//스테이지별 생성될 몬스터의 리스트

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject howToPlayPanel;
    public Player player;
    public GameObject overPanel;

    public Text curScoreText;
    public Text bestText;

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

    /* Zone 들*/
    public GameObject startZone;
    public GameObject itemShop;
    public GameObject weaponShop;
    void Awake()
    {
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
        enemyList = new List<int>();

        if (PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
    }

    public void HowToPlay()
    {
        howToPlayPanel.SetActive(true);
        menuPanel.SetActive(false);
    }
    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
        shopSound.Play();
        stageSound.Stop();
    }

    public void StageStart()
    {
        isBattle = true;
        startZone.SetActive(false);
        itemShop.SetActive(false);
        weaponShop.SetActive(false);

        foreach (Transform zone in enemyZones)
            zone.gameObject.SetActive(true);
        StartCoroutine(InBattle());
        stageSound.Play(); 
        shopSound.Stop();
    }

    public void StageEnd()
    {
        player.transform.position = Vector3.up * 0.8f;
        isBattle = false;
        startZone.SetActive(true);
        itemShop.SetActive(true);
        weaponShop.SetActive(true);

        foreach (Transform zone in enemyZones)
            zone.gameObject.SetActive(false);

        stage++;
        shopSound.Play();
        stageSound.Stop();

    }

    IEnumerator InBattle()
    {
        if (stage % 3 == 0)//3로 나눠떨어지는 stage마다 보스가 등장
        {
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[3], enemyZones[0].position, enemyZones[0].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;//enemy는 프리펩이라 scene의 object인 player에 접근 못함. 그래서 gameManager에서 target을 player로 넣어줌.
            enemy.manager = this;
            boss = instantEnemy.GetComponent<Boss>();
        }
        else
        {
            for (int i = 0; i < stage; i++)
            {
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;

                }
            }

            while (enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
                Enemy enemy = instantEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;//enemy는 프리펩이라 scene의 object인 player에 접근 못함. 그래서 gameManager에서 target을 player로 넣어줌.
                enemy.manager = this;
                enemyList.RemoveAt(0);//맨 첫번째 data 지워줌
                yield return new WaitForSeconds(4f);
            }
        }

        while (enemyCntA+ enemyCntB + enemyCntC + enemyCntD > 0 )//updata함수처럼 사용됨
        {
            yield return null;
        }
        yield return new WaitForSeconds(6f);
        boss = null;
        StageEnd();
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
        if(boss != null)
        {
            bossHealthGroup.anchoredPosition = Vector3.down * 30;
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        }
        else
        {
            bossHealthGroup.anchoredPosition = Vector3.up * 200;
        }
            

    }

    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreTxt.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");

        if (player.score > maxScore)
        {
            bestText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore",player.score);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
