using UnityEngine;

[System.Serializable]
public struct IngredientData
{
    [Header("상점 표시 정보")]
    public string ingredientName;  
    public Sprite icon;            
    public int price;              

    // ⭐ [핵심 변경] 프리팹을 직접 연결하는 대신, 구매 완료 시 냉장고에 저장될 '초기 구조체 데이터'를 인스펙터에서 직접 세팅하도록 합니다.
    [Header("구매 시 냉장고로 들어갈 데이터")]
    public HoldableObject initialHoldableData;   
}