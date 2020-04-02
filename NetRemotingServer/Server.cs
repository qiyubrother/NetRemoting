using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using NetRemoting;
using System.Collections;
using System.Runtime.Serialization.Formatters;
using ICommand;

namespace NetRemotingServer
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            Initialize();
        }
        /// <summary>
        /// 注册通道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_Load(object sender, EventArgs e)
        {

            ChannelServices.RegisterChannel(_channel, false);
            //RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemotingObject), "SumMessage", WellKnownObjectMode.Singleton); //a方案
            /*将给定的 System.MarshalByRefObject 转换为具有指定 URI 的 System.Runtime.Remoting.ObjRef 类的实例。
              ObjRef ：存储生成代理以与远程对象通信所需要的所有信息。*/
            ObjRef objRef = RemotingServices.Marshal(_remotingObject, "SumMessage");//b方案
            _remotingObject.ClientToServer += (info, toName) =>
            {
                rxtInfo.Invoke((MethodInvoker)(() => { rxtInfo.AppendText(info.ToString() + "\r\n"); }));
                SendToClient(info, toName);
            };
            _remotingObject.Login += (name) =>
            {
                rxtInfo.Invoke((MethodInvoker)(() => { rxtInfo.AppendText(name + " 登录" + "\r\n"); }));
            };
            _remotingObject.Exit += (name) =>
            {
                rxtInfo.Invoke((MethodInvoker)(() => { rxtInfo.AppendText(name + " 退出" + "\r\n"); }));
            };
        }
        /// <summary>
        /// 注销通道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_channel != null)
            {
                _channel.StopListening(null);
                ChannelServices.UnregisterChannel(_channel);
            }
        }
        /// <summary>
        /// 广播消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendToClient(txtSend.Text, txtName.Text);
        }
        /// <summary>
        /// 发送消息到客户端
        /// </summary>
        /// <param name="info"></param>
        /// <param name="toName"></param>
        private void SendToClient(object info, string toName)
        {
            foreach (var v in _remotingObject.GetServerEventList())
            {
                try
                {
                    ReceiveHandler receive = (ReceiveHandler)v;
                    receive.BeginInvoke(info, toName, null, null);
                }
                catch
                { }
            }
            //_remotingObject.ToClient(txtSend.Text, txtName.Text);
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            //设置反序列化级别  
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;//支持所有类型的反序列化，级别很高  
            IDictionary idic = new Dictionary<string, string>();
            idic["name"] = "serverHttp";
            idic["port"] = "8022";
            _channel = new HttpChannel(idic, clientProvider, serverProvider);
            _remotingObject = new RemotingObject();
        }

        HttpChannel _channel;
        private RemotingObject _remotingObject;


    }
}
