using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    [Header("Paramètres de vitesse")]
    public float lookSpeed = 2.0f;
    public float moveSpeed = 20.0f;
    public float sprintMultiplier = 5.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // Verrouille et masque le curseur au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;
    }

    void Update()
    {
        // 1. Rotation (Souris) - L'axe de la souris reste standard
        rotationY += Input.GetAxis("Mouse X") * lookSpeed;
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);

        // 2. Multiplicateur de vitesse
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier;
        }

        // 3. Déplacement avec touches explicites (ZQSD / WASD)
        Vector3 movementDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W)) movementDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) movementDirection += Vector3.back;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A)) movementDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) movementDirection += Vector3.right;

        // Normalise le vecteur pour ne pas aller plus vite en diagonale
        if (movementDirection.magnitude > 1f)
        {
            movementDirection.Normalize();
        }

        transform.Translate(movementDirection * currentSpeed * Time.deltaTime);

        // 4. Déplacement vertical
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Vector3.up * currentSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position -= Vector3.up * currentSpeed * Time.deltaTime;
        }

        // 5. Déverrouiller la souris
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}