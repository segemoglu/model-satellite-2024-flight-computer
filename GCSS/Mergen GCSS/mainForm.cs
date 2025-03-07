using Accord.Video.FFMPEG;
using GMap.NET;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Media;


namespace Mergen_GCSS
{
    public partial class mainForm : Form
    {
       
        public float x=0.0f, y=0.0f, z=0.0f;
        private GondericiServer gonderici;
        private _3dSim simulation;
        private Camera camera;
        private TcpServer server;
        private DataGridViewHandler dataGridViewHandler;
        private ChartHandler chartHandler;
        private SoundPlayer player;
        public float lat=0.0f, lng=0.0f;
        public mainForm()
        {
            InitializeComponent();
            gonderici = new GondericiServer("192.168.187.69", 65432);
            simulation = new _3dSim();
            player = new SoundPlayer();
            player.SoundLocation = @"C:\Users\HP\source\repos\Mergen-GCSS\Mergen GCSS\bmw.wav";
            button2.Enabled = false;
            button4.Enabled = false;
            dataGridViewHandler = new DataGridViewHandler(dataGridView1);
            chartHandler = new ChartHandler(this);
            camera = new Camera(pictureBox7);
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
           // gMapControl1.SetPositionByKeywords("Istanbul, Turkey");

            // Koordinatlarla manuel olarak pozisyon ayarlama
           //double latitude = 38.707901; // Enlem (Latitude)
          //  double longitude = 35.519833; // Boylam (Longitude)

            // Haritayı bu koordinatlara göre merkezleme
           // gMapControl1.Position = new PointLatLng(latitude, longitude);
            gMapControl1.MinZoom = 5;
            gMapControl1.MaxZoom = 20;
            gMapControl1.Zoom = 16;
            
            

        }

        public void Map(float latt, float lngg)
        {
            this.lat = latt;
            this.lng = lngg;

            // UI iş parçacığında mı olduğunuzu kontrol edin
            if (gMapControl1.InvokeRequired)
            {
                // Eğer değilsek, Invoke kullanarak işlemi UI iş parçacığında yap
                gMapControl1.Invoke(new MethodInvoker(delegate
                {
                    gMapControl1.Position = new PointLatLng(lat, lng);
                }));
            }
            else
            {
                // UI iş parçacığındaysanız doğrudan işlem yapabilirsiniz
                gMapControl1.Position = new PointLatLng(lat, lng);
            }
        }


        private void label6_Click(object sender, EventArgs e)
        {
            DialogResult firstResponse = MessageBox.Show("Uygulamadan çıkmak istediğinize emin misiniz?", "Çıkış Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (firstResponse == DialogResult.Yes)
            {
                // İkinci onay kutusu
                DialogResult secondResponse = MessageBox.Show("Emin misiniz? Cidden çıkmak istiyor musunuz? Videoyu vs kontrol ettiniz mi?", "Çıkış Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (secondResponse == DialogResult.Yes)
                {
                    // Uygulamadan çıkış
                    Application.Exit();
                }
            }
        }

        private void label6_MouseEnter(object sender, EventArgs e)
        {
            label6.ForeColor = Color.DarkRed;
        }

        private void label6_MouseLeave(object sender, EventArgs e)
        {
            label6.ForeColor = Color.Red;

        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            
            pictureBox1.BackColor = Color.Chartreuse;
            pictureBox2.BackColor = Color.Chartreuse;
            pictureBox3.BackColor = Color.Chartreuse;
            pictureBox4.BackColor = Color.Chartreuse;
            pictureBox5.BackColor = Color.Chartreuse;

            label7.ForeColor = Color.Chartreuse;
            label8.ForeColor = Color.Chartreuse;
            label9.ForeColor = Color.Chartreuse;
            label10.ForeColor = Color.Chartreuse;
            label11.ForeColor = Color.Chartreuse;

            timer1.Start();
            timer2.Interval = 10000; // 1 dakika (60000 milisaniye)
            timer2.Tick += timer2_Tick;
            timer2.Start();
        }

        private  void button3_Click(object sender, EventArgs e)
        {
            camera.StartRecording();
            button3.Enabled = false;
            button4.Enabled = true;
        }

        private  void button4_Click(object sender, EventArgs e)
        {
            camera.StopRecording();
            button4.Enabled = false;
            button3.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server = new TcpServer("192.168.187.26", 12345, dataGridViewHandler, chartHandler,simulation,this);
            server.Start();
            button1.Enabled = false; // Başlat butonunu devre dışı bırak
            button2.Enabled = true;   // Durdur butonunu etkinleştir
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // server.Stop();
            if (server != null)
            {
                server.Stop(); // server olarak tanımladığınız TcpServer'ı durdurun
                server = null;

            }
            button1.Enabled = true;  // Başlat butonunu etkinleştir
            button2.Enabled = false;
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.Gray);
            
        }
       

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 4 / 3, 1, 1000);
            Matrix4 view = Matrix4.LookAt(15, 0, 0, 0, 0, 0, 0, 1, 0);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref view);




            

