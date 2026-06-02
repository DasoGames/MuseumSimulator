using UnityEngine;

[System.Serializable]
public struct HoldableObject
{
    [Header("물체 정보")]
    public string objectName;      // 물체 이름 (ex: "생닭", "치킨조각", "닭강정")
    
    // ⭐ [에러 해결을 위해 추가] 냉장고 UI 등에서 그림을 그릴 수 있게 이미지를 품어줍니다.
    [Header("UI 표시 정보")]
    public Sprite icon;            

    [Header("시각적 실체")]
    public GameObject prefab;      
}
