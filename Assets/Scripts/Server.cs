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

    //Corrutina por si tarda en cargarse el spawn y no lo encuentra en el primer find
    IEnumerator SearchSpawns()
    {
        while (!spawns)
        {
            spawns = FindObjectOfType<CharacterSpawn>();
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("consegui spawn");
    }

    //Corrutina por si tarda en cargarse el chat manager y no lo encuentra en el primer find
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

    //Checkeo de los jugadores en el room para ver cuando empezar la partida
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
    
    //Pantalla del lobby
    [PunRPC]
    void SetWaitingScreen(bool state)
    {
        if (!state)
            _enoughPlayers = true;
        var screens = FindObjectOfType<ScreenManager>();

        screens.WaitingScreenState(state);
        screens.SetRoomName(PhotonNetwork.CurrentRoom.Name);
    }

    //Pantalla de derrota
    [PunRPC]
    void SetDisconnectScreen()
    {
        var screens = FindObjectOfType<ScreenManager>();

        screens.DisconnectScreen();
    }
    
    //Pantalla de victoria
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

	public void RequestDamage(Player player, int damage)
	{
		if (!_enoughPlayers) return;
		photonView.RPC("Damage", _server, player, damage);
	}

	[PunRPC]
	private void Damage(Player player, int damage)
	{
		if (player == null) return;
		if (_dicModels.ContainsKey(player))
		{
			var myPlayer = _dicModels[player];
			myPlayer.Damage(damage);
			var playerHP = myPlayer.GetHP();
            foreach (var p in _dicModels)
            {
                photonView.RPC("UpdateLifeBar", p.Key, myPlayer.photonView.ViewID, playerHP);
            }
		}
	}

    [PunRPC]
    //Uso el viewID porque no puedo pasar el character, así se a cual cambiarle la vida
    void UpdateLifeBar(int photonViewID, float playerHP)
    {
        var characters = FindObjectsOfType<Character>();
        foreach (var c in characters)
        {
            if (c.photonView.ViewID == photonViewID)
                c.UpdateLifeBar(playerHP);
        }
    }

    public void SetPlayerName(int photonViewID, string name)
    {
        photonView.RPC("SetPlayerNameBuffered", RpcTarget.AllBuffered, photonViewID, name);
    }

    [PunRPC]
    //Uso el viewID porque no puedo pasar el character, así se a cual cambiarle el nombre
    void SetPlayerNameBuffered(int photonViewID, string name)
    {
        var characters = FindObjectsOfType<Character>();
        
        foreach (var c in characters)
        {
            if (c.photonView.ViewID == photonViewID)
                c.UpdateName(name);
        }
    }


	//Se ejecuta cuando un jugador pierde
	public void PlayerLose(Player player)
    {
        PhotonNetwork.Destroy(_dicModels[player].gameObject);
        photonView.RPC("SetDisconnectScreen", player);
        _dicModels.Remove(player);
        
        //Si queda 1 en el diccionario se activa en ese jugador la pantalla de victoria
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

    //Desconección del jugador que sale del lobby
    [PunRPC]
    private void Disconnect()
    {
        //PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("Menu");
    }
    
    //Pedido del local player para enviar un mensaje al chat
    public void RequestSendText(string text)
    {
        photonView.RPC("SendText", _server, PhotonNetwork.LocalPlayer, text);
    }

    //Envía el mensaje a a todos los player
    [PunRPC]
    private void SendText(Player player, string text)
    {
        //la posición es para asignarle el color en el chat
        var pos = GetPositionInPlayersList(player);
        foreach (var p in PhotonNetwork.PlayerList)
        {
            photonView.RPC("UpdateChatBox", p, pos, player.NickName, text);
        }
    }

    //Actualiza la ventana de chat con los nuevos mensajes
    [PunRPC]
    private void UpdateChatBox(int posInPlayerList, string nickname, string text)
    {
        chatManager.UpdateChatBox(posInPlayerList, nickname, text);
    }

    //Pedido al server para actualizar la lista de players en la sala
    private void RequestUpdatePlayerList()
    {
        photonView.RPC("CheckPlayersList", _server);
    }

    //Envía la actualización de la lista de players a todos los players
    [PunRPC]
    private void CheckPlayersList()
    {
        Debug.Log("check player server orig");
        Player[] players = PhotonNetwork.PlayerList;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            photonView.RPC("UpdatePlayersList", p, players);
        }
    }
    
    //Actualización de la lista de players
    [PunRPC]
    private void UpdatePlayersList(Player[] players)
    {
        chatManager.UpdatePlayersList(players);
    }

    //Utilidad para poder asignar el color del player en el chat
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

    //Lo ejecuta el local player cuando sale de la sala, pide al server original que remueva al player
    public void PlayerLeavesRoom(Player player)
    {
        photonView.RPC("RemoveAndDisconnectPlayer", _server, player);
    }

    //Lo ejecuta el server original, remuevo y desconecto al player de la sala
    [PunRPC]
    public void RemoveAndDisconnectPlayer(Player player)
    {
        if (player == null) return;

        if (!_dicModels.ContainsKey(player)) return;
        
        //Destruyo el character del player que se desconecto
        PhotonNetwork.Destroy(_dicModels[player].gameObject);
        //PhotonNetwork.DestroyPlayerObjects(player);
        
        //Remuevo el player que se desconecto del diccionario
        _dicModels.Remove(player);
        
        //Lo desconecto y lo mando a la pantalla principal
        photonView.RPC("Disconnect", player);
        
        //Actualizo la lista de players
        //Corrutina para darle tiempo a que se desconecte por completo el player y desaparezca de PhotonNetwork.PlayerList
        StartCoroutine(UpdatePlayerListWithTimer());
    }
    
    IEnumerator UpdatePlayerListWithTimer()
    {
        yield return new WaitForSeconds(2);
        if (PhotonNetwork.PlayerList.Length > 1)
            RequestUpdatePlayerList();
    }
}
