using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn; // 배치할 오브젝트(프리팹)
    public int count = 10;           // 배치할 개수
    public Vector3 spacing = new Vector3(2f, 0f, 0f); // 간격 (X축으로 2만큼씩)

    [ContextMenu("Spawn Objects")] // 에디터에서 우클릭으로 실행 가능
    public void Spawn()
    {
        if (prefabToSpawn == null) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = transform.position + (spacing * i);
            GameObject newObj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            newObj.transform.parent = this.transform; // 정리를 위해 부모 오브젝트 하위로 넣음
        }
    }
}