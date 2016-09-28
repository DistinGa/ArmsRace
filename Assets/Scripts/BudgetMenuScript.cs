using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BudgetMenuScript : MonoBehaviour {
    public RectTransform ChartPanel;
    public RectTransform LinePrefab;
    public RectTransform YearTick;

    void OnEnable()
    {
        DrawChart();
    }

    void DrawChart()
    {
        int YearsAmount = 10;   //количество лет на графике
        Color redBrush = new Color(1, 0, 0);
        Color blueBrush = new Color(0, 0, 1);

        float xScale, yScale, yOffset;

        //Сначала удалим предыдущие графики
        while (ChartPanel.childCount > 0)
            DestroyImmediate(ChartPanel.GetChild(0).gameObject);

        xScale = ChartPanel.rect.width / (YearsAmount - 1);

        //Определяем начальный элемент истории
        int FirstInd = GameManagerScript.GM.Player.History.Count - YearsAmount;
        if (FirstInd < 0)
            FirstInd = 0;

        //Рисуем годы на графике
        int InitYear = 51 + FirstInd;   //51-й год - первый, где есть статистика

        for (int i = 0; i < YearsAmount; i++)
        {
            RectTransform Year = Instantiate(YearTick);
            Year.SetParent(ChartPanel);
            Year.localPosition = new Vector3(xScale * i, 0, 0);
            int tmpYear = InitYear + i;
            if (tmpYear >= 100)
                tmpYear -= 100;
            Year.transform.Find("Text").GetComponent<Text>().text = tmpYear.ToString("d2");
        }

        //Если в истории меньше двух значений, нечего рисовать
        if (GameManagerScript.GM.Player.History.Count < 2)
            return;

        PlayerScript AmPlayer = GameObject.Find("GameManager/AmerPlayer").GetComponent<PlayerScript>();
        PlayerScript SovPlayer = GameObject.Find("GameManager/SovPlayer").GetComponent<PlayerScript>();
        int[] AmHist = AmPlayer.History.GetRange(FirstInd, Mathf.Min(YearsAmount, AmPlayer.History.Count)).ToArray();
        int[] SovHist = SovPlayer.History.GetRange(FirstInd, Mathf.Min(YearsAmount, SovPlayer.History.Count)).ToArray();

        yScale = ChartPanel.rect.height / (Mathf.Max(Mathf.Max(AmHist), Mathf.Max(SovHist)) - Mathf.Min(Mathf.Min(AmHist), Mathf.Min(SovHist)));
        yOffset = Mathf.Min(Mathf.Min(AmHist), Mathf.Min(SovHist));

        //Вывод графиков
        Vector2 p1, p2;
        RectTransform Line;
        //Американский график
        for (int ind = 0; ind < AmHist.Length - 1; ind++)
        {
            //Для рисования линии будем поворачивать и растягивать простой прямоугольник (Image с пустым спрайтом)
            //Начало линии будет в точке текущего значения статистики (х - год, у - значени), а конец в точке следующего значения из массива.
            p1.x = ind * xScale;
            p1.y = (AmHist[ind] - yOffset) * yScale;

            Line = Instantiate(LinePrefab);
            Line.GetComponent<Image>().color = blueBrush;
            Line.SetParent(ChartPanel);
            Line.localPosition = p1;

            p2.x = (ind + 1) * xScale;
            p2.y = (AmHist[ind + 1] - yOffset) * yScale;

            p1 = p2 - p1;
            Line.localScale = new Vector3(p1.magnitude, 1, 1);
            Line.rotation = Quaternion.FromToRotation(Vector3.right, p1);
        }

        //Советский график
        for (int ind = 0; ind < SovHist.Length - 1; ind++)
        {
            p1.x = ind * xScale;
            p1.y = (SovHist[ind] - yOffset) * yScale;

            Line = Instantiate(LinePrefab);
            Line.GetComponent<Image>().color = redBrush;
            Line.SetParent(ChartPanel);
            Line.localPosition = p1;

            p2.x = (ind + 1) * xScale;
            p2.y = (SovHist[ind + 1] - yOffset) * yScale;

            p1 = p2 - p1;
            Line.localScale = new Vector3(p1.magnitude, 1, 1);
            Line.rotation = Quaternion.FromToRotation(Vector3.right, p1);
        }
    }
}
