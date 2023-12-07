using PolymindGames.BuildingSystem;//
using PolymindGames.WorldManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;//


namespace PolymindGames
{
    public sealed class CraftStation : Workstation, ISaveableComponent//
    {
        // [����]
        public static CraftStation Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        // [����]

        // �迭�� �� ���� ������ ���� �Ӽ� ����
        public int[] CraftableLevels => m_CraftableLevels;

        [Title("Settings (Craft Station)")]

        [SerializeField, ReorderableList(childLabel: "level")]
        [Tooltip("Limits the items that can be crafted to only the ones that have a craft level between the minimum and maximum craft level range.")]
        private int[] m_CraftableLevels = System.Array.Empty<int>();

        public bool CookingActive // ���� �丮�� Ȱ��ȭ�Ǿ� �ִ��� ����
        {
            get => m_CookingIsActive;
            private set
            {
                if (m_CookingIsActive != value)
                {
                    m_CookingIsActive = value;

                    if (m_CookingIsActive)
                    {
                        CookingStarted?.Invoke();
                        StartCoroutine(C_Update());
                    }
                    else
                    {
                        StopAllCoroutines();
                        CookingStopped?.Invoke();
                    }
                }
            }
        }
        
        // ���⼭����

        public float CookingStrength => m_CookingDurationRealtime / m_MaxTemperatureAchieveTime; // ���� �丮�� ����

        private int m_CurrentHeat = 0; // ���÷� �߰��� currentHeat ��

        public int CurrentHeat // *********************
        {
            get => m_CurrentHeat;
            set
            {
                if (m_CurrentHeat != value)
                {
                    m_CurrentHeat = value;
                    Debug.Log($"CurrentHeat changed to {m_CurrentHeat}");
                }
            }
        }

        public event UnityAction CookingStarted;
        public event UnityAction CookingStopped;
        public event UnityAction<float> FuelAdded;

        [Title("Settings (Cooking)")]

        [SerializeField, Range(0.01f, 10f)]
        [Tooltip("Multiplies the effects of any fuel added (heat and added time).")]
        private float m_FuelDurationMod = 1f; // ���� ���� �ð��� ���� ���� ���

        [SpaceArea]

        [Title("Temperature (Cooking)")]

        [SerializeField, Range(1f, 1000f)]
        private float m_MaxTemperatureAchieveTime = 500; // �ִ� �µ��� �����ϴ� �� �ɸ��� �ð�

        [SerializeField, Range(40, 60)]
        public int m_MaxProximityTemperature = 50; // private >> public���� ����

        private float m_InGameDayScale;

        private float m_CookingDurationRealtime;
        private bool m_CookingIsActive = false;


        public void StartCooking(float fuelDuration) // �丮�� ����
        {
            m_CookingDurationRealtime = Mathf.Clamp(m_CookingDurationRealtime + fuelDuration * m_FuelDurationMod, 0f, m_MaxTemperatureAchieveTime);
            CookingActive = true;
        }

        public void StopCooking() // �丮�� ����
        {
            m_CookingDurationRealtime = 0f;
            Description = string.Empty;
            CookingActive = false;
        }

        public void AddFuel(float fuelDuration) // ���Ḧ �߰�
        {
            m_CookingDurationRealtime = Mathf.Clamp(m_CookingDurationRealtime + fuelDuration * m_FuelDurationMod, 0f, m_MaxTemperatureAchieveTime);
            FuelAdded?.Invoke(fuelDuration);
        }

        private void Start()
        {
            if (WorldManagerBase.HasInstance)
            {
                m_InGameDayScale = WorldManagerBase.Instance.GetDayDurationInMinutes() / 1440f;
            }
            else
            {
                // Handle the case where WorldManagerBase.Instance is null
                // You might want to provide a default value or log a warning.
                m_InGameDayScale = WorldManagerBase.k_DefaultDayDurationInMinutes / 1440f;
            }
        }

        private void UpdateDescriptionText() // ���� �ؽ�Ʈ�� ������Ʈ
        {
            // ���� �丮 ���� �ð��� ���� �� ��¥ ô���� ����Ͽ� GameTime ��ü�� ����
            GameTime fireDuration = new(m_CookingDurationRealtime, m_InGameDayScale);
            // ���� �ؽ�Ʈ�� ���� ���� ���ڿ��� ����
            string infoString = $"Duration: {fireDuration.GetTimeToStringWithSuffixes(true, true, false)} \n";
            // ���� �丮�� ������ �ִ� ���� �µ��� ���Ͽ� ���� ���
            infoString += $"Heat: +{Mathf.RoundToInt(CookingStrength * m_MaxProximityTemperature)}C";

            // Ŭ������ Description �Ӽ��� ������ ���� ���ڿ��� ����
            Description = infoString;
        }

        private IEnumerator C_Update()
        {
            yield return null;

            while (m_CookingIsActive)
            {
                if (m_CookingDurationRealtime < 0f)
                    StopCooking();
                else
                    m_CookingDurationRealtime -= Time.deltaTime;

                if (InspectionActive || HoverActive)
                {
                    CurrentHeat = Mathf.RoundToInt(CookingStrength * m_MaxProximityTemperature); // ******************************
                    UpdateDescriptionText();
                }

                yield return null;
            }
        }

        #region Save & Load
        public void LoadMembers(object[] members)
        {
            m_CookingDurationRealtime = (float)members[1];
            CookingActive = (bool)members[2];
        }

        public object[] SaveMembers()
        {
            object[] members = new object[]
            {
                m_CookingDurationRealtime,
                m_CookingIsActive,
            };

            return members;
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

        }
#endif
        #endregion
    }
    // �������
}
