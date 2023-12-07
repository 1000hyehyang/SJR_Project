using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace PolymindGames.UISystem
{
    public sealed class CraftingStationUI : WorkstationInspectorBaseUI<CraftStation>
    {
        [Title("Settings (Crafting & Fuel)")]

        [SerializeField]
        private CraftingUI m_CraftingUI;

        //���⼭����
        [Title("Settings (Cooking)")]

        [SerializeField, Range(0f, 10f)]
        private float m_StartFireDuration = 3f;

        [SerializeField, Range(0f, 10f)]
        private float m_StopFireDuration = 3f;

        [Title("References")]

        [SerializeField]
        private FuelSelectorUI m_FuelSelector;

        [SerializeField]
        private Button m_StartFireBtn;

        [SerializeField]
        private Button m_AddFuelBtn;

        [SerializeField]
        private Button m_ExtinguishBtn;

        [SerializeField]
        private TextMeshProUGUI m_FuelHeader;

        [SerializeField]
        private TextMeshProUGUI m_DescriptionText;

        [Title("Audio")]

        [SerializeField]
        private SoundPlayer m_FireStartSound;

        private bool m_FireToggleInProgress;
        private int m_LastFuelItemId;
        //�������

        protected override void OnInspectionStarted(CraftStation workstation)
        {
            // CraftingUI�� SetAvailableCraftingLevels �޼��带 ȣ���Ͽ� ������ ���� ������ ����
            m_CraftingUI.SetAvailableCraftingLevels(workstation.CraftableLevels);

            // ���� ������ ������ 2 �̻��̸�
            if (workstation.CraftableLevels[0] >= 2) // ���� �߰�
            {
                if (!workstation.CookingActive)
                    m_DescriptionText.text = string.Empty;

                //���⼭����
                m_StartFireBtn.gameObject.SetActive(!workstation.CookingActive);
                m_AddFuelBtn.gameObject.SetActive(workstation.CookingActive);
                m_ExtinguishBtn.interactable = workstation.CookingActive;

                m_FuelSelector.AttachToInventory(Player.Inventory);

                workstation.DescriptionChanged += OnCampfireDescriptionUpdate;
                OnCampfireDescriptionUpdate();
                //�������
            }
            else
            {
                m_FuelSelector.gameObject.SetActive(false);
                m_StartFireBtn.gameObject.SetActive(false);
                m_AddFuelBtn.gameObject.SetActive(false);
                m_ExtinguishBtn.gameObject.SetActive(false);
                m_FuelHeader.gameObject.SetActive(false);
                m_DescriptionText.gameObject.SetActive(false);
            }

        }

        protected override void OnInspectionEnded(CraftStation workstation)
        {
            // CraftingUI�� ResetCraftingLevel �޼��带 ȣ���Ͽ� ���� ������ �ʱ�ȭ
            m_CraftingUI.ResetCraftingLevel();

            //���⼭����
            if (m_FireToggleInProgress)
            {
                CustomActionManagerUI.TryCancelAction();
                m_FireToggleInProgress = false;
            }

            if (workstation.CraftableLevels[0] >= 2)
            {
                m_FuelSelector.DetachFromInventory();

                m_Workstation.DescriptionChanged -= OnCampfireDescriptionUpdate;
            }
                
            // �������
        }

        //���⼭����

        protected override void OnAttachment()
        {
            m_StartFireBtn.onClick.AddListener(QueueFireStart);
            m_ExtinguishBtn.onClick.AddListener(QueueFireStop);

            m_AddFuelBtn.onClick.AddListener(TryAddFuel);

            m_DescriptionText.text = string.Empty;
        }

        private void QueueFireStart()
        {
            if (m_Workstation != null && !m_Workstation.CookingActive && m_FuelSelector.SelectedFuel != null)
            {
                m_FireStartSound.Play2D();

                m_LastFuelItemId = m_FuelSelector.SelectedFuel.Item;
                bool removedItemFromStorage = Player.Inventory.RemoveItemsWithId(m_LastFuelItemId, 1) > 0;

                if (removedItemFromStorage)
                {
                    var fireStartParams = new CustomActionManagerUI.AParams("Fire Starting", "Starting Fire...", m_StartFireDuration, true, () => ToggleFire(true), CancelFireQueue);
                    CustomActionManagerUI.TryStartAction(fireStartParams);

                    m_FireToggleInProgress = true;
                }
            }
        }

        private void QueueFireStop()
        {
            if (m_Workstation != null && m_Workstation.CookingActive)
            {
                var fireStartParams = new CustomActionManagerUI.AParams("Fire Extinguish", "Extinguishing Fire...", m_StopFireDuration, true, () => ToggleFire(false), CancelFireQueue);
                CustomActionManagerUI.TryStartAction(fireStartParams);

                m_FireToggleInProgress = true;
            }
        }

        private void CancelFireQueue()
        {
            m_FireToggleInProgress = false;

            if (m_LastFuelItemId != -1)
                Player.Inventory.AddItemsWithId(m_LastFuelItemId, 1);
        }

        private void OnCampfireDescriptionUpdate() => m_DescriptionText.text = m_Workstation.CookingActive ? m_Workstation.Description : string.Empty;

        private void TryAddFuel()
        {
            if (m_Workstation != null && m_Workstation.CookingActive && m_FuelSelector.SelectedFuel != null)
            {
                bool removedItemFromStorage = Player.Inventory.RemoveItemsWithId(m_FuelSelector.SelectedFuel.Item, 1) > 0;

                if (removedItemFromStorage)
                    m_Workstation.AddFuel(m_FuelSelector.SelectedFuel.Duration);
            }
        }

        private void ToggleFire(bool enableFire)
        {
            m_StartFireBtn.gameObject.SetActive(!enableFire);
            m_AddFuelBtn.gameObject.SetActive(enableFire);
            m_ExtinguishBtn.interactable = enableFire;

            if (enableFire)
                m_Workstation.StartCooking(m_FuelSelector.SelectedFuel.Duration);
            else
                m_Workstation.StopCooking();

            m_DescriptionText.text = string.Empty;

            m_FireToggleInProgress = false;
        }
        //�������
    }
}
