using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    #region Global and Instance Variables 

    [Header("Game Settings")]
    [SerializeField] [Range(8, 24)] int ammoPickupSize = 12;
    [SerializeField] [Range(10, 100)] int healthPickupAmount = 30; 


    [Header("Player Settings")]
    public float moveSpeed = 6f;
    public float jumpHeight = 100f;
    

    Rigidbody myRb;
    CapsuleCollider myCol;

    [Header("Animation Settings")]
    public Animator myAnim;
    public Transform gunHandPosition;
    public GameObject currentGun;

    [Header("Audio Settings")]
    public AudioSource[] footsteps;
    [SerializeField] [Range(0.1f, 2.0f)] float footStepInterval = 0.4f;
    public AudioSource jumpSFX;
    public AudioSource landSFX;
    public AudioSource incDamageSFX;
    public AudioSource deathSFX;
    public AudioSource reloadSFX;

    [Header("Pickups")]
    public AudioSource ammoPickupSFX;
    public AudioSource healthPickupSFX;
    public AudioSource triggerSFX;

    [Header("Camera Settings")]
    public Camera cam;
    public float turnSpeed = 5f;
    public float minX = -90f;
    public float maxX = 90f;

    Quaternion camRot;
    Quaternion playerRot;

    bool cursorIsLocked = true;
    bool lockCursor = true;

    bool playingRunning = false;
    bool previouslyGrounded = true;

    
    //instance variables

    float delta = 0.0f;
    float x = 0f;
    float y = 0f;

    // Inventory
    int ammo = 0;
    int maxAmmo = 48;

    int health = 10;
    int maxHealth = 100;

    int ammoClip = 0;
    int ammoMaxClip = 10;

    int ammoDesired = 0;
    int ammoSupply = 0;

    #endregion

    #region Start / Initialisation

    // Start is called before the first frame update
    void Start()
    {
        myRb = (Rigidbody) GetComponent("Rigidbody");
        myCol = (CapsuleCollider) GetComponent("CapsuleCollider");

        camRot = cam.transform.localRotation;
        playerRot = transform.localRotation;

        currentGun.transform.SetParent(gunHandPosition);
        
    }

    #endregion

    #region Updates

    void Update()
    {

        FootSteps();
        ToggleHolster();
        ReloadGun();
        FireGun();
        Jump();

    }

    void FixedUpdate()
    {
        Move();
        Look();
        UpdateCursorLock();

    }

    #endregion

    #region Methods

    public void Move()
    {
        // Movement Logic
        delta = Time.deltaTime;
        x = Input.GetAxis("Horizontal") * delta * moveSpeed;
        y = Input.GetAxis("Vertical") * delta * moveSpeed;
        
        transform.position += cam.transform.forward * y  + cam.transform.right * x;
    }

    public void Look()
    {
        // Look Logic
        float yRot = Input.GetAxis("Mouse X")  * turnSpeed;
        float xRot = Input.GetAxis("Mouse Y")  * turnSpeed;

        camRot *= Quaternion.Euler(-xRot, 0, 0);
        playerRot *= Quaternion.Euler(0, yRot, 0);

        camRot = ClampRotationAroundAxis(camRot);

        transform.localRotation = playerRot;
        cam.transform.localRotation = camRot;

    }

    public void Jump()
    {
        // Jump Logic
        bool grounded = isGrounded();
        if(Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            myRb.AddForce(0, jumpHeight, 0);
            PlayAudioSource(jumpSFX);
            if(myAnim.GetBool("running"))
            {
                CancelInvoke(nameof(PlayFootStepAudio));
                playingRunning = false;
            }
        }
        else if(!previouslyGrounded && grounded)
        {
            PlayAudioSource(landSFX);
        }

        previouslyGrounded = grounded;
    }

    public void FootSteps()
    {
        if(Mathf.Abs(x) > 0.0f || Mathf.Abs(y) > 0.0f)
        {
            if(!myAnim.GetBool("running"))
            {
                myAnim.SetBool("running", true);
                InvokeRepeating(nameof(PlayFootStepAudio), 0f, footStepInterval);
            }
        }
        else if (!myAnim.GetBool("running"))
        {
            myAnim.SetBool("running", false);
            CancelInvoke(nameof(PlayFootStepAudio));
            
        }
        else
        {
            myAnim.SetBool("running", false);
            CancelInvoke(nameof(PlayFootStepAudio));
            playingRunning = false;
            
        }

    }

    public void ToggleHolster()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            myAnim.SetBool("arm", !myAnim.GetBool("arm"));
        }
    }

    public void ReloadGun()
    {
        if(Input.GetKeyDown(KeyCode.R) && myAnim.GetBool("arm"))
        {
            myAnim.SetTrigger("reload");

            PlayAudioSource(reloadSFX);

            ammoDesired = ammoMaxClip - ammoClip;
            ammoSupply = ammoDesired < ammo ? ammoDesired : ammo;
            
            ammo -= ammoSupply;
            ammoClip += ammoSupply; 

            Debug.Log("Ammo Left: " + ammo);
            Debug.Log("Ammo in clip " + ammoClip);
            
        }
    }

    public void FireGun()
    {
        if(Input.GetMouseButtonDown(0) && !myAnim.GetBool("fire"))
        {
            if(ammoClip > 0f ) 
            {
                myAnim.SetTrigger("fire"); 
                ammoClip--;
                Debug.LogWarning("Ammo in clip " + ammoClip);
            }
            else if(myAnim.GetBool("arm"))
            {
                PlayAudioSource(triggerSFX);
            }
        }
    }

    // Normalizing a quaternion
    private Quaternion ClampRotationAroundAxis(Quaternion q)
    {
        // Normalising the incoming Quaternion
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, minX, maxX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;

    }

    // Raycast method for ground detection
    private bool isGrounded()
    {
        RaycastHit hit;

        if(Physics.SphereCast(transform.position, myCol.radius, -Vector3.up, out hit, (myCol.height / 2f) - myCol.radius + 0.1f))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Events Sections

    // Collisions
    private void OnCollisionEnter(Collision other) 
    {
        if(isGrounded())
        {
            if(myAnim.GetBool("running") && !playingRunning)
            {
                InvokeRepeating(nameof(PlayFootStepAudio), 0f, footStepInterval);
            }   
        }

        if(other.gameObject.CompareTag("Ammo") && ammo < maxAmmo)
        {
            Destroy(other.gameObject);
            ammo =  Mathf.Clamp(ammo + ammoPickupSize, 0, maxAmmo);
            PlayAudioSource(ammoPickupSFX);
            
            Debug.Log("Got " + ammo + " ammo");
        }

        if(other.gameObject.CompareTag("MedKit") && health < maxHealth)
        {
            Destroy(other.gameObject);
            health = Mathf.Clamp(health + healthPickupAmount, 0, maxHealth);
            PlayAudioSource(healthPickupSFX);

            Debug.Log("Got " + health + " hp");
        }

        if(other.gameObject.CompareTag("Lava"))
        {
            health -= 10;
            if(health <= 0)
            {
                PlayAudioSource(deathSFX);
                health = 0;
            }
            else
            {
                PlayAudioSource(incDamageSFX);
            }

            Debug.Log("Got " + health + " hp");   
        }
    }

    #endregion
 
    #region Audio

    // Audiosource helper method
    public void PlayAudioSource(AudioSource audioSource)
    {
        if(audioSource != null)
        {
            audioSource.Play();
            //Debug.Log("called");
        }

    }

    // Random footsep method
    void PlayFootStepAudio()
    {
        AudioSource audioSource = new AudioSource();

        int n = Random.Range(1, footsteps.Length);

        audioSource = footsteps[n];
        PlayAudioSource(audioSource);
        footsteps[n] = footsteps[0];
        footsteps[0] = audioSource;
        playingRunning = true;
    }


    #endregion

    #region Cursor LockState

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if(!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if(lockCursor)
        {
            InternalLockUpdate();
        }
    }

    public void InternalLockUpdate()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            cursorIsLocked = false;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            cursorIsLocked = true;
        }

        if(cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if(!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
    }



    #endregion


}
