﻿using UnityPUBG.Scripts.Entities;
using UnityPUBG.Scripts.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPUBG.Scripts.Utilities;

namespace UnityPUBG.Scripts
{
    public class ItemSpawnManager : Singleton<ItemSpawnManager>
    {
        #region 필드
        [SerializeField] private ItemObject baseItemObject;
        [SerializeField] private ItemCollection itemCollection;

        [SerializeField] [ReadOnly] private List<ItemSpawnPoint> allSpawnPoints;
        #endregion

        #region 유니티 메시지
        private void Awake()
        {
            itemCollection = Instantiate(itemCollection);
            allSpawnPoints = FindAllSpawnPoints();
        }

        private void Start()
        {
            SpawnRandomItemAt(allSpawnPoints);
        }
        #endregion

        #region 메서드
        /// <summary>
        /// ItemSpawnPoint Tag와 Component가 있는 GameObject의 ItemSpawnPoint 리스트를 반환
        /// </summary>
        /// <returns>ItemSpawnPoint의 리스트</returns>
        private List<ItemSpawnPoint> FindAllSpawnPoints()
        {
            return GameObject
                .FindGameObjectsWithTag("ItemSpawnPoint")
                .Where(e => e.GetComponent<ItemSpawnPoint>() != null)
                .Select(e => e.GetComponent<ItemSpawnPoint>())
                .ToList();
        }

        /// <summary>
        /// 매개변수로 받은 ItemSpawnChance 기반으로 무작위 아이템을 선택
        /// </summary>
        /// <param name="spawnChance">스폰 확률 정보</param>
        /// <returns>무작위로 선택된 아이템</returns>
        private Item GetRandomItem(ItemSpawnChance spawnChance)
        {
            if (UnityEngine.Random.value <= spawnChance.SpawnChance)
            {
                ItemRarity randomRarity = spawnChance.GetRandomItemRarity();

                if (itemCollection.ItemsByRarity.TryGetValue(randomRarity, out var items))
                {
                    int randomIndex = UnityEngine.Random.Range(0, items.Count);
                    return items[randomIndex];
                }
                else
                {
                    Debug.LogWarning($"Rarity: {randomRarity}에 해당하는 아이템 컬렉션이 없습니다");
                    return null;
                }
            }
            else
            {
                // 아이템을 스폰하지 않음
                return null;
            }
        }

        /// <summary>
        /// 매개변수로 아이템을 받아 ItemObject를 생성
        /// </summary>
        /// <param name="item">스폰 할 아이템</param>
        /// <returns>생성 된 ItemObject</returns>
        private ItemObject InstantiateItemObject(Item item)
        {
            if (item == null || item.Model == null)
            {
                return null;
            }

            if (baseItemObject == null)
            {
                Debug.LogError("BaseItemObject가 유효하지 않습니다");
                return null;
            }

            var itemObject = Instantiate(baseItemObject);

            itemObject.item = item;
            Instantiate(item.Model, itemObject.transform);

            return itemObject;
        }

        /// <summary>
        /// 매개변수로 받은 스폰 지점들에 무작위 아이템을 스폰
        /// </summary>
        /// <param name="spawnPoints">아이템을 스폰 할 스폰 지점들</param>
        private void SpawnRandomItemAt(List<ItemSpawnPoint> spawnPoints)
        {
            foreach (var spawnPoint in spawnPoints)
            {
                Item randomItem = GetRandomItem(spawnPoint.SpawnChance);
                ItemObject itemObject = InstantiateItemObject(randomItem);

                if (itemObject != null)
                {
                    spawnPoint.SpawnedItem = itemObject;
                }
            }
        }
        #endregion
    }
}