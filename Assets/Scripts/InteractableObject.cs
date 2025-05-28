using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

public class InteractableObject : MonoBehaviourPun
{
    private Renderer rend;
    private bool isActivated = false;

    public EnemyAI enemy; // 적 스크립트 연결 (호스트만 보유)
    public GameManager gameManager;

    AudioSource myAudio;
    public AudioClip bellSound;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = Color.white;

        myAudio = GetComponent<AudioSource>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", new Color(160f / 255f, 0f, 0f) * 0f);

        if (PhotonNetwork.IsMasterClient)
        {
            if (enemy == null)
            {
                GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
                if (enemyObj != null)
                    enemy = enemyObj.GetComponent<EnemyAI>();
                else
                    Debug.LogWarning("❗ Enemy 태그 오브젝트를 찾을 수 없습니다.");
            }
        }
    }

    public void Interact()
    {
        if (!isActivated)
        {
            isActivated = true;

            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", new Color(1f, 0f, 0f) * 1f);

            myAudio.PlayOneShot(bellSound);

            if (PhotonNetwork.IsMasterClient)
            {
                if (enemy != null)
                {
                    enemy.MoveToPointAround(transform.position, 2f, 7f);
                }
                    
            }
            else
            {
                photonView.RPC("RPC_TriggerEnemy", RpcTarget.MasterClient, transform.position);
            }

            if (gameManager != null)
            {
                PhotonView gmPV = gameManager.GetComponent<PhotonView>();
                if (gmPV != null)
                {
                    gmPV.RPC("RPC_DecreaseCount", RpcTarget.All);
                }
                else
                {
                    Debug.LogError("❗ GameManager에 PhotonView가 없습니다.");
                }
            }
        }
    }

    [PunRPC]
    void RPC_TriggerEnemy(Vector3 pos)
    {
        if (enemy == null)
        {
            GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
            if (enemyObj != null)
                enemy = enemyObj.GetComponent<EnemyAI>();
        }

        if (enemy != null)
        {
            enemy.MoveToPointAround(pos, 2f, 5f);
        }
        else
        {
            Debug.LogWarning("❗ Enemy not found on MasterClient when RPC_TriggerEnemy called.");
        }
    }
}
