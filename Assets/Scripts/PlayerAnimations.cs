using System.Collections;
using System.Collections.Generic;
using FIMSpace.Basics;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private Transform hipRaycast, chestRaycast;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform fLeft, fRight, rLeft, rRight;
    [SerializeField] private ParticleSystem tracks;
    [SerializeField] private float trackOffset = 0f;

    [HideInInspector] public float elapsedTime = 0;
    [HideInInspector] public float layerStartWeight = 0;
    [HideInInspector] public float layerEndWeight = 0;
    [HideInInspector] public bool lerpingToZero = false;
    [HideInInspector] public bool lerpingToOne = false;

    private PlayerController playerController;
    private Animator animator;
    private ParticleSystem.MainModule psMain;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        psMain = tracks.main;
    }

    private void Update()
    {
        if (lerpingToOne || lerpingToZero) SmoothlyChangeLayerWeight();
    }

    public void SetGait(int movementState)
    {
        if (movementState == -1)
        {
            lerpingToZero = true;
            layerStartWeight = animator.GetLayerWeight(3);
            layerEndWeight = 0;
            elapsedTime = 0;
            animator.SetBool("IsBacking", true);
            playerController.movementSpeed = -1;
            StopAllCoroutines();
        }

        if (movementState == 1)
        {
            lerpingToZero = true;
            layerStartWeight = animator.GetLayerWeight(3);
            layerEndWeight = 0;
            elapsedTime = 0;
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsTrotting", false);
            playerController.movementSpeed = 1.5f;
            StopAllCoroutines();
        }

        else if (movementState == 2)
        {
            animator.SetBool("IsTrotting", true);
            animator.SetBool("IsCantering", false);
            playerController.movementSpeed = 3;
        }

        else if (movementState == 3)
        {
            animator.SetBool("IsCantering", true);
            animator.SetBool("IsGalloping", false);
            playerController.movementSpeed = 5;
        }

        else if (movementState == 4)
        {
            animator.SetBool("IsGalloping", true);
            playerController.movementSpeed = 10;
        }

        else if (movementState == 0)
        {
            lerpingToOne = true;
            layerStartWeight = animator.GetLayerWeight(3);
            layerEndWeight = 1;
            elapsedTime = 0;
            animator.SetBool("IsBacking", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsCantering", false);
            playerController.movementSpeed = 0;
            StartCoroutine(TailSwing());
            StartCoroutine(EarFlick());
            StartCoroutine(HeadTurn());
        }
    }

    public void StartJump()
    {
        playerController.movementSpeed += 2;
    }

    public void EndJump()
    {
        playerController.movementSpeed -= 2;
    }

    public void SmoothlyChangeLayerWeight()
    {
        elapsedTime += 2 * Time.deltaTime;

        float lerp = Mathf.Lerp(layerStartWeight, layerEndWeight, elapsedTime);

        animator.SetLayerWeight((int)AnimationLayers.tail, lerp);
        animator.SetLayerWeight((int)AnimationLayers.ears, lerp);
        animator.SetLayerWeight((int)AnimationLayers.head, lerp);

        if (lerp == layerEndWeight)
        {
            lerpingToOne = false;
            lerpingToZero = false;
        }
    }

    private IEnumerator TailSwing()
    {
        yield return new WaitForSeconds(2);

        while (playerController.movementSpeed == 0)
        {
            int randomNum = Random.Range(0, 2);
            if (randomNum == 0)
            {
                yield return new WaitForSeconds(5);
            }
            else
            {
                animator.SetBool("TailSwing", true);
                yield return new WaitForSeconds(0.5f);
                animator.SetBool("TailSwing", false);
                yield return new WaitForSeconds(5);
            }
        }
    }

    private IEnumerator EarFlick()
    {
        yield return new WaitForSeconds(2);

        while (playerController.movementSpeed == 0)
        {
            int randomNum = Random.Range(0, 2);
            if (randomNum == 0)
            {
                yield return new WaitForSeconds(5);
            }
            else
            {
                int num = Random.Range(0, 2);
                if (num == 0) animator.SetBool("EarFlickL", true);
                if (num == 1) animator.SetBool("EarFlickR", true);
                yield return new WaitForSeconds(0.5f);
                animator.SetBool("EarFlickL", false);
                animator.SetBool("EarFlickR", false);
                yield return new WaitForSeconds(5);
            }
        }
    }

    private IEnumerator HeadTurn()
    {
        yield return new WaitForSeconds(2);

        while (playerController.movementSpeed == 0)
        {
            int randomNum = Random.Range(0, 2);
            if (randomNum == 0)
            {
                yield return new WaitForSeconds(5);
            }
            else
            {
                int whichDirection = Random.Range(0, 2);
                animator.SetFloat("HeadDirection", whichDirection);
                animator.SetBool("HeadTurn", true);
                yield return new WaitForSeconds(0.5f);
                animator.SetBool("HeadTurn", false);
                yield return new WaitForSeconds(10);
            }
        }
    }

    //---------------STEPS---------------

    public void Step1()
    {
        audioSources[0].clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSources[0].PlayOneShot(audioSources[0].clip);
    }

    public void Step2()
    {
        audioSources[1].clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSources[1].PlayOneShot(audioSources[1].clip);
    }

    public void Step3()
    {
        audioSources[2].clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSources[2].PlayOneShot(audioSources[2].clip);
    }

    public void Step4()
    {
        audioSources[3].clip = stepSounds[Random.Range(0, stepSounds.Length)];
        audioSources[3].PlayOneShot(audioSources[3].clip);
    }

    public void FLeft()
    {
        if (!fLeft) return;

        var trackRotation = (Quaternion.FromToRotation(-this.transform.forward, this.transform.up) * this.transform.rotation).eulerAngles * Mathf.Deg2Rad;
        var trackPosition = new Vector3(fLeft.position.x, chestRaycast.position.y + trackOffset, fLeft.position.z);

        RaycastHit hit;

        Physics.Raycast(chestRaycast.transform.position, Vector3.down, out hit, groundMask);

        psMain.startRotationX = trackRotation.x;
        psMain.startRotationY = trackRotation.y;
        psMain.startRotationZ = trackRotation.z;

        ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
        {
            position = trackPosition, //Set The Position

        };

        tracks.Emit(ptrack, 1);
    }

    public void FRight()
    {
        if (!fRight) return;

        var trackRotation = (Quaternion.FromToRotation(-this.transform.forward, this.transform.up) * this.transform.rotation).eulerAngles * Mathf.Deg2Rad;
        var trackPosition = new Vector3(fRight.position.x, chestRaycast.position.y + trackOffset, fRight.position.z);

        RaycastHit hit;

        Physics.Raycast(chestRaycast.transform.position, Vector3.down, out hit, groundMask);

        psMain.startRotationX = trackRotation.x;
        psMain.startRotationY = trackRotation.y;
        psMain.startRotationZ = trackRotation.z;

        ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
        {
            position = trackPosition, //Set The Position

        };

        tracks.Emit(ptrack, 1);
    }

    public void RLeft()
    {
        if (!rLeft) return;

        var trackRotation = (Quaternion.FromToRotation(-this.transform.forward, this.transform.up) * this.transform.rotation).eulerAngles * Mathf.Deg2Rad;
        var trackPosition = new Vector3(rLeft.position.x, hipRaycast.position.y + trackOffset, rLeft.position.z);

        RaycastHit hit;

        Physics.Raycast(chestRaycast.transform.position, Vector3.down, out hit, groundMask);

        //Debug.Log(rLeft.transform.position);
        //hoofPos.PrintPosition();

        psMain.startRotationX = trackRotation.x;
        psMain.startRotationY = trackRotation.y;
        psMain.startRotationZ = trackRotation.z;

        ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
        {
            position = trackPosition, //Set The Position

        };

        tracks.Emit(ptrack, 1);
    }

    public void RRight()
    {
        if (!rRight) return;

        var trackRotation = (Quaternion.FromToRotation(-this.transform.forward, this.transform.up) * this.transform.rotation).eulerAngles * Mathf.Deg2Rad;
        var trackPosition = new Vector3(rRight.position.x, hipRaycast.position.y + trackOffset, rRight.position.z);

        RaycastHit hit;

        Physics.Raycast(chestRaycast.transform.position, Vector3.down, out hit, groundMask);

        psMain.startRotationX = trackRotation.x;
        psMain.startRotationY = trackRotation.y;
        psMain.startRotationZ = trackRotation.z;

        ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
        {
            position = trackPosition, //Set The Position

        };

        tracks.Emit(ptrack, 1);
    }
}
