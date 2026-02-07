using UnityEngine;

[CreateAssetMenu(fileName = "AttackChain", menuName = "Attack System/AttackChain")]
public class AttackChain : ScriptableObject
{
    
    [Header("Chain info")]
    public string ChainName;
    [TextArea(2, 4)]
    public string Description;

    [Header("Attacks in Chain")]
    public AttackData[] Attacks;    

    [Header("Requirements")]
    public float minStamina = 0f;
    public bool requireGrounded = true;
    public bool requireCombatStance = true;

    public AttackData GetStarterAttack()
    {
        if (Attacks != null && Attacks.Length > 0)
        {
            return Attacks[0];
        }
        return null;
    }

    public AttackData GetNextAttack(int currentIndex)
    {
        if(currentIndex >= 0 && currentIndex < Attacks.Length - 1)
        {
            return Attacks[currentIndex + 1];
        } else if(currentIndex == Attacks.Length -1)
        {
            return Attacks[0];
        }
        return null;
    }

}
