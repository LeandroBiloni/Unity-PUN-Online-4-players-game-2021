using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CharacterControl: MonoBehaviourPun
{
    private Camera _cam;
    private Player _localPlayer;
    private ChatManager _chatManager;
    private void Start()
    {
        if (!photonView.IsMine) return;

        _localPlayer = PhotonNetwork.LocalPlayer;
        
        DontDestroyOnLoad(gameObject);

        StartCoroutine(SendPackages());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Server.Instance.RequestShoot(_localPlayer, MousePosition());
        }
            
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            Server.Instance.RequestJump(_localPlayer);
        }

	}

    IEnumerator SendPackages()
    {
        while (true)
        {
            Debug.Log("send packages");
            var h = Input.GetAxis("Horizontal");
            
            var dir = Vector3.zero;
            
            if (h > 0)
            {
                dir = transform.right;
            }
            
            if (h<0)
            {
                dir = -transform.right;
            }
            Server.Instance.RequestMove(_localPlayer, dir);

            yield return new WaitForSeconds(1 / Server.Instance.PackagesPerSecond);
        }
    }
    
    Vector3 MousePosition()
    {
        if (!_cam)
        {
            _cam = FindObjectOfType<Camera>();
        }
        return _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (_cam.transform.position - transform.position).magnitude));
    }
}