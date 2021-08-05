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

    private Character _owner;
    private void Update()
    {
        if (!photonView.IsMine) return;
        
        transform.position += _dir * (speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!photonView.IsMine) return;
        
        var character = other.gameObject.GetComponent<Character>();
        if (character != null && character != _owner)
        {
            var contactPoint = other.collider.ClosestPoint(transform.position);
            var dirToPush = (contactPoint - transform.position).normalized;
            
            Server.Instance.RequestDamage(character.GetCharacterAsPlayer(), damage);

			character.Push(dirToPush, force, contactPoint);
        }
        PhotonNetwork.Destroy(gameObject);
    }

    public Projectile SetDir(Vector3 dir)
    {
        _dir = dir;
        return this;
    }

    public Projectile SetOwner(Character owner)
    {
        _owner = owner;
        return this;
    }
}