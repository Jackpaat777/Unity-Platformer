using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Game Manager : ����, ��������, HP ����
public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public PlayerMove player;
    public GameObject[] Stages;
    public Image[] UIhealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;
    public GameObject menuSet;

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();

        if (Input.GetButtonDown("Cancel"))
        {
            // �̹� �������� �� ESC ��ư�� ������ ����
            if (menuSet.activeSelf)
            {
                Time.timeScale = 1;
                menuSet.SetActive(false);
            }
            else
            {
                Time.timeScale = 0;
                menuSet.SetActive(true);
            }
        }
    }
    public void NextStage()
    {
        if (stageIndex < Stages.Length - 1)
        {
            // Change Stage
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            // Player Reposition
            PlayerReposition();

            UIStage.text = "Stage " + (stageIndex + 1);
        }
        // Game Clear
        else
        {
            // Player Control Rock
            Time.timeScale = 0;
            // ��ư �ؽ�Ʈ�� ��ư�ȿ� �����Ƿ� GetComponentInChildren�� ���
            Text btnText = UIRestartBtn.GetComponentInChildren<Text>();
            btnText.text = "Clear!";
            // Restart Button UI
            UIRestartBtn.SetActive(true);
        }

        // Calculate Point
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIhealth[health].color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        }
        else
        {
            // All Health UI Off
            UIhealth[0].color = new Color(0.2f, 0.2f, 0.2f, 0.6f);

            // Player Die Effect
            player.OnDie();
            // Retry Button UI
            UIRestartBtn.SetActive(true);
        }
    }

    // ���� �������� ��
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // ����� �����ִ� ��쿡�� ����ġ
            if (health > 1)
            {
                // Player RePosition
                PlayerReposition();
            }

            // Health Down
            HealthDown();
        }
    }

    void PlayerReposition()
    {
        player.VelocityZero();
        player.transform.position = new Vector3(0, 0, 0);
    }

    public void Restart()
    {
        // �ð����� Ǯ��
        Time.timeScale = 1;
        // ȭ�� �����
        SceneManager.LoadScene(0);
    }

    public void GameExit()
    {
        Application.Quit();
    }
}
