using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public class CraftingData : ItemData
    {
        // 제작에 필요한 재료를 담은 배열
        public CraftRequirement[] Blueprint => m_Blueprint;

        // 아이템이 제작 가능한지 여부
        public bool IsCraftable => m_Blueprint.Length > 0 && m_CraftAmount > 0;
        // 제작에 걸리는 시간
        public float CraftDuration => m_CraftDuration;
        // 제작할 아이템의 양
        public int CraftAmount => m_CraftAmount;

        // 제작에 필요한 온도 (추가된 내용)
        public int CraftTemperature => m_CraftTemperature;
        // 제작에 필요한 스테이션의 레벨
        public int CraftLevel => m_CraftLevel;

        // 아이템 분해가 가능한지 여부
        public bool AllowDismantle => m_Blueprint.Length > 0 && m_DismantleEfficiency > 0.01f;
        // 분해 효율
        public float DismantleEfficiency => m_DismantleEfficiency;

        // 제작에 필요한 재료 배열
        [Tooltip("A list with all the 'ingredients' necessary to craft this item, it's also used in dismantling.")]
        [SpaceArea, SerializeField, ReorderableList(ListStyle.Lined)]
        private CraftRequirement[] m_Blueprint;

        // 제작할 아이템의 양
        [SerializeField, Range(0, 100)]
        [Help("Note: A craft amount of 0 will disable the ability to craft this item.")]
        private int m_CraftAmount = 1;

        // 제작할 아이템의 필요 온도 (추가된 내용)
        [SerializeField, Range(0, 100)]
        [Help("Note: Build when it reaches a certain temperature.")]
        private int m_CraftTemperature = 0;

        // 제작에 걸리는 시간
        [Tooltip("How much time does it take to craft this item, in seconds.")]
        [SerializeField, Range(0f, 30f), EnableIf(nameof(m_CraftAmount), 0, Comparison = UnityComparisonMethod.Greater)]
        private float m_CraftDuration = 3f;

        // 제작에 필요한 스테이션의 레벨
        [SerializeField, Range(0, 20)]
        [Tooltip("Makes this item only craftable from stations of the same tier.")]
        private int m_CraftLevel = 0;

        [SpaceArea(3f)]

        // 아이템 분해 효율
        [Help("Note: A dismantle efficiency of 0 will disable the ability to dismantle this item." )]
        [Tooltip("An efficiency of 1 will result in getting all of the item back after dismantling, while 0 means that no item from the blueprint will be made available.")]
        [SerializeField, Range(0, 1f)]
        private float m_DismantleEfficiency = 0.75f;


        // 제작에 필요한 재료 배열을 생성하는 메소드
        public CraftRequirement[] CreateCraftRequirements(float durability)
        {
            var req = new CraftRequirement[m_Blueprint.Length];

            for (int i = 0; i < m_Blueprint.Length; i++)
            {
                int requiredAmount = Mathf.Max(Mathf.RoundToInt(m_Blueprint[i].Amount * Mathf.Clamp01((100f - durability) / 100f)), 1);
                req[i] = new CraftRequirement(m_Blueprint[i].Item, requiredAmount);
            }

            return req;
        }
    }
}