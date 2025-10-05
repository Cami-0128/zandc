using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealEffect : IShopItemEffect
{
    private int healAmount;

    public HealEffect(int healAmount)
    {
        this.healAmount = healAmount;
    }

    public void ApplyEffect(PlayerController2D player)
    {
        player.Heal(healAmount);
    }
}

