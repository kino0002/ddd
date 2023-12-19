using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJump : MonoBehaviour {
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private Collision coll;

    private bool canDoubleJump;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collision>();
    }

    public void Update() {
        if (Input.GetButtonDown("Jump") && !coll.onGround && canDoubleJump) {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            canDoubleJump = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            canDoubleJump = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            canDoubleJump = false;
        }
    }
}

