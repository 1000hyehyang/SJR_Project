using PolymindGames.UISystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.InventorySystem
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/crafting#crafting-manager-module")]
    public sealed class CraftingManager : CharacterBehaviour, ICraftingManager
    {
        public bool IsCrafting => m_CurrentItemToCraft != null; // 현재 제작 프로세스가 진행 중인지 여부

        public event UnityAction<ItemDefinition> CraftingStart; // 제작이 시작 및 종료될 때 트리거되는 이벤트
        public event UnityAction CraftingEnd;

        [SerializeField]
        [Tooltip("Craft Sound: Sound that will be played after crafting an item.")]
        private StandardSound m_CraftSound; // 아이템 제작 시 재생되는 사운드

        private ItemDefinition m_CurrentItemToCraft; // 현재 제작 중인 아이템

        private IInventory m_Inventory; // 플레이어 인벤토리를 나타내는 참조
        private IItemDropHandler m_ItemDropHandler; // 아이템 드롭 핸들러를 나타내는 참조 
        private CraftStation m_CraftStation; // [수정]

        protected override void OnBehaviourEnabled() // 동작이 활성화될 때 호출
        {
            GetModule(out m_ItemDropHandler);
            GetModule(out m_Inventory);
            m_CraftStation = CraftStation.Instance;
        }

        public void Craft(ItemDefinition itemDef) // 특정 아이템 정의에 대한 제작 프로세스를 시작
        {
            // 현재 제작 중인지 여부를 확인합니다. 이미 제작 중인 경우, 더 이상의 제작을 방지하고 함수를 종료
            if (IsCrafting)
                return;

            // 아이템이 제작 데이터 (CraftingData)를 가지고 있는지 확인
            // 아이템 데이터를 가져오고, 가져오기에 성공하면 crafData 변수에 해당 데이터가 저장
            if (itemDef.TryGetDataOfType<CraftingData>(out var crafData))
            {
                var blueprint = crafData.Blueprint;

                // CraftingData에서 가져온 blueprint을 사용하여 제작에 필요한 재료를 확인
                for (int i = 0; i < blueprint.Length; i++)
                {
                    int itemCount = m_Inventory.GetItemsWithIdCount(blueprint[i].Item); // 인벤토리에서 해당 재료가 충분한지 확인하고, 재료가 충분하지 않으면 함수를 종료
                    if (itemCount < blueprint[i].Amount)
                        return;
                }

                Debug.Log($"CraftingManager의 currentHeat(현재 온도) : {m_CraftStation.CurrentHeat} \n 도달해야 하는 온도 : {crafData.CraftTemperature}");
                // 현재 제작 중인 아이템의 제작에 필요한 온도가 CraftTemperature보다 큰지 확인
                if (m_CraftStation.CurrentHeat >= crafData.CraftTemperature)
                {
                    // Start crafting
                    m_CurrentItemToCraft = itemDef; // 제작이 가능하면 m_CurrentItemToCraft에 현재 제작 중인 아이템을 저장
                                                    // CustomActionManagerUI를 사용하여 제작 프로세스를 시작하고 UI를 업데이트
                                                    // UI에 표시할 제작 중인 아이템의 이름과 진행 상태를 나타내는 텍스트를 생성
                    var craftingParams = new CustomActionManagerUI.AParams("Crafting", "Crafting <b>" + itemDef.Name + "</b>...", crafData.CraftDuration, true, OnCraftItemEnd, OnCraftCancel);
                    CustomActionManagerUI.TryStartAction(craftingParams);

                    CraftingStart?.Invoke(itemDef); // 제작이 시작되었음을 알림

                    Character.AudioPlayer.PlaySound(m_CraftSound); // 제작 사운드를 재생
                }
                else
                {
                    // 제작에 필요한 온도에 도달하지 못했을 때의 처리 (예: 메시지 출력 또는 다른 조치)
                    Debug.Log("아직 충분히 뜨겁지 않습니다.");
                }

                /*
                // Start crafting
                m_CurrentItemToCraft = itemDef; // 제작이 가능하면 m_CurrentItemToCraft에 현재 제작 중인 아이템을 저장
                // CustomActionManagerUI를 사용하여 제작 프로세스를 시작하고 UI를 업데이트
                // UI에 표시할 제작 중인 아이템의 이름과 진행 상태를 나타내는 텍스트를 생성
                var craftingParams = new CustomActionManagerUI.AParams("Crafting", "Crafting <b>" + itemDef.Name + "</b>...", crafData.CraftDuration, true, OnCraftItemEnd, OnCraftCancel);
                CustomActionManagerUI.TryStartAction(craftingParams);

                CraftingStart?.Invoke(itemDef); // 제작이 시작되었음을 알림

                Character.AudioPlayer.PlaySound(m_CraftSound); // 제작 사운드를 재생
                */
            }
        }

        public void CancelCrafting() // 현재 제작 프로세스를 취소
        {
            if (IsCrafting)
                CustomActionManagerUI.TryCancelAction();
        }

        private void OnCraftItemEnd()
        {
            // 만약 현재 아이템을 제작 중이 아니라면 함수를 종료
            if (!IsCrafting)
                return;

            // m_CurrentItemToCraft에 저장된 현재 제작 중인 아이템의 제작 데이터 (CraftingData)를 가져옴
            if (m_CurrentItemToCraft.TryGetDataOfType<CraftingData>(out var craftData))
            {
                var blueprint = craftData.Blueprint;

                for (int i = 0; i < blueprint.Length; i++)
                {
                    // 제작에 사용된 재료를 인벤토리에서 제거
                    int removedCount = m_Inventory.RemoveItemsWithName(blueprint[i].Item, blueprint[i].Amount);

                    // 모든 필요한 재료가 제거되지 않으면 제작 프로세스가 중단
                    if (removedCount < blueprint[i].Amount)
                        return;
                }

                // 제작이 완료되면 제작된 아이템을 인벤토리에 추가
                int addedItems = m_Inventory.AddItemsWithId(m_CurrentItemToCraft.Id, craftData.CraftAmount);

                // 인벤토리에 추가할 수 없을 경우, 아이템을 월드에 떨어뜨림
                if (addedItems < craftData.CraftAmount)
                    m_ItemDropHandler.DropItem(new Item(m_CurrentItemToCraft, craftData.CraftAmount - addedItems));

                // _CurrentItemToCraft를 null로 초기화하여 현재 제작 중인 아이템을 없앰
                m_CurrentItemToCraft = null;
                // 제작이 완료되었음을 알리는 이벤트를 트리거
                CraftingEnd?.Invoke();
            }
        }

        private void OnCraftCancel() // 제작 프로세스가 취소된 경우 호출
        {
            m_CurrentItemToCraft = null;
            CraftingEnd?.Invoke();
        }
    }
}