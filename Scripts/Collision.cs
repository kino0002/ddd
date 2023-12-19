using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour {

    [Header("Layers")]
    public LayerMask groundLayer;

    [Space]

    public bool onGround;

    [Space]

    [Header("Collision")]

    public float collisionRadius = 0.25f;
    public Vector2 bottomOffset;
    public float distanceToGround = 0.05f; // distance to ground for ground detection
    private Color debugCollisionColor = Color.red;

    void Update() {
        // Check if the player is within distanceToGround of the ground layer
        RaycastHit2D hit = Physics2D.CircleCast(transform.position + (Vector3)bottomOffset, collisionRadius, Vector2.down, distanceToGround, groundLayer);
        if (hit) {
            // Check if the angle between the hit normal and Vector2.up is less than a threshold (e.g., 45 degrees) to make sure the player is on the ground
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            onGround = angle <= 45f;
        }
        else {
            onGround = false;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset };

        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);
    }
}
