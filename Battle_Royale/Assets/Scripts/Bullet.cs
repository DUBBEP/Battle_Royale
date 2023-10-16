using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;

    public Rigidbody rig;
    public GameObject exp;
    private PlayerWeapon attackingPlayer;


    public void Initialize (int damage, int attackerId, bool isMine, PlayerWeapon attacker)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;
        attackingPlayer = attacker;

        if (attackingPlayer.weaponType == "HamGrenade")
            Invoke("DelayedExplosion", attackingPlayer.detonateTimer);
        else
            Destroy(gameObject, 5.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bullet Triggered");
        if (other.CompareTag("Player") && isMine)
        {
            Debug.Log("Was a player and was mine");
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (player.id != attackerId)
            {
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);

                DestroyBullet();
            }
        }
        else
        {
            DestroyBullet();
        }

    }

    public void DestroyBullet()
    {
        if (attackingPlayer.weaponType == "Pistol" || attackingPlayer.weaponType == "AWP")
        {
            Destroy(gameObject);
        }
        else if (attackingPlayer.weaponType == "Bazooka")
        {
            if (isMine)
                attackingPlayer.CallSpawnExplosion(this.gameObject.transform.position, 25);
            
            Destroy(gameObject);
        }
    }

    void DelayedExplosion()
    {
        if (isMine)
        {
            attackingPlayer.CallSpawnExplosion(this.gameObject.transform.position, 100);
            attackingPlayer.detonateTimer = 3f;
        }
        Destroy(gameObject);
    }

}
