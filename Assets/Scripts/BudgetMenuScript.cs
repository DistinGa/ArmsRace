using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BudgetMenuScript : MonoBehaviour
{
    public RectTransform ChartPanel;
    public RectTransform LinePrefab;
    public RectTransform YearTick;
    public Text Value0;
    public Text Value1;
    public Text Value2;
    public Text Budget;
    public Text Revenue;
    public Text Expenditure;
    public Text MilitaryOutlay;
    public Text DiplomacyOutlay;
    public Text SpaceOutlay;

    void OnEnable()
    {
        GameManagerScript.GM.SubscribeMonth(UpdateView);
        UpdateView();
    }

    public void OnDisable()
    {
        GameManagerScript.GM.UnsubscribeMonth(UpdateView);
    }

    void UpdateView()
    {
        PlayerScript pl = GameManagerScript.GM.Player;
        int milOutlay = pl.Outlays[OutlayField.air].Outlay + pl.Outlays[OutlayField.ground].Outlay + pl.Outlays[OutlayField.sea].Outlay + pl.Outlays[OutlayField.rocket].Outlay + pl.Outlays[OutlayField.military].Outlay;
        int dipOutlay = pl.Outlays[OutlayField.diplomat].Outlay + pl.Outlays[OutlayField.spy].Outlay;
        int spaceOutlay = 0;
        int expenditure;

        expenditure = pl.TotalYearSpendings();

        if (pl.History.Count > 0)
        {
            Revenue.text = pl.History2[pl.History2.Count - 1].ToString() + " (" + pl.History[pl.History.Count - 1].ToString() + "%)";
            Budget.text = pl.Budget.ToString("f0") + " (" + (pl.History2[pl.History2.Count - 1] - expenditure).ToString() +")";
        }
        else
            Revenue.text = "";

        Expenditure.text = expenditure.ToString();


        MilitaryOutlay.text = milOutlay.ToString();
        DiplomacyOutlay.text = dipOutlay.ToString();
        SpaceOutlay.text = spaceOutlay.ToString();

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

        int maxY = Mathf.Max(Mathf.Max(AmHist), Mathf.Max(SovHist));
        int minY = Mathf.Min(Mathf.Min(AmHist), Mathf.Min(SovHist));
        yScale = ChartPanel.rect.height / (maxY - minY);
        yOffset = minY;

        Value0.text = minY.ToString();
        Value1.text = ((maxY + minY) / 2f).ToString();
        Value2.text = maxY.ToString();

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
