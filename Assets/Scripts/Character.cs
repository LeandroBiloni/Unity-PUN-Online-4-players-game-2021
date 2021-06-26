using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using UnityEditor;

public class Character : MonoBehaviourPun, IPunObservable
{
    private PhotonView _myView;
    private string _playerName;
    public float maxHp;
    private float _hp;
    public Image hpBar;
    private Rigidbody _rb;

    public float jumpForce;

    public float moveSpeed;
    


    private float _horizontal;

    public List<Transform> spawnPoints = new List<Transform>();
    public Projectile projectile;
    public float cooldown;
    
    private bool _jumping;
    private bool _grounded;
    private  bool _canShoot;
    private bool _canMove;
    public bool alive = true;
    private Camera _cam;

    public TextMeshPro nameText;
    // Start is called before the first frame update
    void Start()
    {
        _myView = GetComponent<PhotonView>();
        _rb = GetComponent<Rigidbody>();
        
        if (!_myView.IsMine) return;
        GetComponent<MeshRenderer>().material.color = Color.green;
        
        _myView.RPC("RPCChangeColor", RpcTarget.OthersBuffered, 1f,0f,0f);
        _myView.RPC("SetPlayerName", RpcTarget.AllBuffered,PhotonNetwork.LocalPlayer.NickName);
        
        _cam = Camera.main;
        _canShoot = true;
        _canMove = true;
        _hp = maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_myView.IsMine) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (_canShoot)
            {
                _canShoot = false;
                Shoot(MousePosition());
                //_myView.RPC("Shoot", RpcTarget.AllBuffered, MousePosition());
                StartCoroutine(Cooldown());
            }
        }
        
        if (_canMove)
        {
            
            _horizontal = Input.GetAxis("Horizontal");
            if (_horizontal != 0)
            {
                Move();
                //_myView.RPC("Move", RpcTarget.AllBuffered);
            }

        }
        
        if (Input.GetKeyDown(KeyCode.W) && _jumping == false)
        {
            Jump();
            //_myView.RPC("Jump", RpcTarget.AllBuffered);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_myView == null)
        {
            Debug.Log("sin view");
        }
        if (!_myView.IsMine) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            //_myView.RPC("ResetRotation", RpcTarget.AllBuffered);
            ResetRotation();
        }
    }

    Vector3 MousePosition()
    {
        return _cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (_cam.transform.position - transform.position).magnitude));
    }
    
    [PunRPC]
    void Move()
    {
        Debug.Log("me muevo");
        if (_horizontal > 0)
            _rb.AddForce(transform.right * moveSpeed);
        else _rb.AddForce(-transform.right * moveSpeed);
    }

    [PunRPC]
    void Jump()
    {
        _jumping = true;
        _grounded = false;
        _rb.constraints = RigidbodyConstraints.None;
        _rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX |
                          RigidbodyConstraints.FreezeRotationY;
        _rb.AddForce(transform.up * jumpForce);
    }
    
    void Shoot(Vector3 mousePos)
    {
        Vector3 closest = Vector3.zero;
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (i == 0)
            {
                closest = spawnPoints[i].position;
                continue;
            }

            
            if ( (mousePos- spawnPoints[i].position).magnitude <
                (mousePos - closest).magnitude)
                closest = spawnPoints[i].position;

        }
        var proj = PhotonNetwork.Instantiate("Projectile", closest, Quaternion.identity);
        var p = proj.GetComponent<Projectile>();
        var dir = (mousePos - transform.position);
        dir.z = 0;
        p.SetDir(dir.normalized);
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        _canShoot = true;
    }
    
    
    public void Push(Vector3 dir, float force, Vector3 collisionPoint)
    {
        _rb.AddForceAtPosition(dir * force, collisionPoint);
    }
    
    
    public void Damage(int damage)
    {
        _hp -= damage;
        _myView.RPC("UpdateLifeBar", RpcTarget.All, _hp);
        
        if (_hp <= 0)
        {
            alive = false;
            ActivateLoseScreen();
            _myView.RPC("Die", RpcTarget.All);
        }
    }
    
    [PunRPC]
    void ResetRotation()
    {
        _rb.constraints = RigidbodyConstraints.None;
        _rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX |
                          RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        _jumping = false;
        _grounded = true;
        transform.rotation = Quaternion.identity;
        if (_horizontal == 0)
            _rb.velocity = Vector3.zero;
    }

    [PunRPC]
    public void RPCChangeColor(float r, float g, float b)
    {
        GetComponent<MeshRenderer>().material.color = new Color(r,g,b);
    }

    [PunRPC]
    void UpdateLifeBar(float currenthp)
    {
        hpBar.fillAmount = currenthp / maxHp;
    }

    [PunRPC]
    void Die()
    {
        Debug.Log("memo ri");
        Destroy(gameObject);
        //PhotonNetwork.Disconnect();
    }
    
    void ActivateLoseScreen()
    {
        FindObjectOfType<ScreenManager>().EndGame();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_hp);
        }
        else
        {
            _hp = (float) stream.ReceiveNext();
        }
    }

    public float GetHP()
    {
        return _hp;
    }

    [PunRPC]
    public void SetPlayerName(string name)
    {
        _playerName = name;
        if (_myView == null)
        {
            _myView = GetComponent<PhotonView>();
        }
        _myView.RPC("UpdateName", RpcTarget.All);
    }

    public string GetPlayerName()
    {
        return _playerName;
    }

    [PunRPC]
    void UpdateName()
    {
        nameText.text = _playerName;
    }
}
