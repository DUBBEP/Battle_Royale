using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    public float shrinkWaitTime;
    public float shrinkAmmount;
    public float shrinkDuration;
    public float minShrinkAmmount;

    public int playerDamage;

    private float lastShrinkEndTime;
    private bool shrinking;
    private float targetDiameter;
    private float lastPlayerCheckTime;

    void Start()
    {
        lastShrinkEndTime = Time.time;
        targetDiameter = transform.localScale.x;
    }

    void Update()
    {
        if (shrinking)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one * targetDiameter, (shrinkAmmount / shrinkDuration) * Time.deltaTime);

            if (transform.localScale.x == targetDiameter)
                shrinking = false;
        }
        else
        {
            // can we shrink again?
            if (Time.time - lastShrinkEndTime >= shrinkWaitTime && transform.localScale.x > minShrinkAmmount)
                Shrink();
        }
        
        if (GameManager.instance.playersSpawned)
            CheckPlayers();
    }

    void Shrink ()
    {
        shrinking = true;

        // make sure we don't shrink below the min ammount
        if (transform.localScale.x - shrinkAmmount > minShrinkAmmount)
            targetDiameter -= shrinkAmmount;
        else
            targetDiameter = minShrinkAmmount;

        lastShrinkEndTime = Time.time + shrinkDuration;
    }

    void CheckPlayers ()
    {
        if(Time.time - lastPlayerCheckTime > 1.0f)
        {
            lastPlayerCheckTime = Time.time;

            // loop through all players
            foreach(PlayerController player in GameManager.instance.players)
            {
                if (player.dead || !player)
                    continue;

                if(Vector3.Distance(Vector3.zero, player.transform.position) >= transform.localScale.x)
                {
                    player.photonView.RPC("TakeDamage", player.photonPlayer, 0, playerDamage);
                }
            }
        }
    }
}
