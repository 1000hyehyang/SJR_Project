using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public class CraftingData : ItemData
    {
        // ���ۿ� �ʿ��� ��Ḧ ���� �迭
        public CraftRequirement[] Blueprint => m_Blueprint;

        // �������� ���� �������� ����
        public bool IsCraftable => m_Blueprint.Length > 0 && m_CraftAmount > 0;
        // ���ۿ� �ɸ��� �ð�
        public float CraftDuration => m_CraftDuration;
        // ������ �������� ��
        public int CraftAmount => m_CraftAmount;

        // ���ۿ� �ʿ��� �µ� (�߰��� ����)
        public int CraftTemperature => m_CraftTemperature;
        // ���ۿ� �ʿ��� �����̼��� ����
        public int CraftLevel => m_CraftLevel;

        // ������ ���ذ� �������� ����
        public bool AllowDismantle => m_Blueprint.Length > 0 && m_DismantleEfficiency > 0.01f;
        // ���� ȿ��
        public float DismantleEfficiency => m_DismantleEfficiency;

        // ���ۿ� �ʿ��� ��� �迭
        [Tooltip("A list with all the 'ingredients' necessary to craft this item, it's also used in dismantling.")]
        [SpaceArea, SerializeField, ReorderableList(ListStyle.Lined)]
        private CraftRequirement[] m_Blueprint;

        // ������ �������� ��
        [SerializeField, Range(0, 100)]
        [Help("Note: A craft amount of 0 will disable the ability to craft this item.")]
        private int m_CraftAmount = 1;

        // ������ �������� �ʿ� �µ� (�߰��� ����)
        [SerializeField, Range(0, 100)]
        [Help("Note: Build when it reaches a certain temperature.")]
        private int m_CraftTemperature = 0;

        // ���ۿ� �ɸ��� �ð�
        [Tooltip("How much time does it take to craft this item, in seconds.")]
        [SerializeField, Range(0f, 30f), EnableIf(nameof(m_CraftAmount), 0, Comparison = UnityComparisonMethod.Greater)]
        private float m_CraftDuration = 3f;

        // ���ۿ� �ʿ��� �����̼��� ����
        [SerializeField, Range(0, 20)]
        [Tooltip("Makes this item only craftable from stations of the same tier.")]
        private int m_CraftLevel = 0;

        [SpaceArea(3f)]

        // ������ ���� ȿ��
        [Help("Note: A dismantle efficiency of 0 will disable the ability to dismantle this item." )]
        [Tooltip("An efficiency of 1 will result in getting all of the item back after dismantling, while 0 means that no item from the blueprint will be made available.")]
        [SerializeField, Range(0, 1f)]
        private float m_DismantleEfficiency = 0.75f;


        // ���ۿ� �ʿ��� ��� �迭�� �����ϴ� �޼ҵ�
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