using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
using Mergen_GCSS;
using OpenTK;

public class TcpServer
{
   
    private mainForm formm;
    private TcpListener listener;
    private Thread listenerThread;
    private volatile bool isRunning;
    private DataGridViewHandler dataGridViewHandler;
    private ChartHandler chartHandler;
    private _3dSim simulation;
    public TcpServer(string ipAddress, int port, DataGridViewHandler dataGridViewHandler, ChartHandler chartHandler, _3dSim simulation,mainForm formm)
    {
        listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        this.dataGridViewHandler = dataGridViewHandler;
        this.chartHandler = chartHandler;
        this.simulation = simulation;
        this.formm = formm;
    }

    public void Start()
    {
        isRunning = true;  // Sunucuyu başlat
        listener.Start();
        listenerThread = new Thread(ListenForClients);
        listenerThread.Start();
    }

    private void ListenForClients()
    {
        try
        {
            while (isRunning)
            {
                if (listener.Pending())
                {
                    Socket clientSocket = listener.AcceptSocket();
                    Thread clientThread = new Thread(() => HandleClient(clientSocket));
                    clientThread.Start();
                }
                else
                {
                    Thread.Sleep(100);  // CPU kullanımını azaltmak için bekleme süresi
                }
            }
        }
        catch (SocketException)
        {
            // listener.Stop() çağrıldığında bu hata alınabilir
        }
    }

    private void HandleClient(Socket clientSocket)
    {
        using (clientSocket)
        {
            try
            {
                while (isRunning)
                {
                    byte[] buffer = new byte[8];
                    int bytesRead = clientSocket.Receive(buffer, 0, 8, SocketFlags.None);
                    if (bytesRead == 0) break;

                    int[] intValues = new int[2];
                    Buffer.BlockCopy(buffer, 0, intValues, 0, 8);

                    byte[] lengthBuffer = new byte[4];
                    clientSocket.Receive(lengthBuffer, 0, 4, SocketFlags.None);
                    int stringLength = BitConverter.ToInt32(lengthBuffer, 0);
                    byte[] stringBuffer = new byte[stringLength];
                    clientSocket.Receive(stringBuffer, 0, stringLength, SocketFlags.None);
                    string receivedString = Encoding.UTF8.GetString(stringBuffer);

                    byte[] lengthBuffer2 = new byte[4];
                    clientSocket.Receive(lengthBuffer2, 0, 4, SocketFlags.None);
                    int stringLength2 = BitConverter.ToInt32(lengthBuffer2, 0);
                    byte[] stringBuffer2 = new byte[stringLength2];
                    clientSocket.Receive(stringBuffer2, 0, stringLength2, SocketFlags.None);
                    string receivedString2 = Encoding.UTF8.GetString(stringBuffer2);

                    buffer = new byte[14 * 4];
                    clientSocket.Receive(buffer, 0, 14 * 4, SocketFlags.None);
                    float[] floatValues = new float[14];
                    Buffer.BlockCopy(buffer, 0, floatValues, 0, 14 * 4);

                    clientSocket.Receive(lengthBuffer, 0, 4, SocketFlags.None);
                    stringLength = BitConverter.ToInt32(lengthBuffer, 0);
                    stringBuffer = new byte[stringLength];
                    clientSocket.Receive(stringBuffer, 0, stringLength, SocketFlags.None);
                    string additionalString = Encoding.UTF8.GetString(stringBuffer);

                    buffer = new byte[4];
                    clientSocket.Receive(buffer, 0, 4, SocketFlags.None);
                    float receivedFloat = BitConverter.ToSingle(buffer, 0);

                    buffer = new byte[4];
                    clientSocket.Receive(buffer, 0, 4, SocketFlags.None);
                    int receivedInt = BitConverter.ToInt32(buffer, 0);

                    AddDataToGridAndCharts(intValues, receivedString, receivedString2, floatValues, additionalString, receivedFloat, receivedInt);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("İstemciyle iletişimde hata: " + e.Message);
            }
        }
    }

    private void AddDataToGridAndCharts(int[] intValues, string receivedString, string receivedString2, float[] floatValues, string additionalString, float receivedFloat, int receivedInt)
    {
        dataGridViewHandler.AddData(intValues, receivedString, receivedString2, floatValues, additionalString, receivedFloat, receivedInt);
        chartHandler.UpdateCharts(receivedString2,floatValues, receivedFloat);
        simulation.UpdateRotation(floatValues[11], floatValues[12], floatValues[13]);
        formm.Aras(receivedString);
        formm.Map(floatValues[8], floatValues[9]);   //floatValues[8], floatValues[9]
        
    }

    public void Stop()
    {
        isRunning = false;  // Sunucuyu durdur
        listener.Stop();    // Listener durdurulur

        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Join();  // Thread'in tamamlanmasını bekle
        }
    }
}
