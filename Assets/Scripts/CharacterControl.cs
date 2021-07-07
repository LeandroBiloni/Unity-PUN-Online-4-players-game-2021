using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterControl: MonoBehaviourPun
{
    private Camera _cam;
    private void Start()
    {
        //_cam = Camera.main;
        if (!photonView.IsMine) return;
        
        DontDestroyOnLoad(gameObject);

        StartCoroutine(SendPackages());
    }

    // private void Update()
    // {
    //     var h = Input.GetAxis("Horizontal");
    //
    //     if (h == 0) return;
    //     
    //     var dir = Vector3.zero;
    //         
    //     if (h > 0)
    //     {
    //         dir = transform.right;
    //     }
    //     else
    //     {
    //         dir = -transform.right;
    //     }
    //     Server.instance.RequestMove(PhotonNetwork.LocalPlayer, dir);
    // }

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
            Server.instance.RequestMove(PhotonNetwork.LocalPlayer, dir);
            
            if (Input.GetMouseButtonDown(0))
            {
                Server.instance.RequestShoot(PhotonNetwork.LocalPlayer, MousePosition());
            }
            
            if (Input.GetKeyDown(KeyCode.W) )
            {
                Server.instance.RequestJump(PhotonNetwork.LocalPlayer);
            }

            yield return new WaitForSeconds(1 / Server.instance.PackagesPerSecond);
        }
    }
    
    Vector3 MousePosition()
    {
        var cam = FindObjectOfType<Camera>();
        return cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (cam.transform.position - transform.position).magnitude));
    }
}