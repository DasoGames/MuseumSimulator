using UnityEngine;

/// <summary>
/// 유물의 설치 방식(타입)을 정의합니다.
/// </summary>
public enum PlacementType
{
    Floor, // 바닥 설치형 (그리드 점유, 45도 회전 가능)
    Wall   // 벽면 설치형 (그리드 점유 제외, 높이 고정, 벽면 스냅)
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Museum/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    public string itemName;            // 유물 이름
    [TextArea]
    public string description;         // 유물 설명

    [Header("Placement Settings")]
    public PlacementType placementType; // 설치 타입 (Floor 또는 Wall)
    public Vector2Int dimensions = new Vector2Int(1, 1); // 점유 칸수 (바닥 설치 시에만 중요)
    
    [Header("Visual Resources")]
    public GameObject prefab;          // InstallableObject 스크립트가 붙은 실제 프리팹
    public Sprite icon;                // 상점이나 도감 UI에 표시될 아이콘

    [Header("Gameplay Data (Optional)")]
    public int purchaseCost;           // 구매 비용
    public int appealPoints;           // 관람객 매력도
}