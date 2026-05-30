using UnityEngine;

// 💡 menuString을 menuName으로 변경했습니다!
[CreateAssetMenu(fileName = "NewFoodData", menuName = "타이쿤/Food Data")]
public class FoodData : ScriptableObject
{
    [Header("기본 정보")]
    public string foodName;         // 음식/재료 이름 (ex: "밀가루 반죽", "팥", "과일")
    public int price;               // 상점 구매 가격
    public Sprite icon;             // UI에 표시될 아이콘 이미지

    [Header("3D 연출 정보")]
    public GameObject prefab;       // 플레이어 손이나 기계 위에 스폰될 3D 모델링 프리팹
}