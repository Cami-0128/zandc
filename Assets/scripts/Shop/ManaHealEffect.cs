using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaHealEffect : IShopItemEffect
{
    private int manaAmount;

    public ManaHealEffect(int manaAmount)
    {
        this.manaAmount = manaAmount;
    }

    public void ApplyEffect(PlayerController2D player)
    {
        player.ManaHeal(manaAmount);
    }
}


