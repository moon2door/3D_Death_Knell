using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    Light flashlight;

    [Header("배터리 지속시간")]
    public float batteryLife = 15f; // 배터리 총 지속 시간 (초)
    public Transform player;

    [Header("배터리 이미지")]
    public Image[] batteryLevels; // [0] = 20%, ..., [4] = 100%
    public Image batteryFrame; // 틀 (항상 on)

    AudioSource myAudio;
    public AudioClip thalcak;

    public GameObject light_obj;


    private bool isDead = false;
    private float batteryTimer = 0f;

    private int currentStage = 5; // 시작은 5단계(100%)

    private void Awake()
    {
        List<Image> points = new List<Image>();

        for (int i = 0; i <= 4; i++) // 정확히 5개만 가져오기
        {
            string name = $"teel 0 ({i})";
            GameObject found = GameObject.Find(name);
            if (found != null)
            {
                Image img = found.GetComponent<Image>();
                if (img != null)
                    points.Add(img);
                else
                    Debug.LogWarning($"⚠️ {name} 오브젝트에는 Image 컴포넌트가 없습니다.");
            }
            else
            {
                Debug.LogWarning($"🔍 {name} 을(를) 찾을 수 없습니다.");
            }
        }

        batteryLevels = points.ToArray();

        // 배터리 틀 (프레임) 이미지도 같이 할당
        GameObject frameObj = GameObject.Find("teel 0");
        if (frameObj != null)
        {
            batteryFrame = frameObj.GetComponent<Image>();
        }
    }

    void Start()
    {
        flashlight = GetComponent<Light>();
        myAudio = GetComponent<AudioSource>();

        // ✅ 무조건 초기화해야 RPC에서 접근 가능
        if (light_obj == null)
        {
            light_obj = transform.Find("LightObj")?.gameObject; // 또는 수동으로 설정
        }

        StartCoroutine(FlahLightDebug());
    }

    void Update()
    {
        if (isDead)
            return;

        // 손전등 켜기/끄기
        if (Input.GetKeyDown(KeyCode.F))
        {
            myAudio.PlayOneShot(thalcak);
            flashlight.enabled = !flashlight.enabled;
            light_obj.SetActive(flashlight.enabled);

            PhotonView pv = transform.GetComponent<PhotonView>();
            if (pv != null)
            {
                pv.RPC("RPC_SyncFlashlight", RpcTarget.Others, flashlight.enabled);
            }

            SoundManager.Instance.EmitSound(transform.position, 6f);
        }

        // 배터리 소모
        if (flashlight.enabled)
        {
            batteryTimer += Time.deltaTime;
            if (batteryTimer >= batteryLife)
            {
                flashlight.enabled = false;
                isDead = true;
            }

            UpdateBatteryUI();

            // ✅ 손전등 빛이 닿는 적에게 알림
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 20f)) // 빛 최대 사거리
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    PhotonView myPV = GetComponentInParent<PhotonView>();
                    int myViewID = myPV.ViewID;

                    PhotonView enemyPV = hit.collider.GetComponent<PhotonView>();
                    enemyPV.RPC("RPC_FlashlightHit", RpcTarget.MasterClient, hit.point, myViewID);
                }
            }
        }
    }


    IEnumerator FlahLightDebug()
    {
        while (!isDead)
        {
            //Debug.Log((batteryLife - batteryTimer).ToString("F1") + "초 남음");

            yield return new WaitForSeconds(0.5f);
        }
    }

    void UpdateBatteryUI()
    {
        float percent = Mathf.Clamp01((batteryLife - batteryTimer) / batteryLife);
        int newStage = Mathf.CeilToInt(percent * 5 - 0.0001f); // 0~5

        if (newStage != currentStage)
        {
            for (int i = 0; i < batteryLevels.Length; i++)
            {
                if (batteryLevels[i] != null)
                {
                    batteryLevels[i].enabled = false;
                }
            }

            int index = newStage - 1;
            if (index >= 0 && index < batteryLevels.Length)
            {
                batteryLevels[index].enabled = true;
            }

            currentStage = newStage;
        }

        // ✅ 여기서 teel 0 (5) 제어
        GameObject warn = GameObject.Find("teel 0 (5)");
        if (warn != null)
        {
            Image img = warn.GetComponent<Image>();
            img.enabled = percent > 0.8f;  // 80% 초과일 때만 보이게
        }
    }


    /*
     
    IEnumerator FadeOutAndDisable(Image img)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Color originalColor = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Abs(Mathf.Sin(elapsed * 20f)); // 깜빡이는 느낌
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        img.enabled = false;
        img.color = originalColor; // 복원 (비활성화지만 혹시 몰라서)
    }

    */

    [PunRPC]
    public void RPC_FlashlightHit(Vector3 lightPos, int playerViewID)
    {
        GameObject playerObj = PhotonView.Find(playerViewID)?.gameObject;
        EnemyAI enemy = GameObject.Find("Enemy(Clone)").GetComponent<EnemyAI>();
        if (playerObj != null)
        {
            enemy.ReactToFlashlight(lightPos, playerObj.transform);
        }
    }

    [PunRPC]
    public void RPC_SyncFlashlight(bool state)
    {
        if (flashlight == null)
        {
            flashlight = GetComponent<Light>();
            if (flashlight == null)
            {
                Debug.LogError("❗ RPC: flashlight 컴포넌트를 찾을 수 없습니다.");
                return;
            }
        }

        if (light_obj == null)
        {
            Debug.LogWarning("❗ RPC: light_obj가 null입니다. 동기화에 실패할 수 있습니다.");
            return;
        }

        flashlight.enabled = state;
        light_obj.SetActive(state);
    }
}
