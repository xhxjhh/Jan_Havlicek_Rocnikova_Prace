using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Hero Roster", fileName = "HeroRoster")]
public class HeroRoster : ScriptableObject
{
    [System.Serializable]
    public class HeroEntry
    {
        public string displayName;
        public Sprite portrait;
        public GameObject prefab;
    }

    public List<HeroEntry> heroes = new List<HeroEntry>();
    public int defaultIndex = 0;
}

