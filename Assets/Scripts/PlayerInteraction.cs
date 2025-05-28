using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("오브젝트와의 거리")]
    public float interactDistance = 3f;

    [Header("참조")]
    public LayerMask interactableLayer;
    public Camera cam;
    public Transform player;
    public GameManager gameManager;



    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
            {
                GameObject hitObj = hit.collider.gameObject;

                // ✅ 태그가 Interactable이면 Interact 실행
                if (hitObj.CompareTag("Interactable"))
                {
                    InteractableObject interactable = hitObj.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }
}
