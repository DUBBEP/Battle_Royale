using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [Header("Info")]
    public int id;
    private int curAttackerId;
    


    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;

    private bool flashingDamage;


    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public PlayerWeapon weapon;
    public MeshRenderer mr;

    [PunRPC]
    public void Initialize (Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;


        // is this not our local player?
        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }
    }

    private void Update()
    {
        // if this in't our local player or we're dead - return
        if (!photonView.IsMine || dead)
            return;


        Move();

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();

        if (Input.GetMouseButtonDown(0) && weapon.weaponType != "HamGrenade" && weapon.weaponType != "MiniGun")
                weapon.TryShoot();

        if(weapon.weaponType == "HamGrenade")
        {
            if (Input.GetMouseButton(0))
                weapon.detonateTimer -= Time.deltaTime;
            if (Input.GetMouseButtonUp(0) || weapon.detonateTimer < 0)
                weapon.TryShoot();
        }

        if (weapon.weaponType == "AWP" && Input.GetMouseButton(1) && photonView.IsMine)
        {
            GetComponentInChildren<Camera>().fieldOfView = 25f;
            GameUI.instance.ScopedUIOn();
        }
        else
        {
            GetComponentInChildren<Camera>().fieldOfView = 60f;
            GameUI.instance.ScopedUIOff();
        }

        if(weapon.weaponType == "MiniGun")
        {
            if (Input.GetMouseButton(0))
                weapon.TryShoot();
        }

        void Move ()
        {
            // get input axis
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            // calculate the direction relativ to where we're facing
            Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
            dir.y = rig.velocity.y;

            rig.velocity = dir;

        }

        void TryJump ()
        {
            // Creat ray facing down
            Ray ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, 1.5f))
                rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

    }

    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
            return;

        curHp -= damage;
        curAttackerId = attackerId;

        // flash the player red
        photonView.RPC("DamageFlash", RpcTarget.Others);
        // update the health bar UI
        GameUI.instance.UpdateHealthBar();
        // die if no health left
        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);

    }
    [PunRPC]
    void DamageFlash ()
    {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());
         
        IEnumerator DamageFlashCoRoutine ()
        {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;
            
            Debug.Log("Flash on");


            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;

            Debug.Log("Flash Off");

        }
    }

    [PunRPC]
    void Die ()
    {
        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;

        // host will chekc the win condition
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.CheckWinCondition();
        }

        // is this our local player?
        if(photonView.IsMine)
        {
            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            // set the cam to spectator
            GetComponentInChildren<CameraController>().SetAsSpectator();

            // disable the physics and hide the player
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }

    [PunRPC]
    public void AddKill ()
    {
        kills++;

        // update the UI
        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal (int ammountToHeal)
    {
        curHp = Mathf.Clamp(curHp + ammountToHeal, 0, maxHp);

        // update the health bar UI
        GameUI.instance.UpdateHealthBar();
    }

}
