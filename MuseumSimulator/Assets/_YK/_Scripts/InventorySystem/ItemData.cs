using UnityEngine;

public enum ItemType { Artifact, Decoration, Facility }

[CreateAssetMenu(fileName = "New Item", menuName = "Museum/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemId;
    public string itemName;
    public ItemType itemType;
    public Sprite itemIcon;
    
    [Header("기획 수치")]
    public int baseGrade;      // 유물 체급 (G_base)
    public float bonusRate;    // 꾸미기 버프율 (BonusRate)
    public int capacity;       // 시설 수용량

    [Header("인게임 배치 데이터")]
    public GameObject placedPrefab; // ⭐ 설치 버튼을 눌렀을 때 생성될 실제 프리팹
}

// 인벤토리 리스트에 들어갈 슬롯 클래스 (일반 클래스이므로 MonoBehaviour 불필요)
[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    public InventorySlot(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
    }
}