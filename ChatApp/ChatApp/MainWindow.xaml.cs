using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket sck;
        private IPEndPoint epLocal;
        private EndPoint epRemote;
        //private IAsyncResult aResult;

        public MainWindow()
        {
            InitializeComponent();
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        private string GetLocalIP()
        {
            IPHostEntry host;

            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            textLocalIp.Text = GetLocalIP();

            textFriendsIp.Text = GetLocalIP();

            try
            {
                // binding socket
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);
                // connect to remote IP and port
                epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
                sck.Connect(epRemote);
                // startsto listen to an specific port
                var buffer = new byte[1500];

                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote,
                    new AsyncCallback(MessageCallBack), buffer);
                // release button to send message
                buttonSend.IsEnabled = true;
                buttonStart.Content = "Connected";
                buttonStart.IsEnabled = false;
                textMessage.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // converts from string to byte[]
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(textMessage.Text);

                // sending the message
                sck.Send(msg);

                // add to listbox
                listMessage.Items.Add("You: " + textMessage.Text);
                // clear txtMessage
                textMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);
                // check if theres actually information if (size > 0) { // used to help us on getting the data
                byte[] receivedData = new byte[1464];
                // getting the message data
                receivedData = (byte[]) aResult.AsyncState;
                // convets message data byte array to string
                ASCIIEncoding eEncoding = new ASCIIEncoding();
                string receivedMessage = eEncoding.GetString(receivedData);
                // adding Message to the listbox
                listMessage.Items.Add("Friend: " + receivedMessage);
                // starts to listen the socket again
                var buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
    }
}
