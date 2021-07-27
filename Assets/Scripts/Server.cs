using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Server : MonoBehaviourPunCallbacks
{
    public static Server Instance;

    private Player _server;

    public Character characterPrefab;

    public CharacterSpawn spawns;

    public ChatManager chatManager;
    private Dictionary<Player, Character> _dicModels = new Dictionary<Player, Character>();
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

        if (Instance == null)
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
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

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
        var pos = GetPositionInPlayersList(player);
        foreach (var p in PhotonNetwork.PlayerList)
        {
            photonView.RPC("UpdateChatBox", p, pos, player.NickName, text);
        }
    }

    [PunRPC]
    private void UpdateChatBox(int posInPlayerList, string nickname, string text)
    {
        chatManager.UpdateChatBox(posInPlayerList, nickname, text);
    }

    private void RequestUpdatePlayerList()
    {
        photonView.RPC("CheckPlayersList", _server);
    }

    [PunRPC]
    private void CheckPlayersList()
    {
        Player[] players = PhotonNetwork.PlayerList;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            photonView.RPC("UpdatePlayersList", p, players);
        }
    }
    
    [PunRPC]
    private void UpdatePlayersList(Player[] players)
    {
        chatManager.UpdatePlayersList(players);
    }

    private int GetPositionInPlayersList(Player player)
    {
        for (int i = 1; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == player)
            {
                return i;
            }
        }

        return 0;
    }
}
