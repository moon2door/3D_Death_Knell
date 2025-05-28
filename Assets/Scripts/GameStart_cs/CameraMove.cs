using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;     // 기준이 되는 오브젝트 (A 오브젝트)
    public float orbitSpeed = 10f; // 초당 공전 속도 (도 단위)

    public Vector3 orbitAxis = Vector3.up; // 공전하는 축 (Y축 기준으로 회전)

    void Update()
    {
        if (target == null) return;

        // target.position을 중심으로 orbitAxis 방향으로 회전
        transform.RotateAround(target.position, orbitAxis, orbitSpeed * Time.deltaTime);

        // 항상 target을 바라보게 만듦
        transform.LookAt(target);
    }
}
