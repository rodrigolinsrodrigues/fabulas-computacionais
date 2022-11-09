using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject planet;
    public float speed = 4f;
    public float  jumpHeight = 1.2f; 
    public GameObject guia;

    float gravity = 10;
    public bool onGround = false;

    public Rigidbody rb;

    float distanceToGround;
    Vector3 groundNormal;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Movimento
        float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.Translate(x,0,z);

        // GroundControl
        RaycastHit hit = new RaycastHit();
        if(Physics.Raycast(transform.position, -transform.up, out hit, 10))
        {
            distanceToGround = hit.distance;
            groundNormal = hit.normal;

            if(distanceToGround <= 0.6f)
            {
                onGround = true;
            }
            else
            {
                onGround = false;
            }

            //rotação
            Quaternion toRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
            transform.rotation = toRotation;
        }
    }

    void FixedUpdate()
    {
        // Gravidade
        Vector3 gravDirection = (transform.position - planet.transform.position).normalized;
            if(onGround == false)
            {
                rb.AddForce(gravDirection * -gravity);
            }

        //Pulo
        if(onGround == true && Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(transform.up * 20000 * jumpHeight * Time.deltaTime);
        }
    }

}
