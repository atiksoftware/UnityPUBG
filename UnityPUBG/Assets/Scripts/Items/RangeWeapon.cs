﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Items
{
    public class RangeWeapon : Weapon
    {
        public RangeWeapon(int id, string name, ItemRarity rarity, float damage, float attackSpeed, float attackRange, float knockbackPower) : base(id, name, rarity, damage, attackSpeed, attackRange, knockbackPower)
        {
        }
    }
}