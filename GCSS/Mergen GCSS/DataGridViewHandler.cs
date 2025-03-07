using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

public class DataGridViewHandler
{
    private DataGridView dataGridView;

    public DataGridViewHandler(DataGridView dataGridView)
    {
        this.dataGridView = dataGridView;
    }

    public void AddData(int[] intValues, string receivedString, string receivedString2, float[] floatValues, string additionalString, float receivedFloat, int receivedInt)
    {
        dataGridView.Invoke((MethodInvoker)delegate
        {
            dataGridView.Rows.Add(intValues[0], intValues[1], receivedString, receivedString2,
                floatValues[0], floatValues[1], floatValues[2], floatValues[3], floatValues[4],
                floatValues[5], floatValues[6], floatValues[7], floatValues[8], floatValues[9],
                floatValues[10], floatValues[11], floatValues[12], floatValues[13],
                additionalString, receivedFloat, receivedInt);
            // 1. Paket numarasına göre sıralama yap
            dataGridView.Sort(dataGridView.Columns[0], System.ComponentModel.ListSortDirection.Descending);

            // 2. En üst satırı seç ve vurgula
            dataGridView.Rows[0].Selected = false;
        });
    }
    public void SaveDataGridViewToCSV(string filePath)
    {
        StringBuilder csvContent = new StringBuilder();

        // Başlıkları ekleyin
        string[] columnNames = dataGridView.Columns
            .Cast<DataGridViewColumn>()
            .Select(column => column.HeaderText)
            .ToArray();
        csvContent.AppendLine(string.Join(";", columnNames));

        // Satırları ekleyin
        foreach (DataGridViewRow row in dataGridView.Rows)
        {
            if (!row.IsNewRow)
            {
                string[] cells = row.Cells
                    .Cast<DataGridViewCell>()
                    .Select(cell => cell.Value?.ToString() ?? string.Empty)
                    .ToArray();
                csvContent.AppendLine(string.Join(";", cells));
            }
        }

        // CSV dosyasına yaz
        File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
    }
}
