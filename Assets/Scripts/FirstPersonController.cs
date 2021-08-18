using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 6f;
    public float jumpHeight = 100f;

    Rigidbody myRb;
    CapsuleCollider myCol;

    [Header("Camera Settings")]
    public Camera cam;
    public float turnSpeed = 5f;
    public float minX = -90f;
    public float maxX = 90f;

    Quaternion camRot;
    Quaternion playerRot;

    bool cursorIsLocked = true;
    bool lockCursor = true;


    // Start is called before the first frame update
    void Start()
    {
        myRb = (Rigidbody) GetComponent("Rigidbody");
        myCol = (CapsuleCollider) GetComponent("CapsuleCollider");

        camRot = cam.transform.localRotation;
        playerRot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // Movement Logic
        float delta = Time.deltaTime;
        float x = Input.GetAxis("Horizontal") * delta * moveSpeed;
        float y = Input.GetAxis("Vertical") * delta * moveSpeed;
        
        transform.position += cam.transform.forward * y  + cam.transform.right * x;
        
        // Look Logic
        float yRot = Input.GetAxis("Mouse X")  * turnSpeed;
        float xRot = Input.GetAxis("Mouse Y")  * turnSpeed;

        camRot *= Quaternion.Euler(-xRot, 0, 0);
        playerRot *= Quaternion.Euler(0, yRot, 0);

        camRot = ClampRotationAroundAxis(camRot);

        transform.localRotation = playerRot;
        cam.transform.localRotation = camRot;


        // Jump Logic
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            myRb.AddForce(0, jumpHeight, 0);
        }

        // Cursor Check
        UpdateCursorLock();

    }

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

    private bool isGrounded()
    {
        RaycastHit hit;

        if(Physics.SphereCast(transform.position, myCol.radius, -Vector3.up, out hit, (myCol.height / 2f) - myCol.radius + 0.1f))
        {
            return true;
        }

        return false;
    }

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



    



}
