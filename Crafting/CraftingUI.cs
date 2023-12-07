using PolymindGames.InventorySystem;
using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames.UISystem
{
    public sealed class CraftingUI : PlayerUIBehaviour
    {
        [SerializeField, NotNull]
        private SelectableGroupBaseUI m_CraftingLevelsGroup; // 선택 가능한 제작 레벨

        [SpaceArea]

        [SerializeField, NotNull]
        private Transform m_ItemsSpawnRoot; // 제작 아이템 슬롯이 생성될 위치

        [SerializeField, NotNull]
        private ItemDefinitionSlotUI m_ItemSlotTemplate; // 개별 제작 아이템 슬롯의 템플릿

        [SerializeField, Range(5, 20)]
        private int m_MaxTemplateInstanceCount = 10; // 생성될 제작 아이템 슬롯의 최대 개수

        private ICraftingManager m_CraftingManager;
        private int m_CurrentCraftingLevel = -1; // 현재 선택된 제작 레벨을 추적

        private ItemDefinitionSlotUI[] m_CachedSlots; // 캐시된 제작 아이템 슬롯 배열

        /// <summary>
        /// <para> Key: Crafting level. </para>
        /// Value: List of items that correspond to the crafting level.
        /// </summary>
        private readonly Dictionary<int, List<ItemDefinition>> m_CraftableItemsDictionary = new();
        // 제작 레벨에 기반한 제작 가능한 아이템을 저장하기 위한 딕셔너리
        private int m_CraftingItemsCount; // 제작 가능한 아이템의 총 수


        public void SetAvailableCraftingLevels(params int[] levels) // 사용 가능한 제작 레벨
        {
            var craftingLevels = m_CraftingLevelsGroup.RegisteredSelectables;

            for (int i = 0; i < craftingLevels.Count; i++)
            {
                if (IsPartOfArray(i, levels))
                    craftingLevels[i].gameObject.SetActive(true);
                else
                    craftingLevels[i].gameObject.SetActive(false);
            }

            var highestCraftableLevel = craftingLevels[GetLargestValue(levels)];
            m_CraftingLevelsGroup.SelectSelectable(highestCraftableLevel);
        }

        public void ResetCraftingLevel() => SetAvailableCraftingLevels(0); // 제작 레벨을 0으로 재설정

        protected override void OnAttachment() // 스크립트가 게임 오브젝트에 첨부될 때 호출
        {
            GetModule(out m_CraftingManager);

            var inventoryInspection = GetModule<IInventoryInspectManager>();
            inventoryInspection.AfterInspectionStarted += OnInspectionStarted;
            inventoryInspection.BeforeInspectionEnded += OnInspectionEnded;

            InitializeDictionary();
            InitializeCraftingSlots();

            m_CraftingLevelsGroup.SelectedChanged += OnCraftingLevelSelected;
            SetAvailableCraftingLevels(0);

            void OnCraftingLevelSelected(SelectableUI selectable)
            {
                int craftLevel = m_CraftingLevelsGroup.GetIndexOfSelectable(selectable);
                UpdateCraftingLevel(craftLevel);
            }
        }

        protected override void OnDetachment() // 스크립트가 분리될 때 호출
        {
            var inventoryInspection = GetModule<IInventoryInspectManager>();
            inventoryInspection.AfterInspectionStarted -= RefreshSlots;
            inventoryInspection.BeforeInspectionEnded -= RefreshSlots;
        }

        private void OnInspectionStarted() // 인벤토리 검사 이벤트를 처리
        {
            Player.Inventory.InventoryChanged += RefreshSlots;
            RefreshSlots();
        }

        private void OnInspectionEnded() // 인벤토리 검사 이벤트를 처리
        {
            Player.Inventory.InventoryChanged -= RefreshSlots;
            RefreshSlots();
        }

        private void RefreshSlots() // 인벤토리 변경에 따라 제작 아이템 슬롯을 새로 고침
        {
            for (int i = 0; i < m_CachedSlots.Length; i++)
            {
                var slot = m_CachedSlots[i];
                if (slot.gameObject.activeSelf)
                    slot.SetItem(slot.ItemDef);
            }
        }

        private void UpdateCraftingLevel(int level) // 제작 레벨을 업데이트하고 해당하는 제작 가능한 아이템을 표시
        {
            if (level == m_CurrentCraftingLevel)
                return;

            if (m_CraftableItemsDictionary.TryGetValue(level, out List<ItemDefinition> items))
            {
                SetCurrentlyCraftableItems(items);
                m_CurrentCraftingLevel = level;
            }
        }

        private void SetCurrentlyCraftableItems(List<ItemDefinition> items) //  UI에서 제작 가능한 아이템을 설정
        {
            if (items == null)
            {
                for (int i = 0; i < m_CachedSlots.Length; i++)
                    m_CachedSlots[i].SetNull();
                return;
            }

            int enableCount = items.Count < m_CachedSlots.Length ? items.Count : m_CachedSlots.Length;

            for (int i = 0; i < enableCount; i++)
                m_CachedSlots[i].SetItem(items[i]);

            // Hide the remainning slots.
            if (enableCount < m_CachedSlots.Length)
            {
                int slotStartIndex = items.Count;

                for (int i = slotStartIndex; i < m_CachedSlots.Length; i++)
                    m_CachedSlots[i].SetNull();
            }
        }

        private void InitializeDictionary() // 제작 가능한 아이템의 제작 레벨에 따른 딕셔너리를 초기화
        {
            int craftableItemsCount = 0;

            foreach (var item in ItemDefinition.Definitions)
            {
                if (item.TryGetDataOfType<CraftingData>(out var data) && data.IsCraftable)
                {
                    if (m_CraftableItemsDictionary.TryGetValue(data.CraftLevel, out var list))
                        list.Add(item);
                    else
                    {
                        list = new List<ItemDefinition>() { item };
                        m_CraftableItemsDictionary.Add(data.CraftLevel, list);
                    }

                    craftableItemsCount++;
                }
            }

            m_CraftingItemsCount = craftableItemsCount;
        }

        private void InitializeCraftingSlots()
        {
            // 제한된 수의 인스턴스를 사용하여 슬롯 배열을 초기화
            int instancesCount = Mathf.Min(m_MaxTemplateInstanceCount, m_CraftingItemsCount);
            m_CachedSlots = new ItemDefinitionSlotUI[instancesCount];

            // 슬롯이 생성될 루트 위치
            var spawnRoot = m_ItemsSpawnRoot.transform;

            // 주어진 수만큼 슬롯을 생성하고 초기화
            for (int i = 0; i < instancesCount; i++)
            {
                // 아이템 슬롯 템플릿을 복제하여 새로운 슬롯을 생성
                var slot = Instantiate(m_ItemSlotTemplate, spawnRoot);
                // 슬롯을 비움 (SetNull 메서드 호출)
                slot.SetNull();
                // 슬롯이 선택되었을 때 호출될 이벤트 핸들러 설정
                slot.Selectable.OnSelected += StartCrafting;
                // 생성된 슬롯을 배열에 저장
                m_CachedSlots[i] = slot;
            }

            void StartCrafting(SelectableUI selectable) // 제작 아이템 슬롯이 선택될 때 호출
            {
                // 선택된 슬롯에서 ItemDefinitionSlotUI 컴포넌트를 가져옴
                var itemSlot = selectable.gameObject.GetComponent<ItemDefinitionSlotUI>();
                // 제작 관리자에게 해당 아이템을 제작하도록 지시
                m_CraftingManager.Craft(itemSlot.ItemDef);
            }
        }

        private static bool IsPartOfArray(int refInt, int[] range) // 주어진 정수가 배열의 일부인지 확인
        {
            for (int i = 0; i < range.Length; i++)
            {
                if (refInt == range[i])
                    return true;
            }

            return false;
        }

        private static int GetLargestValue(int[] intArray) // 정수 배열에서 가장 큰 값을 찾음
        {
            int highestValue = -1000000000;

            for (int i = 0; i < intArray.Length; i++)
            {
                if (intArray[i] > highestValue)
                    highestValue = intArray[i];
            }

            return highestValue;
        }
    }
}