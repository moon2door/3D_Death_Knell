using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float hp = 100f;
    public Transform cameraTransform;
    public Image bloodVignette; // Inspector 연결 필요

    private CharacterController controller;
    private float xRotation = 0f;
    private bool isInvincible = false; // 무적 여부

    private float timeSinceLastDamage = 0f;
    private float healCooldown = 10f; // 10초마다 회복

    private bool isClearing = false;


    public bool isDead = false;
    public bool isClear = false;

    // ✅ 중력 관련 변수
    public float gravity = -9f;
    private float verticalVelocity = 0f;

    [Header("발소리")]
    AudioSource audioSource;
    public AudioClip[] footstepSounds;
    public float interval = 0.7f;
    private Coroutine footstepCoroutine;

    PhotonView myPV;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        myPV = GetComponent<PhotonView>();
        bloodVignette = GameObject.Find("BloodVignette").GetComponent<Image>();

        cameraTransform.gameObject.SetActive(myPV.IsMine);
        cameraTransform.GetComponent<AudioListener>().enabled = myPV.IsMine;

        if (myPV.IsMine)
        {
            footstepCoroutine = StartCoroutine(PlayFootsteps());
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Update()
    {
        if (!myPV.IsMine) return;
        if (isDead) return;
        if (isClear) return;

        // 마우스 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);

        // 중력 적용
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // ✅ 이동 + Shift로 속도 조절
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool isSneaking = Input.GetKey(KeyCode.LeftShift);

        float speed = isSneaking ? moveSpeed * 0.5f : moveSpeed;

        Vector3 move = transform.right * h + transform.forward * v;
        move.y = verticalVelocity;

        controller.Move(move * speed * Time.deltaTime);

        timeSinceLastDamage += Time.deltaTime;

        // ✅ 10초 이상 지났고 체력이 100 미만일 경우 회복
        if (timeSinceLastDamage >= healCooldown && hp < 100f)
        {
            hp += 20f;
            hp = Mathf.Min(hp, 100f); // 최대 100 제한
            timeSinceLastDamage = 0f; // 타이머 초기화
            //Debug.Log($"❤️ 체력 회복됨: {hp}");
        }

        UpdateBloodVignette();
    }

    void UpdateBloodVignette()
    {
        Color c = bloodVignette.color;

        if (hp >= 100)
            c.a = 0f;
        else if (hp >= 80)
            c.a = 0.1f;
        else if (hp >= 60)
            c.a = 0.2f;
        else if (hp >= 40)
            c.a = 0.35f;
        else if (hp >= 20)
            c.a = 0.6f;
        else
            c.a = 0.9f;

        bloodVignette.color = new Color(c.r, c.g, c.b, c.a);
    }

    IEnumerator PlayFootsteps()
    {
        while (true)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
            bool isSneaking = Input.GetKey(KeyCode.LeftShift);

            if (isMoving && controller.isGrounded && !isSneaking)
            {
                int index = Random.Range(0, footstepSounds.Length);
                audioSource.PlayOneShot(footstepSounds[index]);
                SoundManager.Instance.EmitSound(transform.position, 12f);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage)
    {
        if (isInvincible) return;

        hp -= damage;
        timeSinceLastDamage = 0f;
        //Debug.Log($"🩸 HP 감소됨: {hp}");

        if (hp <= 0f)
        {
            isDead = true;
            gameObject.tag = "PlayerDead";
            StartCoroutine(RotateZOverTime(90f, 2f)); // ✅ Z축으로 90도 회전
        }

        StartCoroutine(InvincibilityCoroutine());
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(1f); // 1초간 무적
        isInvincible = false;
    }

    private IEnumerator RotateZOverTime(float angle, float duration)
    {
        Quaternion startRotation = transform.rotation;

        // ✅ 오른쪽 방향 기준으로 회전하는 쿼터니언 계산
        Vector3 rightAxis = -transform.right; // 플레이어가 바라보는 방향 기준 오른쪽
        Quaternion endRotation = Quaternion.AngleAxis(angle, rightAxis) * startRotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t); // 부드러운 회전
            yield return null;
        }

        transform.rotation = endRotation;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Clear") && !isClearing)
        {
            isClearing = true;
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.StartCoroutine(gm.FadeOutOnClear()); // ✅ GameManager의 페이드 코루틴 실행

                isClear = true;
            }
        }
    }
}
