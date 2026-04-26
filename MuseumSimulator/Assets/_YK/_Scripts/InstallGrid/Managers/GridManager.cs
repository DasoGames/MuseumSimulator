using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public float cellSize = 1.0f;
    
    [Header("Floor Settings")]
    public Transform floorObject; // 유니티 인스펙터에서 바닥 오브젝트를 넣어주세요.

    private Dictionary<Vector2Int, InstallableObject> gridData = new Dictionary<Vector2Int, InstallableObject>();
    
    // 자동으로 계산될 범위
    private float minX, maxX, minZ, maxZ;

    void Awake()
    {
        InitializeGridBounds();
    }

    /// <summary>
    /// 바닥 오브젝트의 실제 크기를 측정하여 그리드 범위를 설정합니다.
    /// </summary>
    private void InitializeGridBounds()
    {
        if (floorObject == null)
        {
            Debug.LogError("Floor Object가 비어있습니다! 직접 범위를 사용하거나 오브젝트를 할당하세요.");
            return;
        }

        // 바닥의 Renderer나 Collider에서 실제 월드 크기를 가져옵니다.
        Bounds floorBounds;
        if (floorObject.TryGetComponent<Renderer>(out var renderer))
        {
            floorBounds = renderer.bounds;
        }
        else if (floorObject.TryGetComponent<Collider>(out var collider))
        {
            floorBounds = collider.bounds;
        }
        else
        {
            Debug.LogError("바닥 오브젝트에 Renderer나 Collider가 없어 크기를 측정할 수 없습니다.");
            return;
        }

        // 계산된 경계값 저장
        minX = floorBounds.min.x;
        maxX = floorBounds.max.x;
        minZ = floorBounds.min.z;
        maxZ = floorBounds.max.z;

        Debug.Log($"그리드 범위 자동 설정 완료: X({minX} ~ {maxX}), Z({minZ} ~ {maxZ})");
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x + 0.001f) / cellSize);
        int z = Mathf.FloorToInt((worldPos.z + 0.001f) / cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 CellToWorld(Vector2Int cellPos)
    {
        return new Vector3(
            cellPos.x * cellSize + (cellSize * 0.5f),
            0,
            cellPos.y * cellSize + (cellSize * 0.5f)
        );
    }

    public bool IsAreaAvailable(List<Vector2Int> cells)
    {
        foreach (var cell in cells)
        {
            // 1. 이미 다른 물체가 점유 중인지 체크
            if (gridData.ContainsKey(cell)) return false;

            // 2. [자동화된 범위 체크] 
            Vector3 centerPos = CellToWorld(cell);

            // 보정치를 주어 바닥 끝에 딱 걸쳤을 때 설치가 안 되는 현상을 방지
            if (centerPos.x < minX || centerPos.x > maxX ||
                centerPos.z < minZ || centerPos.z > maxZ) 
            {
                return false;
            }
        }
        return true;
    }

    // RegisterObject, UnregisterObject, GetObjectAt은 이전과 동일
    public void RegisterObject(InstallableObject obj, List<Vector2Int> cells)
    {
        foreach (var cell in cells) gridData[cell] = obj;
    }

    public void UnregisterObject(List<Vector2Int> cells)
    {
        foreach (var cell in cells) if (gridData.ContainsKey(cell)) gridData.Remove(cell);
    }
}