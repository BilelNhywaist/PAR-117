using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetCharacterController : MonoBehaviour
{
    [Header("Références")]
    public Transform planetTransform; // Glissez l'objet "Planet" ici

    [Header("Paramètres de Mouvement")]
    public float moveSpeed = 10f;
    public float jumpForce = 7f;
    public float gravityStrength = 20f;
    public float rotationSpeed = 15f;

    private Rigidbody rb;
    private Transform cachedTransform;
    private bool isGrounded;

    void Start()
    {
        cachedTransform = transform;
        rb = GetComponent<Rigidbody>();

        // Désactivation de la gravité d'Unity pour la remplacer par notre gravité sphérique
        rb.useGravity = false;
        rb.freezeRotation = true; // Empêche le Rigidbody de basculer de manière chaotique
    }

    void FixedUpdate()
    {
        if (planetTransform == null) return;

        // 1. Calcul de la direction de la gravité (du personnage vers le centre de la planète)
        Vector3 gravityUp = (cachedTransform.position - planetTransform.position).normalized;
        Vector3 localUp = cachedTransform.up;

        // 2. Application d'une force de gravité personnalisée
        rb.AddForce(-gravityUp * gravityStrength, ForceMode.Acceleration);

        // 3. Alignement de la rotation du personnage sur la sphère (les pieds collés au sol)
        Quaternion targetRotation = Quaternion.FromToRotation(localUp, gravityUp) * cachedTransform.rotation;
        cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // 4. Gestion des déplacements (ZQSD / WASD)
        float moveX = Input.GetAxis("Horizontal"); // Q/D ou A/D
        float moveZ = Input.GetAxis("Vertical");   // Z/S ou W/S

        // Calcul des vecteurs de mouvement orientés par rapport à la caméra et à la surface
        Vector3 forward = Vector3.Cross(cachedTransform.right, gravityUp);
        Vector3 right = Vector3.Cross(gravityUp, forward);
        Vector3 moveDir = (forward * moveZ + right * moveX).normalized;

        // Application de la vitesse de déplacement tout en conservant la vélocité verticale (chute/saut)
        Vector3 horizontalVelocity = moveDir * moveSpeed;
        Vector3 verticalVelocity = Vector3.Project(rb.linearVelocity, gravityUp);

        rb.linearVelocity = horizontalVelocity + verticalVelocity;

        // 5. Détection du sol pour le saut
        CheckGrounded(gravityUp);

        // 6. Saut
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(gravityUp * jumpForce, ForceMode.Impulse);
        }
    }

    void CheckGrounded(Vector3 gravityUp)
    {
        // Lance un petit raycast vers le centre de la planète pour détecter le terrain
        float rayLength = 1.1f;
        isGrounded = Physics.Raycast(cachedTransform.position, -gravityUp, rayLength);
    }
}