            GL.Rotate(x, -1, 0, 0);
             GL.Rotate(z, 0, -1, 0);
             GL.Rotate(y, 0, 0, 2);  
            
            simulation.DrawCylinder(2.0f, 8.0f, 32);
            glControl1.SwapBuffers();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            x = simulation.x;
            y = simulation.y;
            z = simulation.z;

            simulation.UpdateRotation(x, y, z);
          
            glControl1.Invalidate(); // Yeniden çizim
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // Kullanıcının Masaüstü klasörünü al
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Dosya adını ve yolu oluştur
            string filePath = Path.Combine(desktopPath, "data.csv");
            dataGridViewHandler.SaveDataGridViewToCSV(filePath);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            gonderici.SendCommand("180");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            gonderici.SendCommand("80");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string command = textBox1.Text;
            if (isValidCommand(command))
            {
                gonderici.SendCommand(command);
            }
            else
            {
                MessageBox.Show("Hatalı komut. Lütfen geçerli bir komut girin.");
            }
        }
        private bool isValidCommand(string command)
        {
            // Komut doğrulama mantığını buraya ekleyebilirsiniz (Python kodundaki gibi)
            if (command.Length != 4) return false;
            if (!char.IsDigit(command[0]) || !char.IsLetter(command[1]) ||
                !char.IsDigit(command[2]) || !char.IsLetter(command[3])) return false;
            if (!"RGBrgb".Contains(command[1]) || !"RGBrgb".Contains(command[3])) return false;
            if (int.Parse(command[0].ToString()) < 1 || int.Parse(command[0].ToString()) > 9 ||
                int.Parse(command[2].ToString()) < 1 || int.Parse(command[2].ToString()) > 9) return false;

            return true;
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Alt+F4 veya kullanıcı tarafından kapatma işlemi engellenir
                e.Cancel = true;
                MessageBox.Show("Alt+F4 ile uygulamayı kapatamazsınız!", "Kapatma Engellendi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            // Kullanıcının Masaüstü klasörünü al
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Dosya adını ve yolu oluştur
            string filePath = Path.Combine(desktopPath, "data.csv");
            dataGridViewHandler.SaveDataGridViewToCSV(filePath); // Uygulamadan çıkarken verileri kaydet
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                if (i % 2 == 0) // Çift indeksli satırları mavi yap
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(38,29,58);
                }
                else // Tek indeksli satırları turuncu yap
                {
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(76,58,116);
                }
            }
        }

        public void Aras(string receivedString)
        {
            // Eğer stringin uzunluğu 5 değilse, bir uyarı döndür
            if (receivedString.Length != 5)
            {
                MessageBox.Show("String 5 karakter olmalı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Stringi karakterlerine ayır
            char[] characters = receivedString.ToCharArray();

            // En az bir '1' olup olmadığını kontrol etmek için bir flag değişkeni tanımla
            bool hasOne = false;

            // Her karakter için kontrol ve ilgili Label/PictureBox güncellemeleri
            for (int i = 0; i < characters.Length; i++)
            {
                // PictureBox isimleri 1, 2, 3, 4, 5 ile bitiyor
                PictureBox pictureBox = Controls.Find($"pictureBox{i + 1}", true).FirstOrDefault() as PictureBox;

                // Label isimleri 7, 8, 9, 10, 11 ile bitiyor
                Label label = Controls.Find($"label{i + 7}", true).FirstOrDefault() as Label;

                // Eğer Label ve PictureBox bulunursa, güncellemeleri yap
                if (label != null && pictureBox != null)
                {
                    kontrolYap(characters[i], label, pictureBox);

                    // Eğer karakter '1' ise, hasOne değişkenini true yap
                    if (characters[i] == '1')
                    {
                        hasOne = true;
                    }
                }
            }

            // Eğer en az bir karakter '1' ise, Beep sesi ver
            if (hasOne)
            {
                player.Play();
            }
        }

        private void kontrolYap(char character, Label label, PictureBox pictureBox)
        {
            if (character == '1')
            {
                label.ForeColor = Color.Red;
                pictureBox.BackColor = Color.Red;
            }
            else
            {
                label.ForeColor = Color.Chartreuse;
                pictureBox.BackColor = Color.Chartreuse;
            }

        }
    }
}
