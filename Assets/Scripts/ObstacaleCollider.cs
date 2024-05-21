using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacaleCollider : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    private int IgnorePlayer;

    private Collider obstacleCollider;
    // Start is called before the first frame update
    void Start()
    {
        IgnorePlayer = LayerMask.NameToLayer("Ignore Raycast");
        obstacleCollider = GetComponent<Collider>();
    }

    public void DisableCollider()
    {
        obstacleCollider.enabled = !obstacleCollider.enabled;
        Debug.Log("Disabled collider");
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject.layer);

        if (collision.gameObject.layer == 7 || collision.gameObject.layer == 3)
        {
            this.gameObject.layer = IgnorePlayer;
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
