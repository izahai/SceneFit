using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class MinimapClickTeleport : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private RectTransform minimapRect;

    [Header("XR")]
    [SerializeField] private TeleportationProvider teleportProvider;

    [Header("Minimap Camera")]
    [SerializeField] private Camera minimapCamera;

    [Header("Raycast")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastExtraHeight = 10f;

    void Awake()
    {
        if (!teleportProvider)
            Debug.LogError("TeleportationProvider missing", this);

        if (!minimapCamera || !minimapCamera.orthographic)
        {
            Debug.LogError("Minimap camera must be orthographic", this);
            enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!teleportProvider || !minimapCamera)
            return;

        // Convert screen click → minimap local space
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
            return;

        Rect rect = minimapRect.rect;
        if (!rect.Contains(localPoint))
            return;

        float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        // Convert minimap UV → minimap camera viewport
        Vector3 viewportPoint = new Vector3(u, v, 0f);

        // Ray straight down from minimap camera
        Ray ray = minimapCamera.ViewportPointToRay(viewportPoint);

        float rayDistance = minimapCamera.transform.position.y + raycastExtraHeight;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundMask))
        {
            QueueTeleport(hit.point);
        }
    }

    private void QueueTeleport(Vector3 worldPoint)
    {
        TeleportRequest request = new TeleportRequest
        {
            destinationPosition = worldPoint,
            destinationRotation = Quaternion.identity,
            matchOrientation = MatchOrientation.None
        };

        teleportProvider.QueueTeleportRequest(request);
    }
}
