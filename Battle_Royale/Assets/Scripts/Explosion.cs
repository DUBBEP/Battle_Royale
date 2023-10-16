using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Explosion : MonoBehaviour
{
    public float bounceForce;

    private float activeDamageTime;
    private int damage;
    private int attackerId;
    private bool isMine;
    private bool canBounce = true;
    public Rigidbody playerRig;

    public void InitializeExplosion(int damage, int attackerId, bool isMine, GameObject attacker)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;
        playerRig = attacker.GetComponent<Rigidbody>();
        activeDamageTime = 0.2f;

        Destroy(gameObject, 8f);
    }

    void Update()
    {
        if (activeDamageTime > 0)
        {
            activeDamageTime -= Time.deltaTime;
        }


        if (activeDamageTime <= 0)
        {
            GetComponent<SphereCollider>().enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isMine)
        {
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if (player.id != attackerId)
            {
                player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
                player.GetComponent<Rigidbody>().AddForce(Vector3.up * bounceForce*(2/3), ForceMode.Impulse);

            }
            else if (player.id == attackerId && canBounce) 
            {
                playerRig.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
                canBounce = false;
            }
        }
    }
}
