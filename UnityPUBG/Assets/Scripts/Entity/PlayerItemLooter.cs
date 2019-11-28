﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPUBG.Scripts.Items;
using UnityPUBG.Scripts.Logic;
using UnityPUBG.Scripts.UI;

namespace UnityPUBG.Scripts.Entities
{
    public class PlayerItemLooter : MonoBehaviour
    {
        [Range(3, 20)] public int maximumLootAtMoment = 10;
        [Range(0.1f, 1f)] public float searchForLootPeriod = 0.2f;
        [Range(1f, 8f)] public float lootRadius = 4f;
        [Range(1f, 3f)] public float autoLootRadius = 1.75f;
        public LayerMask lootMask;

        [Header("UI")]
        public LootButton lootButtonPrefab;

        [Header("Debug")]
        public bool showLootRadius = false;

        private Player player;
        private Collider[] collideObjects;
        private HashSet<ItemObject> currentLootableItemObjects = new HashSet<ItemObject>();
        private HashSet<ItemObject> lastLootableItemObjects = new HashSet<ItemObject>();

        #region 유니티 메시지
        private void Awake()
        {
            player = GetComponentInParent<Player>();
            if (player == null || player.IsMyPlayer == false)
            {
                Destroy(gameObject);
            }

            collideObjects = new Collider[maximumLootAtMoment];
            ObjectPoolManager.Instance.InitializeUIObjectPool(lootButtonPrefab.gameObject, UIManager.Instance.dynamicCanvas, maximumLootAtMoment);
        }

        private void Start()
        {
            StartCoroutine(SearchLootableItem());
        }

        private void OnDrawGizmos()
        {
            if (showLootRadius)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, lootRadius);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, autoLootRadius);
            }
        }
        #endregion

        private bool IsAutoLootTarget(Item lootItem)
        {
            if (lootItem.IsStackEmpty)
            {
                return false;
            }

            // TODO: 실드 내구도가 더 많이 남아있는 경우도 true
            if (lootItem.Data is ArmorData)
            {
                if (player.EquipedArmor.IsStackEmpty || player.EquipedArmor.Data.Rarity < lootItem.Data.Rarity)
                {
                    return true;
                }
            }
            else if (lootItem.Data is BackpackData)
            {
                if (player.EquipedBackpack.IsStackEmpty || player.EquipedBackpack.Data.Rarity < lootItem.Data.Rarity)
                {
                    return true;
                }
            }
            else
            {
                var sameItemAtContainer = player.ItemContainer.TryGetItem(lootItem.Data.ItemName);
                if (sameItemAtContainer.IsStackEmpty == false && sameItemAtContainer.IsStackFull == false)
                {
                    return true;
                }

                //가방이 비었다면
                if(player.ItemContainer.IsFull == false)
                {
                    //먹은적 없는 아이템이라면
                    if(sameItemAtContainer.IsStackEmpty == true)
                    {
                        return true;
                    }

                    //무기라면
                    if(sameItemAtContainer.Data is WeaponData)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private IEnumerator SearchLootableItem()
        {
            float autoLootSquaredRadius = autoLootRadius * autoLootRadius;

            while (true)
            {
                int hitCount = Physics.OverlapSphereNonAlloc(transform.position, lootRadius, collideObjects, lootMask);
                for (int index = 0; index < hitCount; index++)
                {
                    // ItemObject가 유효한지 검사
                    var collideItemObject = collideObjects[index].GetComponent<ItemObject>();
                    if (collideItemObject == null || collideItemObject.AllowLoot == false || collideItemObject.Item.IsStackEmpty)
                    {
                        continue;
                    }

                    // 자동 루팅 대상인지 검사
                    if ((collideItemObject.transform.position - transform.position).sqrMagnitude <= autoLootSquaredRadius && IsAutoLootTarget(collideItemObject.Item))
                    {
                        player.LootItem(collideItemObject);
                    }
                    else
                    {
                        // LootButton이 표시중인지 검사
                        if (collideItemObject.LootButton == null)
                        {
                            // ItemObject에게 새 LootButton을 할당
                            var pooledLootButton = ObjectPoolManager.Instance.ReuseUIObject(lootButtonPrefab.gameObject).GetComponent<LootButton>();
                            pooledLootButton.targetItemObject = collideItemObject;
                            collideItemObject.LootButton = pooledLootButton;

                            currentLootableItemObjects.Add(collideItemObject);
                        }
                        else
                        {
                            lastLootableItemObjects.Remove(collideItemObject);
                            currentLootableItemObjects.Add(collideItemObject);
                        }
                    }
                }

                // 루팅 범위 밖의 ItemObject가 가진 LootButton 제거
                foreach (var item in lastLootableItemObjects)
                {
                    if (item != null && item.LootButton != null)
                    {
                        item.LootButton.SaveToPool();
                        item.LootButton = null;
                    }
                }

                // currentLootable을 lastLootable로 이동
                var tempForSwap = lastLootableItemObjects;
                lastLootableItemObjects = currentLootableItemObjects;
                currentLootableItemObjects = tempForSwap;
                currentLootableItemObjects.Clear();

                yield return new WaitForSeconds(searchForLootPeriod);
            }
        }
    }
}
