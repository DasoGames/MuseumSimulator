using UnityEngine;

[CreateAssetMenu(fileName = "NewFoodData", menuName = "타이쿤/Food Data")]
public class FoodData : ScriptableObject
{
    [Header("기본 정보")]
    public string foodName;         // 음식/재료 이름
    public int price;               // 상점 구매 가격
    public Sprite icon;             // UI용 아이콘

    [Header("3D 모델링 원본")]
    public GameObject prefab;       // 생성할 3D 프리팹

    [Header("⚙️ [1] 기계/도마 위에 배치될 때 설정")]
    [Tooltip("도마나 냄비에 안착할 때의 고유 크기")]
    public Vector3 spawnScale = Vector3.one; 
    [Tooltip("도마나 냄비에 안착할 때의 고유 회전 각도 (X, Y, Z 순서)")]
    public Vector3 spawnRotation = Vector3.zero;

    [Header("⚙️ [2] 플레이어 손에 들려있을 때 설정")]
    [Tooltip("1인칭 손(오른손 소켓)에 부착될 때의 고유 크기")]
    public Vector3 handScale = Vector3.one;
    [Tooltip("1인칭 손(오른손 소켓)에 부착될 때의 고유 회전 각도 (X, Y, Z 순서)")]
    public Vector3 handRotation = Vector3.zero;
}