using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CharacterSpawn : MonoBehaviourPun
{
    public List<Transform> spawns;
    // private bool _spawn = true;
    // private PhotonView _myView;
    //
    // // Start is called before the first frame update
    // void Start()
    // {
    //     _myView = GetComponent<PhotonView>();
    //     
    //     if (!_myView.IsMine) return;
    //     _spawn = true;
    // }
    //
    // // Update is called once per frame
    // void Update()
    // {
    //     if (!_myView.IsMine) return;
    //     if (_spawn && PhotonNetwork.PlayerList.Length >= 2)
    //         _myView.RPC("Spawn", RpcTarget.All);
    // }
    //
    // [PunRPC]
    // public void Spawn()
    // {
    //     for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
    //     {
    //         if (PhotonNetwork.PlayerList[i].IsLocal)
    //         {
    //             var p = PhotonNetwork.Instantiate("Character", spawns[i].position, Quaternion.identity);
    //         }
    //         
    //     }
    //     _spawn = false;
    //     FindObjectOfType<ScreenManager>().checkCharacters = true;
    //     //Destroy(gameObject);
    // } 
}
