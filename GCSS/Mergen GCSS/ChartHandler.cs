using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;


public class ChartHandler
{
    private Form form;

    public ChartHandler(Form form)
    {
        this.form = form;
    }

    public void UpdateCharts(string receivedString2,float[] floatValues, float receivedFloat)
    {
        var parts = receivedString2.Split(',');
        if (parts.Length < 2) return;
        string timePart = parts[1];

        // Zaman kısmındaki '/' işaretlerini ':' ile değiştir
        string formattedTime = timePart.Replace('/', ':');
        form.Invoke((MethodInvoker)delegate
        {
            for (int i = 0; i <= 7; i++)
            {
                Chart chart = form.Controls.Find($"chart{i + 1}", true).FirstOrDefault() as Chart;
                if (chart != null)
                {
                    chart.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
                    chart.Series[0].Points.AddXY(formattedTime, floatValues[i]);
                }
            }

            Chart chart9 = form.Controls.Find("chart9", true).FirstOrDefault() as Chart;
            if (chart9 != null)
            {
                chart9.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
                chart9.Series[0].Points.AddXY(formattedTime, receivedFloat);
            }
        });
    }
}
