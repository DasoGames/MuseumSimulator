// InventoryManager.cs
using System.Collections.Generic;
using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 획득한 순서대로 쌓이는 유일한 '통합 창고'
    public List<InventorySlot> inventoryList = new List<InventorySlot>();

    // 아이템이 추가/제거될 때 UI에 알려줄 이벤트 (옵저버 패턴)
    public event Action OnInventoryUpdated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 아이템 획득 함수
    public void AddItem(ItemData itemToAdd, int amount = 1)
    {
        // 1. 이미 인벤토리에 있는지 확인 (겹치기)
        foreach (var slot in inventoryList)
        {
            if (slot.item == itemToAdd)
            {
                slot.count += amount;
                OnInventoryUpdated?.Invoke(); // UI 갱신 알림
                return;
            }
        }

        // 2. 없다면 새로 획득한 것이므로 리스트 맨 뒤에 추가 (획득 순서 보장)
        inventoryList.Add(new InventorySlot(itemToAdd, amount));
        OnInventoryUpdated?.Invoke();
    }

    // 아이템 소모/배치 함수
    public void RemoveItem(ItemData itemToRemove, int amount = 1)
    {
        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].item == itemToRemove)
            {
                inventoryList[i].count -= amount;
                if (inventoryList[i].count <= 0)
                {
                    inventoryList.RemoveAt(i); // 다 쓰면 슬롯 삭제
                }
                OnInventoryUpdated?.Invoke();
                return;
            }
        }
    }
}