using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;


namespace rocketEngineTestBench
{
    public partial class Form1 : Form
    {

        bool handshake = false;
        string readLine;
        int thrust;
        bool testActive = false;
        double time;
        double xAxisIncrement;
        int startingSensitivity;

        public Form1()
        {
            InitializeComponent();

            timer1.Start();
            bitrateComboBox.Text = "9600";
            this.Select();
            
        }
        
        public void getAvailablePorts() //Get available ports
        {
            try
            {
               
                    String[] availablePorts = SerialPort.GetPortNames();
                    portComboBox.Items.Clear();
                    portComboBox.Items.AddRange(availablePorts);
            }
            catch
            {
                MessageBox.Show("Can`t find available ports");
            }

        }

        public void connectToArduino() //Connect to arduino
        {

            if (serialPort1.IsOpen)
            {


                //backgroundWorker2.CancelAsync();
                serialPort1.Close();

                if (!serialPort1.IsOpen)
                {
                    connectButton.BackColor = Color.OrangeRed;
                    connectButton.Text = "Connect";
                    handshake = false;
                    handshakeLabel.Visible = false;
                }
            }
            else
            {
                //if com port not selected display error
                if (portComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Port not selected");
                }
                //if data rate not selected display error
                else if (bitrateComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Data rate not selected");
                }
                //if port selected try to connect to it
                else
                {

                    if (!serialPort1.IsOpen && portComboBox.SelectedText != null && bitrateComboBox.SelectedText != null)
                    {
                        //Set comport and bitrate
                        serialPort1.PortName = portComboBox.Text;
                        serialPort1.BaudRate = Convert.ToInt16(bitrateComboBox.Text);

                        serialPort1.Open();

                        if (serialPort1.IsOpen)
                        {
                            handshakeLabel.Visible = true;
                            handshakeLabel.ForeColor = Color.Black;
                            handshakeLabel.Text = "Waiting for handshake...";
                            serialPort1.Write("?");
                            serialPort1.DiscardOutBuffer();

                            Thread.Sleep(1000);
                            if (handshake == true)
                            {
                                handshakeLabel.ForeColor = Color.LimeGreen;
                                handshakeLabel.Text = "Handshake OK";
                                connectButton.BackColor = Color.LimeGreen;
                                connectButton.Text = "Connected";
                            }
                            else
                            {
                                handshakeLabel.ForeColor = Color.Red;
                                handshakeLabel.Text = "Handshake failed";

                            }

                        }

                    }
                }

            }

        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort1.ReadByte() == 'Y')
            {
                handshake = true;
                serialPort1.DiscardInBuffer();
            }
            readLine = serialPort1.ReadLine();
            serialPort1.DiscardInBuffer();
            if(serialPort1.ReadByte() == 'S')
            {
                startButton.BackColor = Color.LimeGreen;
                startButton.Text = "ENGINE IGNITED";
            }
            if (Int32.TryParse(readLine, out thrust))
            {
                if(thrust > startingSensitivity)
                {
                    testActive = true;
                    startButton.Text = "ENGINE STARTED";
                    time += xAxisIncrement;
                    chart1.Series["thrust"].Points.AddXY(time, thrust);
                    richTextBox1.AppendText(thrust.ToString() + "\n");

                }
                else if(testActive ==  true)
                {
                    testOver();
                }
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            startingSensitivity = trackBar1.Value;
            label3.Text = startingSensitivity.ToString();

        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            xAxisIncrement = Convert.ToDouble(trackBar2.Value) / 100;
            label4.Text = xAxisIncrement.ToString();
            
        }

        
        public void testOver()
        {
            testActive = false;
            time = 0;
            startButton.Text = "START ENGINE TEST";
            startButton.BackColor = Color.Red;
            saveFileDialog1.ShowDialog();

            
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen && handshake == true)
            {
                testActive = true;
                serialPort1.Write("Start");
            }

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (saveFileDialog1.CheckPathExists == true && saveFileDialog1.ValidateNames == true)
            {
                chart1.SaveImage(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(ModifierKeys == Keys.Shift && ModifierKeys == Keys.Enter)
            {

                startButton.PerformClick();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                getAvailablePorts();
            }
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            chart1.Width = this.Width - 200;
            chart1.Height = this.Height - 230;
        }
    }
}
