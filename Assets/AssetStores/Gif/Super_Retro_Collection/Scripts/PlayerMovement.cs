// -----------------------------------------------------------------------------------------
// using classes
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

// -----------------------------------------------------------------------------------------
// player movement class
public class PlayerMovement1 : MonoBehaviour
{
    // static public members
    public static PlayerMovement1 instance;

    // -----------------------------------------------------------------------------------------
    // public members
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    // -----------------------------------------------------------------------------------------
    // private members
    private Vector2 movement;

    // -----------------------------------------------------------------------------------------
    // awake method to initialisation
    void Awake()
    {
        instance = this;
    }

    void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    // -----------------------------------------------------------------------------------------
    // fixed update methode
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
