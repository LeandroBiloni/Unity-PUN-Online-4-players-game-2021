using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Server : MonoBehaviourPun
{
    public static Server instance;

    private Player _server;

    public Character characterPrefab;

    public CharacterSpawn spawns;
    private Dictionary<Player, Character> _dicModels = new Dictionary<Player, Character>();

    public int PackagesPerSecond
    {
        get;
        private set;
    }
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            if (photonView.IsMine)
            {
                spawns = FindObjectOfType<CharacterSpawn>();
                if (!spawns)
                {
                    StartCoroutine(SearchSpawns());
                }
                else
                {
                    Debug.Log("spawn en el start");
                }
                photonView.RPC("SetServer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, 1);
            }
        }
    }

    IEnumerator SearchSpawns()
    {
        while (!spawns)
        {
            spawns = FindObjectOfType<CharacterSpawn>();
            yield return new WaitForEndOfFrame();
        }
        
        Debug.Log("consegui spawn");
    }

    [PunRPC]
    void SetServer(Player serverPlayer, int sceneIndex = 1)
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        _server = serverPlayer;

        PackagesPerSecond = 60;
        PhotonNetwork.LoadLevel(sceneIndex);
        
        var playerLocal = PhotonNetwork.LocalPlayer;

        if (playerLocal != _server)
        {
            photonView.RPC("AddPlayer", _server, playerLocal);
        }
    }

    [PunRPC]
    void AddPlayer(Player player)
    {
        StartCoroutine(WaitForLevel(player));
    }

    IEnumerator WaitForLevel(Player player)
    {
        while (PhotonNetwork.LevelLoadingProgress > 0.9f)
        {
            yield return new WaitForEndOfFrame();
        }

        var pos = PhotonNetwork.PlayerList.Length - 2;

        Character newCharacter = PhotonNetwork.Instantiate(characterPrefab.name, spawns.spawns[pos].position, spawns.spawns[pos].rotation).GetComponent<Character>();
        newCharacter.transform.Rotate(newCharacter.transform.up, 90.0f);
        newCharacter.SetInitialParameters(player);
        _dicModels.Add(player, newCharacter);

    }

    public void RequestMove(Player player, Vector3 dir)
    {
        photonView.RPC("Move", _server, player, dir);
    }

    [PunRPC]
    void Move(Player player, Vector3 dir)
    {
        if (_dicModels.ContainsKey(player))
        {
            _dicModels[player].Move(dir);
        }
    }
    
    public void RequestJump(Player player)
    {
        photonView.RPC("Jump", _server, player);
    }

    [PunRPC]
    void Jump(Player player)
    {
        if (_dicModels.ContainsKey(player))
        {
            _dicModels[player].Jump();
        }
    }

    public void RequestShoot(Player player, Vector3 dir)
    {
        photonView.RPC("Shoot", _server, player, dir);
    }

    [PunRPC]
    void Shoot(Player player, Vector3 dir)
    {
        if (_dicModels.ContainsKey(player))
        {
            _dicModels[player].Shoot(dir);
        }
    }
}
