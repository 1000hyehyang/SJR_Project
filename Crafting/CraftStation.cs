using PolymindGames.BuildingSystem;//
using PolymindGames.WorldManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;//


namespace PolymindGames
{
    public sealed class CraftStation : Workstation, ISaveableComponent//
    {
        // [수정]
        public static CraftStation Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        // [수정]

        // 배열로 된 제작 가능한 레벨 속성 정의
        public int[] CraftableLevels => m_CraftableLevels;

        [Title("Settings (Craft Station)")]

        [SerializeField, ReorderableList(childLabel: "level")]
        [Tooltip("Limits the items that can be crafted to only the ones that have a craft level between the minimum and maximum craft level range.")]
        private int[] m_CraftableLevels = System.Array.Empty<int>();

        public bool CookingActive // 현재 요리가 활성화되어 있는지 여부
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
        
        // 여기서부터

        public float CookingStrength => m_CookingDurationRealtime / m_MaxTemperatureAchieveTime; // 현재 요리의 강도

        private int m_CurrentHeat = 0; // 예시로 추가된 currentHeat 값

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
        private float m_FuelDurationMod = 1f; // 연료 지속 시간에 대한 수정 계수

        [SpaceArea]

        [Title("Temperature (Cooking)")]

        [SerializeField, Range(1f, 1000f)]
        private float m_MaxTemperatureAchieveTime = 500; // 최대 온도에 도달하는 데 걸리는 시간

        [SerializeField, Range(40, 60)]
        public int m_MaxProximityTemperature = 50; // private >> public으로 수정

        private float m_InGameDayScale;

        private float m_CookingDurationRealtime;
        private bool m_CookingIsActive = false;


        public void StartCooking(float fuelDuration) // 요리를 시작
        {
            m_CookingDurationRealtime = Mathf.Clamp(m_CookingDurationRealtime + fuelDuration * m_FuelDurationMod, 0f, m_MaxTemperatureAchieveTime);
            CookingActive = true;
        }

        public void StopCooking() // 요리를 중지
        {
            m_CookingDurationRealtime = 0f;
            Description = string.Empty;
            CookingActive = false;
        }

        public void AddFuel(float fuelDuration) // 연료를 추가
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

        private void UpdateDescriptionText() // 설명 텍스트를 업데이트
        {
            // 현재 요리 지속 시간과 게임 내 날짜 척도를 사용하여 GameTime 객체를 생성
            GameTime fireDuration = new(m_CookingDurationRealtime, m_InGameDayScale);
            // 설명 텍스트에 사용될 정보 문자열을 생성
            string infoString = $"Duration: {fireDuration.GetTimeToStringWithSuffixes(true, true, false)} \n";
            // 현재 요리의 강도와 최대 근접 온도를 곱하여 열을 계산
            infoString += $"Heat: +{Mathf.RoundToInt(CookingStrength * m_MaxProximityTemperature)}C";

            // 클래스의 Description 속성을 생성된 정보 문자열로 설정
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
    // 여기까지
}
