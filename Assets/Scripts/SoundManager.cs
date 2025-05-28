using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private struct SoundData
    {
        public Vector3 position;
        public float range;
        public float timestamp;
    }

    private List<SoundData> soundList = new List<SoundData>();
    public float soundLifetime = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    public void EmitSound(Vector3 position, float range)
    {
        if (soundList == null)
        {
            Debug.LogError("❗ soundList가 아직 초기화되지 않았습니다!");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // 마스터는 로컬에 바로 추가
            soundList.Add(new SoundData { position = position, range = range, timestamp = Time.time });
        }
        else
        {
            // 마스터에게 발소리 전달
            PhotonView pv = PhotonView.Get(this);
            pv.RPC("RPC_EmitSound", RpcTarget.MasterClient, position, range);
        }
    }

    public Vector3? GetRecentSoundNear(Vector3 listenerPosition, float hearRange)
    {
        float now = Time.time;
        foreach (var sound in soundList)
        {
            if (now - sound.timestamp > soundLifetime) continue;

            float dist = Vector3.Distance(listenerPosition, sound.position);
            if (dist <= sound.range && dist <= hearRange)
            {
                return sound.position;
            }
        }
        return null;
    }

    [PunRPC]
    public void RPC_EmitSound(Vector3 position, float range)
    {
        soundList.Add(new SoundData
        {
            position = position,
            range = range,
            timestamp = Time.time
        });
    }

}
