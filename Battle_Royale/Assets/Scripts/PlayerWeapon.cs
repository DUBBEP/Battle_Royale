using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.ComponentModel;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRate;

    public string weaponType;
    
    private float lastShootTime;

    [Header("WeaponResources")]
    public GameObject currentWeapon;
    public GameObject currentBullet;
    public Transform currentSpawnPos;
    public GameObject[] weapons;
    public GameObject[] bulletPreFabs;
    public Transform[] bulletSpawnPos;

    public float detonateTimer;

    public GameObject exp;
    public GameObject HamGrenadeModel;

    private PlayerController player;


    void Awake()
    {
        // get required components
        player = GetComponent<PlayerController>();
        weaponType = "Pistol";
        detonateTimer = 5f;
    }

    public void TryShoot ()
    {
        // can we shoot?
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRate)
            return;

        curAmmo--;
        lastShootTime = Time.time;

        // update the ammo UI
        GameUI.instance.UpdateAmmoText();
        // spawn the bullet
        player.photonView.RPC("SpawnBullet", RpcTarget.All, currentSpawnPos.position, Camera.main.transform.forward);
    }

    [PunRPC]
    void SpawnBullet (Vector3 pos, Vector3 dir)
    {
        // spawn and orientate it
        GameObject bulletObj = Instantiate(currentBullet, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        // get bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        // initialize it and set the velocity
        bulletScript.Initialize(damage, player.id, player.photonView.IsMine, this);
        if (weaponType == "Pistol" || weaponType == "AWP" || weaponType == "Bazooka")
        {
            bulletScript.rig.velocity = dir * bulletSpeed;
        }
        else if (weaponType == "MiniGun" || weaponType == "HamGrenade")
        {
            bulletScript.rig.AddForce(dir * bulletSpeed, ForceMode.Impulse);
        }

        if (weaponType == "HamGrenade")
        {
            HamGrenadeModel.SetActive(false);
        }

    }

        public void CallSpawnExplosion(Vector3 pos, int damage)
        {
            Debug.Log("Call Spawn Explotion Done");
            player.photonView.RPC("SpawnExplosion", RpcTarget.All, pos, damage);
        }

        [PunRPC]
        public void SpawnExplosion(Vector3 pos, int damage)
        {
            // spawn it
            GameObject explosionObj = Instantiate(exp, pos, Quaternion.identity);

            // get explosion script
            Explosion explosionScript = explosionObj.GetComponent<Explosion>();

            // initialize it
            explosionScript.InitializeExplosion(damage, player.id, player.photonView.IsMine, player.gameObject);
            Debug.Log("Initialized");
        }


    [PunRPC]
    public void GiveAmmo (int ammoToGive)
    {
        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        // update the Ammo text
        GameUI.instance.UpdateAmmoText();

        if (weaponType == "HamGrenade")
            HamGrenadeModel.SetActive(true);
    }

    [PunRPC]
    public void SwitchWeapon (string weapon)
    {
        // set old weapon to inactive
        currentWeapon.SetActive(false);


        // change to new weapon
        switch (weapon)
        {
            case "AWP":
                // parameters are weaponindex, name, bullet speed, damage, fire rate, ammo capacity
                SwitchWeapon(0, "AWP", 100f, 60, 0.9f, 10);
                break;
            case "Bazooka":
                // parameters are weaponindex, name, bullet speed, damage, fire rate, ammo capacity
                SwitchWeapon(1, "Bazooka", 20f, 25, 2f, 8);
                break;
            case "HamGrenade":
                // parameters are weaponindex, name, bullet speed, damage, fire rate, ammo capacity
                SwitchWeapon(2, "HamGrenade", 20f, 2, 0f, 1);
                break;
            case "MiniGun":
                // parameters are weaponindex, name, bullet speed, damage, fire rate, ammo capacity
                SwitchWeapon(3, "MiniGun", 45f, 5, 0.01f, 300);
                break;

        }
    }

    void SwitchWeapon(int weaponListvalue, string weaponName, float speed, int weaponDamage, float fireRate, int ammocapacity)
    {
        // set new weapon active

        currentWeapon = weapons[weaponListvalue];
        currentSpawnPos = bulletSpawnPos[weaponListvalue];
        currentBullet = bulletPreFabs[weaponListvalue];
        currentWeapon.SetActive(true);
        weaponType = weaponName;

        // change weapon stats

        bulletSpeed = speed;
        damage = weaponDamage;
        shootRate = fireRate;
        maxAmmo = ammocapacity;
        curAmmo = ammocapacity;
        GameUI.instance.UpdateAmmoText();
    }
}
