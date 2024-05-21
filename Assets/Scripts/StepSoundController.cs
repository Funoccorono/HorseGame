using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSoundController : MonoBehaviour
{
    [SerializeField] private AudioSource stepSound;
    [SerializeField] private ParticleSystem dustParticles;
    private ThirdPersonController thirdPersonController;
    public ParticleSystem Tracks;
    private ParticleSystem Instance;
    private bool playSound = true;
    [SerializeField] GameObject horse;
    private Vector3 surfaceNormal;
    public float trackOffset = 0.0085f;
    private bool isGrounded;
    public bool instantiateTracks = false;

    private void Awake()
    {
        thirdPersonController = horse.GetComponent<ThirdPersonController>();
        /*if (Tracks != null)
        {
            if (Tracks.gameObject.IsPrefab())
            {
                Instance = Instantiate(Tracks, transform, false); //Instantiate in case the Track is a refab
            }
            else
            {
                Instance = Tracks;
            }
        }*/
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        surfaceNormal = thirdPersonController.surfaceNormal;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Terrain" && playSound)
        {
            stepSound.Play();
            dustParticles.Play();

            if (thirdPersonController.isGrounded)
            {
                var TrackPosition = this.transform.position; TrackPosition.y += trackOffset;
                var TrackRotation = Quaternion.FromToRotation(-horse.gameObject.transform.forward, horse.gameObject.transform.up) * horse.gameObject.transform.rotation;

                /*if (Dust)
                {
                    Dust.transform.position = TrackPosition; //Set The Position
                    Dust.transform.rotation = TrackRotation;
                    Dust.transform.Rotate(-90, 0, 0);
                    Dust.Emit(DustParticles);
                }*/


                if (Instance)
                {
                    ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
                    {
                        rotation3D = TrackRotation.eulerAngles, //Set The Rotation
                        position = TrackPosition, //Set The Position
                    };

                    Instance.Emit(ptrack, 1);
                    //Debug.Log("track");

                    if (instantiateTracks)
                    {
                        if (isGrounded)
                        {
                            Instance.Emit(ptrack, 1);
                        }
                        else
                        {
                            //var newtrack = Instantiate(Tracks);
                            //newtrack.transform.SetParentScaleFixer(hit.transform, TrackPosition);
                            //var main = newtrack.main;
                            ////main.stopAction = ParticleSystemStopAction.Destroy;
                            //main.simulationSpace = ParticleSystemSimulationSpace.Local;
                            //newtrack.Emit(ptrack, 1);
                            //this.Delay_Action(() => newtrack.isPlaying, ()=> Destroy(newtrack));
                        }


                    }
                    else
                    {
                        Instance.Emit(ptrack, 1);
                    }
                }


            }

            playSound = false;
            StartCoroutine(soundCooldown());
        }
    }

    private IEnumerator soundCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        playSound = true;
    }
}

/*public static class TestExtension
{
    public static bool IsPrefab(this GameObject go) => !go.scene.IsValid();
}*/
