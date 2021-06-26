using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Photon.Pun;
public class Projectile : MonoBehaviourPun
{
    public int damage;
    public float speed;
    private Vector3 _dir;
    public float force;

    private void Update()
    {
        transform.position += _dir * (speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("hit something");
        if (photonView.IsMine)
        {
            Debug.Log("view is mine");
            var character = other.gameObject.GetComponent<Character>();
            if (character != null)
            {
                Debug.Log("hit char");
                var contactPoint = other.collider.ClosestPoint(transform.position);
                var dirToPush = (contactPoint - transform.position).normalized;
                character.Damage(damage);
                character.Push(dirToPush, force, contactPoint);
            }
            GetComponent<PhotonView>().RPC("Die", RpcTarget.All);
        }
    }

    [PunRPC]
    void Die()
    {
        Destroy(gameObject);
    }

    public void SetDir(Vector3 dir)
    {
        _dir = dir;
    }
}