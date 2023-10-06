using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public enum PickupType
{
    Health,
    Ammo,
    AWP,
    Bazooka,
    HamGrenade,
    MiniGun
}
public class Pickup : MonoBehaviour
{
    public PickupType type;
    public int value;

    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if(other.CompareTag("Player"))
        {
            // get the player
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            switch (type)
            {
                case PickupType.Health:
                    player.photonView.RPC("Heal", player.photonPlayer, value);
                    break;
                case PickupType.Ammo:
                    player.photonView.RPC("GiveAmmo", player.photonPlayer, value);
                    break;
                case PickupType.AWP:
                    player.photonView.RPC("SwitchWeapon", RpcTarget.All, "AWP");
                    break;
                case PickupType.Bazooka:
                    player.photonView.RPC("SwitchWeapon", RpcTarget.All, "Bazooka");
                    break;
                case PickupType.HamGrenade:
                    player.photonView.RPC("SwitchWeapon", RpcTarget.All, "HamGrenade");
                    break;
                case PickupType.MiniGun:
                    player.photonView.RPC("SwitchWeapon", RpcTarget.All, "MiniGun");
                    break;
            }

            // destroy the object
            PhotonNetwork.Destroy(gameObject);
            
        
        }
    }
}
