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
using ICommand;
using System.Runtime.Serialization.Formatters;
using System.Collections;

namespace NetRemotingClient
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 注册通道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_Load(object sender, EventArgs e)
        {
            try
            {
                //设置反序列化级别  
                BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
                serverProvider.TypeFilterLevel = TypeFilterLevel.Full;//支持所有类型的反序列化，级别很高  
                //信道端口  
                IDictionary idic = new Dictionary<string, string>();
                idic["name"] = "clientHttp";
                idic["port"] = "0";
                HttpChannel channel = new HttpChannel(idic, clientProvider, serverProvider);
                ChannelServices.RegisterChannel(channel, false);
                _remotingObject = (IRemotingObject)Activator.GetObject(typeof(IRemotingObject), "http://localhost:8022/SumMessage");
                //_remotingObject.ServerToClient += (info, toName) => { rtxMessage.AppendText(info + "\r\n"); };
                SwapObject swap = new SwapObject();
                _remotingObject.ServerToClient += swap.ToClient;
                swap.SwapServerToClient += (info, toName) =>
                {
                    rtxMessage.Invoke((MethodInvoker)(() =>
                    {
                        if (toName == txtLogin.Text || toName == "")
                        {
                            rtxMessage.AppendText(info + "\r\n");
                        }
                    }));
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtLogin.Text == "")
                    throw new Exception("用户名不得为空");
                _remotingObject.ToLogin(txtLogin.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _remotingObject.ToExit(txtLogin.Text);
            }
            catch
            { }
        }
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            //rtxMessage.AppendText(_remotingObject.SUM(2, 4) + "\r\n");
            _remotingObject.ToServer(txtSend.Text, txtName.Text);
        }


        private IRemotingObject _remotingObject;

    }
}
