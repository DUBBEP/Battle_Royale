using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Components")]
    public Rigidbody rig;

    private void Update()
    {
        Move();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
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
}