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

    Quaternion camRot;
    Quaternion playerRot;


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
        float x = Input.GetAxis("Horizontal") * turnSpeed;
        float y = Input.GetAxis("Vertical") * turnSpeed;

        Vector3 movement = new Vector3(x, 0, y);
        movement.Normalize();
        
        transform.position += movement * delta * moveSpeed;

        // Look Logic
        float yRot = Input.GetAxis("Mouse X");
        float xRot = Input.GetAxis("Mouse Y");

        camRot *= Quaternion.Euler(-xRot, 0, 0);
        playerRot *= Quaternion.Euler(0, yRot, 0);

        transform.localRotation = playerRot;
        cam.transform.localRotation = camRot;


        // Jump Logic
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            myRb.AddForce(0, jumpHeight, 0);
        }
    }

    bool isGrounded()
    {
        RaycastHit hit;

        if(Physics.SphereCast(transform.position, myCol.radius, -Vector3.up, out hit, (myCol.height / 2f) - myCol.radius + 0.1f))
        {
            return true;
        }

        return false;



    }



}
