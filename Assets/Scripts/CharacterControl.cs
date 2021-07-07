using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterControl: MonoBehaviourPun
{
    private void Start()
    {
        if (!photonView.IsMine) return;
        
        DontDestroyOnLoad(this.gameObject);

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

            yield return new WaitForSeconds(1 / Server.instance.PackagesPerSecond);
        }
    }
}