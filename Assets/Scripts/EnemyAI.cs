using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("참조")]
    private NavMeshAgent agent;
    private Transform player;
    public Transform eye;
    public Light flashlight;
    public PlayerController playerController;
    public GameObject player_G;


    private PhotonView pv;

    public Animator myAnim;

    [Header("시야각")]
    public float viewDistance;
    public float viewAngle;

    private float timeSinceLastSeen = Mathf.Infinity;
    public float chaseMemoryTime = 5f;

    [Header("순찰 포인트 배열")]
    public Transform[] patrolPoints;
    public float patrolDelay;

    [Header("이동속도")]
    public float normalSpeed;
    public float chaseSpeed;
    public float rushSpeed;

    [Header("발소리")]
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;
    public float interval = 0.7f;
    private Coroutine footstepCoroutine;

    [Header("숨소리")]
    public AudioSource howlingaudioSource;
    public AudioClip[] howlingSound;
    public float howlinginterval = 0.7f;
    private Coroutine howlingCoroutine;

    [Header("재생시간")]
    float soundHearRange = 12f;

    private float patrolTimer = 0f;

    public bool isChasing = false;
    private bool isWaiting = false;
    private bool isFinding = false;
    

    private bool isReturningFromRush = false;
    private Vector3 targetPosition;

    void Awake()
    {
        // 자동으로 "point 1 (0)" ~ "point 1 (16)" 검색
        List<Transform> points = new List<Transform>();

        for (int i = 0; i <= 16; i++)
        {
            string name = $"point 1 ({i})";
            GameObject obj = GameObject.Find(name);
            if (obj != null)
                points.Add(obj.transform);
            else
                Debug.LogWarning($"🔍 {name} 을(를) 찾을 수 없습니다.");
        }

        patrolPoints = points.ToArray();
    }


    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (!pv.IsMine)
        {
            // AI, NavMeshAgent, Update 등 비활성화
            enabled = false;
            return;
        }

        agent = GetComponent<NavMeshAgent>();
        footstepCoroutine = StartCoroutine(PlayFootsteps());
        howlingCoroutine = StartCoroutine(PlayHowling());

        myAnim.SetBool("walk", false);
        myAnim.SetBool("where", false);
    }

    void Update()
    {
        if (playerController == null)
        {
            playerController = GameObject.Find("Player(Clone)").GetComponent<PlayerController>();
            player_G = GameObject.Find("Player(Clone)");
            //Debug.Log("playerController 배치중..");
        }

        // 플레이어 탐지 시도
        Transform visiblePlayer = FindVisiblePlayer();
        if (visiblePlayer != null)
        {
            // hp가 0 이하일 경우 → 시야에 보이더라도 추적하지 않음
            if (playerController != null && playerController.isDead)
            {
                // 플레이어는 존재하지만 사망한 상태 → 무시
                player = null;
            }
            else
            {
                player = visiblePlayer;
                timeSinceLastSeen = 0f;
            }
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
        }

        // 소리 감지
        Vector3? soundPos = SoundManager.Instance.GetRecentSoundNear(transform.position, soundHearRange);
        if (soundPos != null && !isChasing && !isFinding)
        {
            MoveToPointAround(soundPos.Value, 0.5f, 2f);
            //Debug.Log("👂 몬스터가 발소리를 들었다!");
        }

        // 속도 설정
        if (isChasing) agent.speed = chaseSpeed;
        else if (isFinding) agent.speed = rushSpeed;
        else agent.speed = normalSpeed;

        myAnim.speed = agent.speed / normalSpeed;

        if (player != null && timeSinceLastSeen < chaseMemoryTime)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            agent.SetDestination(player.position);
            agent.speed = chaseSpeed;
            isChasing = true;
            isWaiting = false;
            patrolTimer = 0f;

            if (distanceToPlayer < 2f)
            {
                myAnim.SetBool("attack", true);
                agent.isStopped = true;

                PhotonView targetPV = player_G.GetComponent<PhotonView>();
                if (targetPV != null)
                {
                    PlayerController pc = player_G.GetComponent<PlayerController>();

                    if (pc != null && pc.isDead == true)
                    {
                        //Debug.Log("❌ 대상이 이미 사망 상태. 공격 중단");
                        myAnim.SetBool("attack", false);    // ✅ 애니메이션 정지
                        agent.isStopped = false;            // ✅ 이동 가능 상태 복구
                        return;
                    }
                    else
                    {
                        //Debug.Log("으아아 공격이 안멈춰");
                    }

                    //Debug.Log("데미지 넣는중 20");
                    targetPV.RPC("RPC_TakeDamage", targetPV.Owner, 20f);
                }

                else
                {
                    //Debug.Log("타겟pv == null");
                }
            }
            else
            {
                myAnim.SetBool("attack", false);
                agent.isStopped = false;
            }

            myAnim.SetBool("walk", true);
            myAnim.SetBool("where", false);
            return;
        }

        // 플레이어 본지 오래됨 → 순찰 전환
        isChasing = false;

        if (isReturningFromRush && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isReturningFromRush = false;
            isFinding = false;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;
            myAnim.SetBool("walk", false);
            myAnim.SetBool("where", true);

            if (patrolTimer >= patrolDelay && patrolPoints.Length > 0)
            {
                int index = Random.Range(0, patrolPoints.Length);
                Vector3 targetPos = patrolPoints[index].position;

                if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    patrolTimer = 0f;
                    isWaiting = false;

                    myAnim.SetBool("walk", true);
                    myAnim.SetBool("where", false);
                }
            }
        }
    }

    Transform FindVisiblePlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject obj in players)
        {
            Transform target = obj.transform;
            Vector3 dirToTarget = target.position - eye.position;
            float angle = Vector3.Angle(eye.forward, dirToTarget);

            if (angle < viewAngle * 0.5f && dirToTarget.magnitude < viewDistance)
            {
                Ray ray = new Ray(eye.position, dirToTarget.normalized);
                if (Physics.Raycast(ray, out RaycastHit hit, viewDistance))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        flashlight.spotAngle = 360f;
                        return target;
                    }
                }
            }
        }

        flashlight.spotAngle = 60f;
        return null;
    }

    public void MoveToPointAround(Vector3 center, float minDistance = 2f, float maxDistance = 7f)
    {
        Vector3 result;

        NavMesh.SamplePosition(center, out NavMeshHit centerHit, 2f, NavMesh.AllAreas);
        center = centerHit.position;

        if (GetRandomPoint(center, minDistance, maxDistance, out result))
        {
                myAnim.SetBool("walk", true);
                myAnim.SetBool("where", false);

                isFinding = true;
                agent.SetDestination(result);
                isChasing = false;
                isReturningFromRush = true;
                targetPosition = result;
        }
    }

    bool GetRandomPoint(Vector3 center, float minDistance, float maxDistance, out Vector3 result)
    {
        for (int i = 0; i < 100; i++)
        {
            float distance = Random.Range(minDistance, maxDistance);
            float angle = Random.Range(0f, 360f);
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 randomPoint = center + dir.normalized * distance;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }

    IEnumerator PlayFootsteps()
    {
        while (true)
        {
            if (myAnim.GetBool("walk") && agent.velocity.magnitude > 0.1f)
            {
                int index = Random.Range(0, footstepSounds.Length);
                audioSource.PlayOneShot(footstepSounds[index]);
            }

            float adjustedInterval = interval / Mathf.Max(myAnim.speed, 0.1f);
            yield return new WaitForSeconds(adjustedInterval);
        }
    }

    IEnumerator PlayHowling()
    {
        while (true)
        {
            if (!isChasing)
            {
                int index = Random.Range(0, howlingSound.Length);
                howlingaudioSource.PlayOneShot(howlingSound[index]);
            }
            yield return new WaitForSeconds(howlinginterval);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (eye == null) return;
        Gizmos.color = Color.yellow;
        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * eye.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * eye.forward;
        Gizmos.DrawRay(eye.position, left * viewDistance);
        Gizmos.DrawRay(eye.position, right * viewDistance);
    }

    public void ReactToFlashlight(Vector3 lightHitPoint, Transform player)
    {
        if (!isChasing && !isFinding)
        {
            MoveToPointAround(player.position, 1f, 2f);
        }
    }

    [PunRPC]
    public void RPC_FlashlightHit(Vector3 lightPos, int playerViewID)
    {
        GameObject playerObj = PhotonView.Find(playerViewID)?.gameObject;
        if (playerObj != null)
        {
            ReactToFlashlight(lightPos, playerObj.transform);
        }
    }


}
