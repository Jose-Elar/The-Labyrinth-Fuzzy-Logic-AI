using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerCC : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float gravity = -10f;

    private CharacterController cc;
    private Vector3 verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 move = (transform.right * h + transform.forward * v).normalized;
        Vector3 horizontal = move * moveSpeed;

        if (cc.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 velocity = horizontal + verticalVelocity;
        cc.Move(velocity * Time.deltaTime);
        //heloworld
    }
}
