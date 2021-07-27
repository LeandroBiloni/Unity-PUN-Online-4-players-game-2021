using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Server : MonoBehaviourPunCallbacks
{
    public static Server instance;

    private Player _server;

    public Character characterPrefab;

    public CharacterSpawn spawns;

    public ChatManager chatManager;
    private Dictionary<Player, Character> _dicModels = new Dictionary<Player, Character>();
    private Dictionary<Player, ChatManager> _dicChat = new Dictionary<Player, ChatManager>();
    private bool _enoughPlayers;
    public int PackagesPerSecond { get; private set; }

    private void Start()
    {
        chatManager = FindObjectOfType<ChatManager>();
        
        if (!chatManager)
        {
            StartCoroutine(SearchChat());
        }
        
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

                StartCoroutine(CheckPlayers());
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

    IEnumerator SearchChat()
    {
        while (!chatManager)
        {
            chatManager = FindObjectOfType<ChatManager>();
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("consegui chat");
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

        Character newCharacter = PhotonNetwork
            .Instantiate(characterPrefab.name, spawns.spawns[pos].position, spawns.spawns[pos].rotation)
            .GetComponent<Character>();
        newCharacter.transform.Rotate(newCharacter.transform.up, 90.0f);
        newCharacter.SetInitialParameters(player);
        _dicModels.Add(player, newCharacter);
        RequestUpdatePlayerList();
        photonView.RPC("SetWaitingScreen", player, true);

    }

    IEnumerator CheckPlayers()
    {
        while (!_enoughPlayers)
        {
            _enoughPlayers = PhotonNetwork.PlayerList.Length >= 5;
            yield return new WaitForSecondsRealtime(1);
        }

        _enoughPlayers = true;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            photonView.RPC("SetWaitingScreen", p, false);
        }

        Debug.Log("enough players: " + _enoughPlayers);
    }

    [PunRPC]
    void SetWaitingScreen(bool state)
    {
        if (!state)
            _enoughPlayers = true;
        var screens = FindObjectOfType<ScreenManager>();

        screens.WaitingScreenState(state);
    }

    [PunRPC]
    void SetDisconnectScreen()
    {
        var screens = FindObjectOfType<ScreenManager>();

        screens.DisconnectScreen();
    }

    [PunRPC]
    void SetWinScreen()
    {
        var screens = FindObjectOfType<ScreenManager>();

        screens.WinScreen();

        var cc = FindObjectsOfType<CharacterControl>();

        foreach (var c in cc)
        {
            Destroy(c.gameObject);
        }
    }

    public void RequestMove(Player player, Vector3 dir)
    {
        if (!_enoughPlayers)
        {
            return;
        }

        photonView.RPC("Move", _server, player, dir);
    }

    [PunRPC]
    private void Move(Player player, Vector3 dir)
    {
        if (player == null) return;

        if (_dicModels.ContainsKey(player))
        {
            _dicModels[player].Move(dir);
        }

    }

    public void RequestJump(Player player)
    {
        if (!_enoughPlayers) return;
        photonView.RPC("Jump", _server, player);
    }

    [PunRPC]
    private void Jump(Player player)
    {
        if (player == null) return;

        if (_dicModels.ContainsKey(player))
        {
            _dicModels[player].Jump();
        }
    }

    public void RequestShoot(Player player, Vector3 dir)
    {
        if (!_enoughPlayers) return;
        photonView.RPC("Shoot", _server, player, dir);
    }

    [PunRPC]
    private void Shoot(Player player, Vector3 dir)
    {
        if (player == null) return;

        if (_dicModels.ContainsKey(player))
        {
            _dicModels[player].Shoot(dir);
        }
    }

    public void PlayerLose(Player player)
    {
        //CAMBIAR A PANTALLA DE DERROTA
        PhotonNetwork.Destroy(_dicModels[player].gameObject);
        photonView.RPC("SetDisconnectScreen", player);
        _dicModels.Remove(player);
        Debug.Log("dic count: " + _dicModels.Count);
        if (_dicModels.Count > 1) return;

        foreach (var p in _dicModels)
        {
            if (p.Key != _server)
            {
                photonView.RPC("SetWinScreen", p.Key);
            }
        }

        StartCoroutine(CloseServer());
    }

    IEnumerator CloseServer()
    {
        while (true)
        {
            if (PhotonNetwork.PlayerList.Length <= 1)
            {

                photonView.RPC("Disconnect", _server);

                break;
            }

            yield return new WaitForSeconds(1);
        }
    }

    [PunRPC]
    private void Disconnect()
    {
        Debug.Log("server ended");
        PhotonNetwork.LoadLevel("Menu");
        PhotonNetwork.Disconnect();
    }


    public void RequestSendText(string text)
    {
        photonView.RPC("SendText", _server, PhotonNetwork.LocalPlayer, text);
    }

    [PunRPC]
    private void SendText(Player player, string text)
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            photonView.RPC("UpdateChatBox", p, player, text);
        }
    }

    [PunRPC]
    private void UpdateChatBox(Player player, string text)
    {
        chatManager.UpdateChatBox(player.NickName, text);
    }

    public void RequestUpdatePlayerList()
    {
        photonView.RPC("CheckPlayersList", _server);
    }

    [PunRPC]
    private void CheckPlayersList()
    {
        string[] names = new string[PhotonNetwork.PlayerList.Length - 1];

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 1; i < players.Length; i++)
        {
            names[i - 1] = players[i].NickName;
        }

        foreach (var p in PhotonNetwork.PlayerList)
        {
            photonView.RPC("UpdateChatBox", p, names);
        }
    }
    
    [PunRPC]
    private void UpdatePlayersList(string[] names)
    {
        chatManager.UpdatePlayersList(names);
    }
}
