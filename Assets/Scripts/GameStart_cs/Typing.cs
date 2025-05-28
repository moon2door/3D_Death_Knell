using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Typing : MonoBehaviour
{
    bool isTyping = false;
    bool isTypingEnabled = false;

    public Text game_startT1;
    public Text game_startT2;

    public AudioSource typeS;
    public AudioClip typeC;

    string gameStartMessages = 
        "흉흉한 이야기가 들려오는 저택이 있었다." +
        "\r\n\r\n\r\n나는 그 저택을 Y튜브 콘텐츠로 다루기 위해 찾아갔고," +
        "\r\n\r\n\r\n안으로 들어선 순간... 갑자기 문이 닫혀버렸다.";

    public Image clearImage;

    string clearMessages = "이렇게 생긴 11개의 문양을 \r\n활성화 시켜야 문이 열릴 것 같다...";

    public Button nextButton;

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.activeSelf && !isTypingEnabled)
        {
            isTypingEnabled = true;
            StartCoroutine(StoryLine());
        }
    }

    IEnumerator StoryLine()
    {
        yield return StartCoroutine(TypeGameOverMessage(gameStartMessages, game_startT1)); // ✅ 타이핑 끝날 때까지 기다림

        yield return new WaitForSeconds(1f);
        clearImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(TypeGameOverMessage(clearMessages, game_startT2)); // ✅ 두 번째 타이핑

        yield return new WaitForSeconds(1f);
        nextButton.gameObject.SetActive(true);
    }


    IEnumerator TypeGameOverMessage(string message, Text targetText, float typingSpeed = 0.05f)
    {
        typeS.PlayOneShot(typeC);

        if (isTyping) yield break; // 중복 방지
        isTyping = true;

        targetText.text = "";

        foreach (char letter in message)
        {
            targetText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        typeS.Stop();
    }
}
