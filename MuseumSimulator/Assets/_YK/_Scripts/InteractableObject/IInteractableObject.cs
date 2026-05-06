public interface IInteractableObject
{
    // 상호작용 가능 여부 확인 (조건 체크를 오브젝트 내부로 위임)
    bool CanInteract(string currentItem);
    
    // 상호작용 시작 (즉시 실행 혹은 홀드 시작)
    void OnInteractStart();
    
    // 상호작용 중 (홀드 진행 중 매 프레임 호출)
    void OnInteracting(float deltaTime);
    
    // 상호작용 중단/취소
    void OnInteractCancel();

    string GetInteractText();
}