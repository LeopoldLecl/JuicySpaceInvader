using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CoupleDatas", menuName = "My Game/Couple Datas")]
public class CoupleDatas : ScriptableObject
{
    public List<IntListWrapper> couplesId = new List<IntListWrapper>
    {
        new IntListWrapper { values = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8} },
        new IntListWrapper { values = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8} },
        new IntListWrapper { values = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8} },
        new IntListWrapper { values = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8} },
        new IntListWrapper { values = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8} }
    };
}

[Serializable]
public class IntListWrapper
{
    public List<int> values;
}
