using Accord.Video.FFMPEG;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video;
using AForge.Video.DirectShow;
using Accord.Video.VFW;
using System.Windows.Forms;

namespace Mergen_GCSS
{
    public  class Camera
    {

        VideoFileWriter writer;
        MJPEGStream VideoStream;
        string outputPath;
        string baseDirectory;
        private PictureBox pictureBox;
        public Camera(PictureBox pictureBox)
        {
            this.pictureBox = pictureBox;
            baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            VideoStream = new MJPEGStream("http://192.168.187.69:8000/stream.mjpg");
            VideoStream.NewFrame += VideoStream_NewFrame;

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            outputPath = Path.Combine(baseDirectory, "output_" + timestamp + ".mp4");

            writer = new VideoFileWriter();
        }

        private void VideoStream_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (writer.IsOpen)
            {
                writer.WriteVideoFrame((System.Drawing.Bitmap)eventArgs.Frame.Clone());
            }
            pictureBox.Image = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
        }

        public void StartRecording()
        {
            VideoStream.Start();

            try
            {
                // H.264 codec'ini kullanarak video dosyasını açın
                writer.Open(outputPath, 1280, 720,30,VideoCodec.MPEG4, 8000000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Video kaydetme hatası: " + ex.Message);
            }
        }
        public void StopRecording()
        {
            // Video akışını durdur
            VideoStream.Stop();

            // Video dosyasını kapat
            if (writer.IsOpen)
            {
                writer.Close();
            }
        }
    }
}
