using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

public class GondericiServer
{
    private string raspberryPiIpAddress;
    private int port;

    public GondericiServer(string ipAddress, int port)
    {
        this.raspberryPiIpAddress = ipAddress;
        this.port = port;
    }

    public void SendCommand(string command)
    {
        try
        {
            using (TcpClient client = new TcpClient(raspberryPiIpAddress, port))
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(command);
                stream.Write(data, 0, data.Length);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}");
        }
    }
    public void SendMechanical(string command)
    {
        try
        {
            using (TcpClient client = new TcpClient(raspberryPiIpAddress, port))
            {
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes(command);
                stream.Write(data, 0, data.Length);

                // Sunucudan yanıt bekleme
                byte[] responseData = new byte[256];
                int bytes = stream.Read(responseData, 0, responseData.Length);
                string response = Encoding.ASCII.GetString(responseData, 0, bytes);

                MessageBox.Show(response);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata: {ex.Message}");
        }
    }
}