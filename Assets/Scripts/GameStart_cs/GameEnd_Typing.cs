using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEnd_Typing : MonoBehaviour
{
    bool isTyping = false;
    bool isTypingEnabled = false;

    public Text game_startT1;
    public AudioSource typeS;
    public AudioClip typeC;

    string gameStartMessages = 
        "문이 열리자, 나는 망설임도 없이 달려 나갔다." +
        "\r\n\r\n\r\n\r\n뒤를 돌아볼 틈도 없이... 숨이 차오르는 것도 잊은 채." +
        "\r\n\r\n\r\n\r\n저택은... 여전히 그 자리에 있었다." +
        "\r\n\r\n\r\n\r\n마치 아무 일도 없었다는 듯이.";

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
