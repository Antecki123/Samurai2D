using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Compoent References")]
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Controller Properties")]
    [SerializeField, Range(0, 30)] private float runSpeed = 8f;
    [SerializeField, Range(0, 30)] private float jumpSpeed = 4f;
    [Space]
    [SerializeField, Range(0, 30)] private float dashForce = 8f;

    [Header("Sensors References")]
    [SerializeField] private Sensor_HeroKnight groundSensor;
    [SerializeField] private Sensor_HeroKnight wallSensorR1;
    [SerializeField] private Sensor_HeroKnight wallSensorR2;
    [SerializeField] private Sensor_HeroKnight wallSensorL1;
    [SerializeField] private Sensor_HeroKnight wallSensorL2;

    [Header("Debug Utils")]
    [SerializeField] private Text debugLabel;
    [SerializeField] private Camera mainCamera;

    private InputController inputActions;
    private float horizontalMove;
    private float verticalMove;

    private bool isJumping = false;
    private bool isAttacking = false;
    private bool isPushing = false;

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Awake()
    {
        inputActions = new InputController();

        inputActions.Gameplay.HorizontalMovement.performed += ctx => horizontalMove = ctx.ReadValue<float>();
        inputActions.Gameplay.HorizontalMovement.canceled += ctx => horizontalMove = 0;

        inputActions.Gameplay.VerticalMovement.performed += ctx => verticalMove = ctx.ReadValue<float>();
        inputActions.Gameplay.VerticalMovement.canceled += ctx => verticalMove = 0;

        inputActions.Gameplay.Jump.performed += ctx => StartCoroutine(Jump());
        inputActions.Gameplay.Attack.performed += ctx => StartCoroutine(Attack());
    }

    private void Update()
    {
        // Horizontal movement
        if (!isAttacking)
            rb2D.velocity = new Vector2(horizontalMove * runSpeed, rb2D.velocity.y);
        else
            rb2D.velocity = Vector2.zero;

        // Swap direction of sprite depending on walk direction
        if (horizontalMove > 0)
            spriteRenderer.flipX = false;

        else if (horizontalMove < 0)
            spriteRenderer.flipX = true;

        if (transform.position.y < -10f)
            transform.position = Vector3.zero;
    }

    private void LateUpdate()
    {
        // Handle animations
        animator.SetFloat("MovementX", Mathf.Abs(horizontalMove));
        animator.SetFloat("MovementY", rb2D.velocity.y);

        animator.SetBool("Grounded", groundSensor.State());

        if (isJumping)
            animator.SetTrigger("Jump");

        animator.SetBool("Block", isPushing);

        // ============ DEBUG ============
        var camPositionX = Mathf.Clamp(transform.position.x, 0f, 15f);
        var camPositionY = Mathf.Clamp(transform.position.y, 0f, Mathf.Infinity);
        mainCamera.transform.position = new Vector3(camPositionX, camPositionY, mainCamera.transform.position.z);

        debugLabel.text = $" Horizontal Input: {horizontalMove} \n Vertical Input: {verticalMove} \n" +
            $" FPS: {Mathf.Round(1 / Time.deltaTime)} \n" +
            $" isPushing: {isPushing}";
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Check if moveable object is moved
        if (collision.CompareTag("Moveable") && Mathf.Abs(horizontalMove) > 0)
            isPushing = true;
        else
            isPushing = false;
    }

    private IEnumerator Jump()
    {
        var delay = .2f;

        if (groundSensor.State() || wallSensorL1.State() || wallSensorR1.State())
        {
            isJumping = true;
            groundSensor.Disable(delay);
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpSpeed);

            while (isJumping)
            {
                if (groundSensor.State())
                    isJumping = false;

                yield return new WaitForEndOfFrame();
            }
        }
    }

    private IEnumerator Attack()
    {
        var attackTimer = .6f;

        if (!isAttacking)
        {
            isAttacking = true;
            animator.SetTrigger("Attack1");

            while (attackTimer >= 0)
            {
                attackTimer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            isAttacking = false;
        }
    }
}