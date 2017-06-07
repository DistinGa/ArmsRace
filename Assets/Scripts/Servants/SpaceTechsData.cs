using UnityEngine;

namespace SpaceTechsData
{
    [CreateAssetMenu(fileName = "SpaceTechsData", menuName = "Create SpaceTechsData object", order = 0)]
    public class SpaceTechsData : ScriptableObject
    {
        public Techno[] techs = new Techno[SpaceRace.TechCount];
    }

    [System.Serializable]
    public class Techno
    {
        [Tooltip("Картинка американской технологии")]
        public Sprite UsaSprite = null;
        [Tooltip("Картинка советской технологии")]
        public Sprite RusSprite = null;
        [TextArea(2, 5)]
        [Tooltip("Описание американской технологии")]
        public string UsaDescr;  // текстовое описание
        [TextArea(2, 5)]
        [Tooltip("Описание советской технологии")]
        public string RusDescr;
        [Tooltip("Стоимость изучения технологии")]
        public int Cost; // стоимость технологии
        [Tooltip("Увеличение локального влияния")]
        public int LocalInfl;
        [Tooltip("Увеличение глобального влияния")]
        public int GlobalInfl;
        [Tooltip("Увеличение локального влияния при первенстве в изучении")]
        public int LocalInfl_1;
        [Tooltip("Увеличение глобального влияния при первенстве в изучении")]
        public int GlobalInfl_1;
    }
}