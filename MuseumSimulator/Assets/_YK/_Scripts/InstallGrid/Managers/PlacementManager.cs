using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public GridManager gridManager;
    
    [Header("Layer Settings")]
    public LayerMask floorLayer;
    public LayerMask wallLayer;

    [Header("Wall Settings")]
    public float wallInstallHeight = 1.5f;

    [Header("Rotation Settings")]
    public float dragThreshold = 50f;
    private Vector3 dragStartPosition;
    private float targetRotationY = 0f;
    private bool isDragging = false;

    private InstallableObject currentPreview;
    private ItemData currentData;

    // [추가] 생성된 직후에 바로 설치되는 것을 방지하는 변수
    private bool canPlaceThisFrame = false;

    public void StartPlacement(ItemData data)
    {
        if (currentPreview != null) Destroy(currentPreview.gameObject);
        
        currentData = data;
        isDragging = false;
        targetRotationY = 0f; 
        
        // 1. 즉시 설치 방지: 이번 프레임에는 클릭을 무시합니다.
        canPlaceThisFrame = false;
        
        GameObject obj = Instantiate(data.prefab);
        currentPreview = obj.GetComponent<InstallableObject>();

        if (currentPreview != null)
        {
            currentPreview.itemData = data;
            currentPreview.dimensions = data.dimensions; 
        }

        // 2. 생성 직후 즉시 마우스 위치를 한 번 계산해서 0, -100에 박히는 걸 방지
        HandleMovement();
    }

    void Update()
    {
        if (currentPreview == null || currentData == null) return;

        // 마우스 클릭을 뗐을 때 즉시 설치되는 것을 방지하기 위해 
        // 마우스를 누르고 있지 않을 때만 설치 가능 상태로 전환
        if (!Input.GetMouseButton(0)) 
        {
            canPlaceThisFrame = true;
        }

        HandleMovement();
        
        if (currentData.placementType == PlacementType.Floor)
            HandleRotationAndPlacement();
        else
            HandleWallPlacement();
        
        if (Input.GetMouseButtonDown(1)) CancelPlacement();
    }

private void HandleMovement()
{
    if (currentPreview == null || currentData == null) return;

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    float cs = gridManager.cellSize;

    // 1. 아이템 타입에 맞는 레이어 결정
    bool isWallType = (currentData.placementType == PlacementType.Wall);
    LayerMask targetLayer = isWallType ? wallLayer : floorLayer;

    // 2. 레이캐스트 시도
    if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
    {
        if (isWallType)
        {
            // [벽 설치형] 벽을 가리키고 있을 때만 위치를 업데이트합니다.
            currentPreview.transform.position = CalculateWallSnap(hit, cs);
            currentPreview.transform.rotation = Quaternion.LookRotation(hit.normal);
            currentPreview.SetPreviewMode(true); 
        }
        else
        {
            // [바닥 설치형] 바닥을 가리키고 있을 때만 그리드 스냅하며 이동합니다.
            Vector2Int targetCell = gridManager.WorldToCell(hit.point);
            currentPreview.transform.position = gridManager.CellToWorld(targetCell);
            currentPreview.transform.rotation = Quaternion.Euler(0, targetRotationY, 0);

            var cells = currentPreview.GetOccupiedCells(gridManager);
            currentPreview.SetPreviewMode(gridManager.IsAreaAvailable(cells));
        }
    }
    else
    {
        // 3. 레이캐스트 실패 시 (벽 아이템인데 바닥을 보거나, 그 반대일 때)
        // 아무것도 하지 않습니다. (위치를 업데이트하지 않으므로 유물은 그 자리에 멈춰있게 됩니다.)
        
        // 다만, 사용자에게 '설치 불가능' 상태임을 알리기 위해 프리뷰 색상만 빨간색으로 바꿉니다.
        currentPreview.SetPreviewMode(false);

        // [로그 최적화] 콘솔이 지저분해지지 않도록 실패 로그는 주석 처리하거나 필요할 때만 찍습니다.
        // Debug.Log("유효하지 않은 설치 영역입니다.");
    }
}

    private void HandleRotationAndPlacement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = Input.mousePosition;
            isDragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            float deltaX = Input.mousePosition.x - dragStartPosition.x;
            if (Mathf.Abs(deltaX) > dragThreshold)
            {
                isDragging = true;
                targetRotationY += Mathf.Sign(deltaX) * 45f;
                dragStartPosition = Input.mousePosition;
            }
        }

        // [수정] canPlaceThisFrame이 true일 때만 설치 시도
        if (Input.GetMouseButtonUp(0) && !isDragging && canPlaceThisFrame)
        {
            TryPlaceFloor();
        }
    }

    private void HandleWallPlacement()
    {
        // [수정] canPlaceThisFrame이 true일 때만 설치 시도
        if (Input.GetMouseButtonUp(0) && canPlaceThisFrame) 
        {
            PlaceFinal();
        }
    }

    private void TryPlaceFloor()
    {
        var cells = currentPreview.GetOccupiedCells(gridManager);
        if (gridManager.IsAreaAvailable(cells))
        {
            gridManager.RegisterObject(currentPreview, cells);
            PlaceFinal();
        }
    }

    private Vector3 CalculateWallSnap(RaycastHit hit, float cs)
    {
        Vector3 snappedPos = hit.point;
        snappedPos.y = wallInstallHeight;
        snappedPos += hit.normal * 0.02f; // 벽에서 살짝 띄움
        return snappedPos;
    }

    private void PlaceFinal()
    {
        currentPreview.SetPlacedMode();
        currentPreview = null;
        currentData = null;
    }

    private void CancelPlacement()
    {
        if (currentPreview != null) Destroy(currentPreview.gameObject);
        currentPreview = null;
        currentData = null;
    }
}