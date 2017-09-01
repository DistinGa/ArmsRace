using UnityEngine;
using UnityEditor;

namespace SpaceTechsData
{
    [CustomEditor(typeof(SpaceTechsData))]
    public class TechsEditor : Editor
    {
        SpaceRace SR;

        void OnEnable()
        {
            SR = GameObject.Find("GameManager").GetComponent<GameManagerScript>().Menus[0].GetComponent<SpaceRace>();
            //serializedObject.FindProperty("SR").objectReferenceValue = SR;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(new GUIContent("Export", "Из игры в файл")))
                Export();
            if (GUILayout.Button(new GUIContent("Import", "Из файла в игру")))
                Import();

            DrawDefaultInspector();
        }

        void Export()
        {
            if (SR == null)
            {
                Debug.LogError("Не указан объект TechMenu");
                return;
            }

            Techno[] td = (Selection.activeObject as SpaceTechsData).techs;

            for (int i = 1; i < SpaceRace.TechCount; i++)
            {
                Technology item = SR.GetTechno(i);

                td[i].UsaSprite = item.mUsaSprite;
                td[i].RusSprite = item.mRusSprite;
                td[i].UsaDescr = item.mUsaDescr;
                td[i].RusDescr = item.mRusDescr;
                td[i].Cost = item.mCost;

                td[i].LocalInfl = item.mLocalInfl;
                td[i].GlobalInfl = item.mGlobalInfl;
                td[i].LocalInfl_1 = item.mLocalInfl_1;
                td[i].GlobalInfl_1 = item.mGlobalInfl_1;
            }
        }

        void Import()
        {
            if (SR == null)
            {
                Debug.LogError("Не указан объект TechMenu");
                return;
            }

            Techno[] td = (Selection.activeObject as SpaceTechsData).techs;

            for (int i = 1; i < SpaceRace.TechCount; i++)
            {
                SR.SetTechno(i, td[i].UsaDescr, td[i].RusDescr, td[i].Cost, td[i].LocalInfl, td[i].GlobalInfl, td[i].LocalInfl_1, td[i].GlobalInfl_1, td[i].UsaSprite, td[i].RusSprite);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}