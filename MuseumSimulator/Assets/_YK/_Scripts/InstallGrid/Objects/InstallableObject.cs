using UnityEngine;
using System.Collections.Generic;

public class InstallableObject : MonoBehaviour
{
    [HideInInspector] public ItemData itemData; 
    public Vector2Int dimensions = new Vector2Int(1, 1);
    
    [SerializeField] private MeshRenderer[] renderers;
    private Dictionary<Renderer, Color[]> originalColors = new Dictionary<Renderer, Color[]>();

    void Awake()
    {
        foreach (var r in renderers)
        {
            Color[] colors = new Color[r.materials.Length];
            for (int i = 0; i < r.materials.Length; i++)
            {
                colors[i] = r.materials[i].color;
            }
            originalColors[r] = colors;
        }
    }

    /// <summary>
    /// [수정됨] GridManager를 인자로 받아 월드 좌표 기반으로 정확한 점유 칸을 계산합니다.
    /// PlacementManager의 GetOccupiedCells(gridManager) 호출과 일치합니다.
    /// </summary>
    public List<Vector2Int> GetOccupiedCells(GridManager gridManager)
    {
        // 벽면 설치형은 바닥 그리드 점유 체크 제외
        if (itemData != null && itemData.placementType == PlacementType.Wall)
            return new List<Vector2Int>();

        List<Vector2Int> cells = new List<Vector2Int>();
        float cs = gridManager.cellSize;

        // 1. 모델의 로컬 중심 기준 절반 크기 계산
        Vector3 halfSize = new Vector3((dimensions.x * cs) * 0.5f, 0, (dimensions.y * cs) * 0.5f);
        
        // 2. 현재 회전 상태를 반영한 4개 꼭짓점의 월드 좌표 추출
        Vector3[] corners = new Vector3[4];
        corners[0] = transform.TransformPoint(new Vector3(-halfSize.x, 0, -halfSize.z));
        corners[1] = transform.TransformPoint(new Vector3(halfSize.x, 0, -halfSize.z));
        corners[2] = transform.TransformPoint(new Vector3(-halfSize.x, 0, halfSize.z));
        corners[3] = transform.TransformPoint(new Vector3(halfSize.x, 0, halfSize.z));

        // 3. 4개 꼭짓점이 포함된 그리드 영역의 최소/최대 인덱스 찾기
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;

        foreach (var corner in corners)
        {
            Vector2Int cell = gridManager.WorldToCell(corner);
            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);
            minZ = Mathf.Min(minZ, cell.y);
            maxZ = Mathf.Max(maxZ, cell.y);
        }

        // 4. [정밀 판정] 사각형 영역 내 모든 칸이 실제 물체와 겹치는지 체크
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector2Int currentCell = new Vector2Int(x, z);
                Vector3 cellWorldPos = gridManager.CellToWorld(currentCell);

                // 물체의 로컬 좌표계로 변환하여 크기 안에 들어오는지 확인 (비대칭 방지)
                Vector3 localPoint = transform.InverseTransformPoint(cellWorldPos);

                if (Mathf.Abs(localPoint.x) <= halfSize.x + 0.01f && 
                    Mathf.Abs(localPoint.z) <= halfSize.z + 0.01f)
                {
                    cells.Add(currentCell);
                }
            }
        }
        return cells;
    }

    public void SetPreviewMode(bool isValid)
    {
        Color targetColor = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        foreach (var r in renderers)
            foreach (var mat in r.materials) mat.color = targetColor;
    }

    public void SetPlacedMode()
    {
        foreach (var r in renderers)
        {
            if (originalColors.ContainsKey(r))
            {
                for (int i = 0; i < r.materials.Length; i++)
                    r.materials[i].color = originalColors[r][i];
            }
        }
    }
}