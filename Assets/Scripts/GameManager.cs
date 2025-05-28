using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float interactableCount;
    public Text howmany;
    public GameObject isStart;
    public GameObject isClear;
    public GameStart gameStart;

    [Header("게임클리어시")]
    public AudioSource clearSource;
    public AudioClip clearClip;

    [Header("게임오버 UI")]
    public Image fadeImage; // 어두워지는 이미지 (투명도 조절용)
    public GameObject gameOverUI; // 이미지2 (게임오버 텍스트 등 포함)
    public Text gameOverText1; // A 텍스트 오브젝트 연결
    public Text gameOverText2;
    public Button quitB;

    [Header("게임클리어 이미지")]
    public Image clearStory;

    [Header("게임오브젝트")]
    public GameObject InGame;
    public GameObject OutGame;

    string[] gameOverMessages = new string[]
{
    "당신은 탈출에 실패했습니다...",
    "어둠이 당신을 삼켰습니다...",
    "희망은 사라졌습니다...",
    "끝이 보이지 않았습니다...",
    "모두가 죽었습니다..."
};


    public bool start;
    public bool clear;

    bool isTyping = false;
    bool isGameOver = false;


    void Update()
    {
        howmany.text = interactableCount.ToString("F0");

        // ✅ 모든 Interactable 처리 완료 시 OBJ 활성화/비활성화
        if (interactableCount == 0)
        {
            isStart.SetActive(false);
            isClear.SetActive(true);

            start = false;
            clear = true;
            clearSource.PlayOneShot(clearClip);
        }
        else
        {
            isStart.SetActive(true);
            isClear.SetActive(false);

            start = true;
            clear = false;
        }

        // ✅ 모든 플레이어가 죽었는지 확인
        if (AllPlayersDead() && !gameOverUI.activeSelf && gameStart.isPlaying && !isGameOver)
        {
            isGameOver = true;
            StartCoroutine(FadeOutAndShowGameOver());
        }
    }

    bool AllPlayersDead()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null && !pc.isDead)
                return false; // 살아있는 사람 있음
        }

        return true; // 전부 죽음
    }

    IEnumerator FadeOutAndShowGameOver()
    {
        float duration = 2f;
        float elapsed = 0f;

        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        fadeImage.gameObject.SetActive(true); // 반드시 켜줘야 보임

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, 1f);

        gameOverUI.SetActive(true);

        yield return new WaitForSeconds(1f);

        gameOverText1.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        

        string selectedMessage = gameOverMessages[gameStart.randomRange];

        // ✅ 타이핑 효과 실행
        StartCoroutine(TypeGameOverMessage(selectedMessage));

        yield return new WaitForSeconds(1f);

        quitB.gameObject.SetActive(true);
    }

    IEnumerator TypeGameOverMessage(string message, float typingSpeed = 0.05f)
    {
        if (isTyping) yield break; // 중복 방지
        isTyping = true;

        gameOverText2.text = "";

        foreach (char letter in message)
        {
            gameOverText2.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public IEnumerator FadeOutOnClear()
    {
        float duration = 2f;
        float elapsed = 0f;

        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        fadeImage.gameObject.SetActive(true); // 반드시 켜줘야 보임

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, 1f);

        clearStory.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        InGame.SetActive(false);
        OutGame.SetActive(true);

        GameObject player = GameObject.Find("Player(Clone)");
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");

        player.SetActive(false);
        enemy.SetActive(false);
    }


    [PunRPC]
    public void RPC_DecreaseCount()
    {
        interactableCount--;
    }

    public void _Click_Out()
    {
        Application.Quit();
    }
}
