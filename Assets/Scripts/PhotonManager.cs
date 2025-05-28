//#define On

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public GameObject SpawnLocation;
    public GameObject enemySpawnPoint;

    public static bool isReady = false;

    [Header("문양이 생성될 위치들 (11개)")]
    public Transform[] spawnPoints; // 씬에서 미리 연결

    private void Awake()
    {
        PhotonNetwork.NickName = "Ban_si";
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

#if On
        Debug.Log(PhotonNetwork.SendRate);
#endif
    }

    public override void OnConnectedToMaster()
    {
#if On
        Debug.Log("Connected to Master!");
#endif
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
#if On
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
#endif
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
#if On
        Debug.Log($"JoinRoom Failed {returnCode}:{message}");
#endif
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 20;
        ro.IsOpen = true;
        ro.IsVisible = true;
        PhotonNetwork.CreateRoom("My Room", ro);
    }

    public override void OnCreatedRoom()
    {
#if On
        Debug.Log("Created Room");
        Debug.Log($"Room Name = {PhotonNetwork.CurrentRoom.Name}");
#endif
    }

    public override void OnJoinedRoom()
    {
        isReady = true;

#if On
        Debug.Log($"PhotonNetwork.InRoom = {PhotonNetwork.InRoom}");
        Debug.Log($"Player Count = {PhotonNetwork.CurrentRoom.PlayerCount}");
#endif
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            //Debug.Log($"{player.Value.NickName}, {player.Value.ActorNumber}");
        }
    }

    // ✅ 게임 시작 버튼에서 호출될 함수
    public void SpawnPlayer()
    {
        PhotonNetwork.Instantiate("Player", SpawnLocation.transform.position, SpawnLocation.transform.rotation, 0);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Enemy", enemySpawnPoint.transform.position, enemySpawnPoint.transform.rotation, 0);
            
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[i];
                if (point != null)
                {
                    PhotonNetwork.Instantiate("MunyangPrefab", point.position, point.rotation);
                }
            }
        }
    }
}
