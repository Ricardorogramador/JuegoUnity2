using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Name Bank", fileName = "NameBank")]
public class NameBankSO : ScriptableObject
{
    [Header("Goblins (sin repetir hasta agotar)")]
    public List<string> goblinNames = new();

    [Header("Humanos (pueden repetirse)")]
    public List<string> humanMaleNames = new();
    public List<string> humanFemaleNames = new();
}