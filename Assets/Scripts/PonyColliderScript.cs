using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PonyColliderScript : MonoBehaviour
{
    // Start is called before the first frame update
    /*void Start()
    {

    }*/

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstacle"))
        {
            collision.collider.gameObject.layer = 10;
            Debug.Log(collision.collider.gameObject.layer);
        }
    }
}
