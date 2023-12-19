using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private AnimationScript anim;
    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public int maxJumps = 1; // maximum number of jumps in the air
    private int jumpsLeft; // number of remaining jumps in the air

    [Space]
    [Header("Booleans")]
    public bool canMove;

    [Space]

    private bool groundTouch;

    public int side = 1;

    [Space]
    [Header("Polish")]
    public ParticleSystem jumpParticle;


    // Start is called before the first frame update
    void Start() {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();

        // initialize the jumpsLeft variable
        jumpsLeft = maxJumps;
    }


    // Update is called once per frame
    void Update() {

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);
        anim.SetHorizontalMovement(x, y, rb.velocity.y);

        if (Input.GetButtonDown("Jump")) {
            if (coll.onGround) {
                jumpsLeft = maxJumps;
                anim.SetTrigger("jump");
            }
            else if (jumpsLeft > 0) {
                jumpsLeft--;
                anim.SetTrigger("jump");
            }
            Jump(Vector2.up, false);
        }

        if (coll.onGround && !groundTouch) {
            GroundTouch();
            groundTouch = true;
        }

        if (!coll.onGround && groundTouch) {
            groundTouch = false;
        }

        if (x > 0) {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0) {
            side = -1;
            anim.Flip(side);
        }
    }

    void GroundTouch() {
        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();

        // Reset jumps left when player touches the ground
        jumpsLeft = maxJumps;
    }


    private void Walk(Vector2 dir) {
        if (!canMove)
            return;

        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
    }

    private void Jump(Vector2 dir, bool wall) {
        if (jumpsLeft > 0) {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity += dir * jumpForce;
            jumpsLeft--;
        }
    }

}

