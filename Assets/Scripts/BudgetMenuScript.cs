using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BudgetMenuScript : MonoBehaviour
{
    public RectTransform ChartPanel1;
    public RectTransform ChartPanel2;
    public RectTransform LinePrefab;
    public RectTransform YearTick;
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
        GameManagerScript GM = GameManagerScript.GM;
        PlayerScript pl = GM.Player;
        int milOutlay = pl.Outlays[OutlayField.air].Outlay + pl.Outlays[OutlayField.ground].Outlay + pl.Outlays[OutlayField.sea].Outlay + pl.Outlays[OutlayField.rocket].Outlay + pl.Outlays[OutlayField.military].Outlay;
        int dipOutlay = pl.Outlays[OutlayField.diplomat].Outlay + pl.Outlays[OutlayField.spy].Outlay;
        int spaceOutlay = pl.Outlays[OutlayField.spaceGround].Outlay + pl.Outlays[OutlayField.spaceLaunches].Outlay;
        int revenue, expenditure;

        expenditure = pl.TotalYearSpendings();

        if (pl.History.Count > 1)
        {
            revenue = pl.LastRevenue;
            string tmpstr;
            if (pl.LastAddBudgetGrow != 0)
                tmpstr = " (" + (pl.History[pl.History.Count - 1] - pl.LastAddBudgetGrow).ToString() + "% +" + pl.LastAddBudgetGrow.ToString() + "%)";
            else
                tmpstr = " (" + pl.History[pl.History.Count - 1].ToString() + "%)";

            Revenue.text = revenue.ToString() + tmpstr;
            Budget.text = pl.Budget.ToString("f0") + " (" + (revenue - expenditure).ToString() + ")";
        }
        else
        {
            Revenue.text = "";
            Budget.text = pl.Budget.ToString("f0");
        }

        Expenditure.text = expenditure.ToString();


        MilitaryOutlay.text = milOutlay.ToString();
        DiplomacyOutlay.text = dipOutlay.ToString();
        SpaceOutlay.text = spaceOutlay.ToString();

        DrawChart(1);
        DrawChart(2);

        //Отображение процента доп. прироста бюджета (покупаемого за political points)
        if (GM.Player.addBudgetGrowPercent > 0)
            ChartPanel1.parent.Find("Text").GetComponent<Text>().text = "GNP GROW % (+" + GM.Player.addBudgetGrowPercent.ToString() + "%)";
        else
            ChartPanel1.parent.Find("Text").GetComponent<Text>().text = "GNP GROW %";

        //Активность кнопки доп. прироста бюджета
        ChartPanel1.parent.Find("btAddBudgetGrow").GetComponent<Button>().interactable = (GM.Player.PoliticalPoints > 0);
    }

    //variant: 1 - проценты роста; 2 - величина бюджета
    void DrawChart(int variant)
    {
        int YearsAmount = 10;   //количество лет на графике
        Color redBrush = new Color(1, 0, 0);
        Color blueBrush = new Color(0, 0, 1);

        float xScale, yScale, yOffset;

        int[] AmHist = null;
        int[] SovHist = null;
        RectTransform chartPanel = null;
        GameManagerScript GM = GameManagerScript.GM;

        //Определяем начальный элемент истории
        int FirstInd = GM.Player.History.Count - YearsAmount;
        if (FirstInd < 0)
            FirstInd = 0;

        PlayerScript AmPlayer = GM.GetPlayerByAuthority(Authority.Amer);
        PlayerScript SovPlayer = GM.GetPlayerByAuthority(Authority.Soviet);
        switch (variant)
        {
            case 1:
                AmHist = AmPlayer.History.GetRange(FirstInd, Mathf.Min(YearsAmount, AmPlayer.History.Count)).ToArray();
                SovHist = SovPlayer.History.GetRange(FirstInd, Mathf.Min(YearsAmount, SovPlayer.History.Count)).ToArray();
                chartPanel = ChartPanel1;
                break;
            case 2:
                AmHist = AmPlayer.History2.GetRange(FirstInd, Mathf.Min(YearsAmount, AmPlayer.History2.Count)).ToArray();
                SovHist = SovPlayer.History2.GetRange(FirstInd, Mathf.Min(YearsAmount, SovPlayer.History2.Count)).ToArray();
                chartPanel = ChartPanel2;
                break;
        }

        //Сначала удалим предыдущие графики
        while (chartPanel.childCount > 0)
            DestroyImmediate(chartPanel.GetChild(0).gameObject);

        xScale = chartPanel.rect.width / (YearsAmount - 1);

        //Рисуем годы на графике
        int InitYear = 50 + FirstInd;   //51-й год - первый, где есть статистика

        for (int i = 0; i < YearsAmount; i++)
        {
            RectTransform Year = Instantiate(YearTick);
            Year.SetParent(chartPanel);
            Year.localPosition = new Vector3(xScale * i, -3, 0);
            int tmpYear = InitYear + i;
            if (tmpYear >= 100)
                tmpYear -= 100;
            Year.transform.Find("Text").GetComponent<Text>().text = tmpYear.ToString("d2");
        }

        //Если в истории меньше двух значений, нечего рисовать
        if (GM.Player.History.Count < 2)
            return;

        int maxY = Mathf.Max(Mathf.Max(AmHist), Mathf.Max(SovHist));
        int minY = Mathf.Min(Mathf.Min(AmHist), Mathf.Min(SovHist));
        yScale = chartPanel.rect.height / (maxY - minY);
        yOffset = minY;

        //значения горизонтальных линий
        chartPanel.parent.Find("Value0").GetComponent<Text>().text = minY.ToString();
        chartPanel.parent.Find("Value1").GetComponent<Text>().text = ((maxY + minY) / 2f).ToString();
        chartPanel.parent.Find("Value2").GetComponent<Text>().text = maxY.ToString();

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
            Line.SetParent(chartPanel);
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
            Line.SetParent(chartPanel);
            Line.localPosition = p1;

            p2.x = (ind + 1) * xScale;
            p2.y = (SovHist[ind + 1] - yOffset) * yScale;

            p1 = p2 - p1;
            Line.localScale = new Vector3(p1.magnitude, 1, 1);
            Line.rotation = Quaternion.FromToRotation(Vector3.right, p1);
        }
    }

    public void AddBudgetGrow()
    {
        GameManagerScript.GM.Player.AddBudgetGrow();
        UpdateView();
    }

}
