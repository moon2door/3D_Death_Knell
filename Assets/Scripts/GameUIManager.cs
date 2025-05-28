using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameHUD;

    public GameObject howPlayImage;
    public Image start_Game_Tutorial;

    public AudioSource button;
    public AudioClip button_clip;

    public void OnGameStartClicked()
    {
        button.PlayOneShot(button_clip);

        if (!PhotonManager.isReady)
        {
            Debug.LogWarning("❗ Photon 준비 중... 플레이어 생성 대기 중입니다.");
            StartCoroutine(WaitThenStart());
            return;
        }

        StartGame();
    }

    IEnumerator WaitThenStart()
    {
        while (!PhotonManager.isReady)
            yield return null;

        StartGame();
    }

    void StartGame()
    {
        startPanel.SetActive(false);
        gameHUD.SetActive(true);

        PhotonManager pm = FindObjectOfType<PhotonManager>();
        pm.SpawnPlayer();
    }


    public void _Click_HowPlay()
    {
        button.PlayOneShot(button_clip);

        howPlayImage.SetActive(true);
    }

    public void _Click_HowPlay_X()
    {
        button.PlayOneShot(button_clip);

        howPlayImage.SetActive(false);
    }

    public void _Click_GameEnd()
    {
        button.PlayOneShot(button_clip);

        Application.Quit();
    }

    public void _Click_PressStart()
    {
        button.PlayOneShot(button_clip);

        start_Game_Tutorial.gameObject.SetActive(true);
    }
}
