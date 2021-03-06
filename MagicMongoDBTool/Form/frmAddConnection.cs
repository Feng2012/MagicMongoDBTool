﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using MagicMongoDBTool.Module;
using MongoDB.Driver;

namespace MagicMongoDBTool
{
    public partial class frmAddConnection : Form
    {
        //http://www.mongodb.org/display/DOCS/Connections

        /// <summary>
        ///     连接配置
        /// </summary>
        public ConfigHelper.MongoConnectionConfig ModifyConn = new ConfigHelper.MongoConnectionConfig();

        /// <summary>
        ///     修改模式中，原来的连接
        /// </summary>
        public String OldConnectionName = String.Empty;

        /// <summary>
        ///     初始化（新建）
        /// </summary>
        public frmAddConnection()
        {
            InitializeComponent();
            OnLoad();
        }

        /// <summary>
        ///     初始化（修改）
        /// </summary>
        /// <param name="ConnectionName"></param>
        public frmAddConnection(String ConnectionName)
        {
            InitializeComponent();
            OldConnectionName = ConnectionName;
            //Modify Mode
            ModifyConn = SystemManager.ConfigHelperInstance.ConnectionList[ConnectionName];
            OnLoad();

            txtConnectionName.Text = ModifyConn.ConnectionName;

            txtHost.Text = ModifyConn.Host;
            numPort.Text = ModifyConn.Port.ToString(CultureInfo.InvariantCulture);
            txtUsername.Text = ModifyConn.UserName;
            txtPassword.Text = ModifyConn.Password;
            txtDataBaseName.Text = ModifyConn.DataBaseName;
            chkFsync.Checked = ModifyConn.fsync;
            chkJournal.Checked = ModifyConn.journal;

            NumSocketTimeOut.Value = (decimal) ModifyConn.socketTimeoutMS;
            NumConnectTimeOut.Value = (decimal) ModifyConn.connectTimeoutMS;

            txtReplsetName.Text = ModifyConn.ReplSetName;
            txtConnectionString.Text = ModifyConn.ConnectionString;

            foreach (string item in ModifyConn.ReplsetList)
            {
                lstHost.Items.Add(item);
            }

            cmdAdd.Text = SystemManager.IsUseDefaultLanguage
                ? "Modify"
                : SystemManager.MStringResource.GetText(StringResource.TextType.Common_Modify);
        }

        /// <summary>
        ///     加载
        /// </summary>
        private void OnLoad()
        {
            cmdCancel.Click += (x, y) => Close();
            numPort.GotFocus += (x, y) => numPort.Select(0, 5);
            NumReplPort.GotFocus += (x, y) => NumReplPort.Select(0, 5);
            NumSocketTimeOut.GotFocus += (x, y) => NumSocketTimeOut.Select(0, 5);
            NumConnectTimeOut.GotFocus += (x, y) => NumConnectTimeOut.Select(0, 5);

            if (SystemManager.IsUseDefaultLanguage) return;
            Text = SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_Title);
            lblConnectionName.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_ConnectionName);
            lblHost.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Host);
            lblPort.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Port);
            lblUsername.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Username);
            lblPassword.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Password);
            lblDataBaseName.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_DBName);
            lblConnectionString.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_ConnectionString);
            lblAttentionPassword.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_Password_Description);


            lblsocketTimeout.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_SocketTimeOut);
            lblConnectTimeout.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_ConnectionTimeOut);


            lblMainReplsetName.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_MainReplsetName);
            lblReplHost.Text = lblHost.Text;
            lblReplPort.Text = lblPort.Text;
            cmdAddHost.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_Region_AddHost);
            cmdRemoveHost.Text =
                SystemManager.MStringResource.GetText(StringResource.TextType.AddConnection_Region_RemoveHost);

            cmdAdd.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Add);
            cmdCancel.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Cancel);
            cmdTest.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Test);
        }

        /// <summary>
        ///     新建或者修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdAdd_Click(object sender, EventArgs e)
        {
            CreateConnection();
            String NewCollectionName = txtConnectionName.Text;
            if (OldConnectionName != String.Empty)
            {
                //如果有旧名称，说明是修改模式
                if (OldConnectionName != NewCollectionName)
                {
                    //修改了名称,检查一下新的名字是否存在
                    if (SystemManager.ConfigHelperInstance.ConnectionList.ContainsKey(NewCollectionName))
                    {
                        //存在则警告
                        MyMessageBox.ShowMessage("Connection", "Connection Name Already Exist!");
                        return;
                    }
                    //不存在则删除旧的记录
                    SystemManager.ConfigHelperInstance.ConnectionList.Remove(OldConnectionName);
                }
            }
            //保存配置
            if (SystemManager.ConfigHelperInstance.ConnectionList.ContainsKey(NewCollectionName))
            {
                SystemManager.ConfigHelperInstance.ConnectionList[NewCollectionName] = ModifyConn;
            }
            else
            {
                SystemManager.ConfigHelperInstance.ConnectionList.Add(NewCollectionName, ModifyConn);
            }
            Close();
        }

        /// <summary>
        ///     测试连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdTest_Click(object sender, EventArgs e)
        {
            CreateConnection();
            try
            {
                MongoServer srv = MongoDbHelper.CreateMongoServer(ref ModifyConn);
                srv.Connect();
                srv.Disconnect();
                MyMessageBox.ShowMessage("Connect Test", "Connected OK.");
            }
            catch (MongoAuthenticationException ex)
            {
                //需要验证的数据服务器，没有Admin权限无法获得数据库列表
                if (!SystemManager.IsUseDefaultLanguage)
                {
                    MyMessageBox.ShowMessage(
                        SystemManager.MStringResource.GetText(StringResource.TextType.Exception_AuthenticationException),
                        SystemManager.MStringResource.GetText(
                            StringResource.TextType.Exception_AuthenticationException_Note), ex.ToString(), true);
                }
                else
                {
                    MyMessageBox.ShowMessage("MongoAuthenticationException:", "Please check UserName and Password",
                        ex.ToString(), true);
                }
            }
            catch (Exception ex)
            {
                //暂时不处理任何异常，简单跳过
                //无法连接的理由：
                //1.服务器没有启动
                //2.认证模式不正确
                if (!SystemManager.IsUseDefaultLanguage)
                {
                    MyMessageBox.ShowMessage(
                        SystemManager.MStringResource.GetText(StringResource.TextType.Exception_NotConnected),
                        SystemManager.MStringResource.GetText(StringResource.TextType.Exception_NotConnected_Note),
                        ex.ToString(), true);
                }
                else
                {
                    MyMessageBox.ShowMessage("Exception", "Mongo Server may not Startup or Auth Mode is not correct",
                        ex.ToString(), true);
                }
            }
        }

        /// <summary>
        ///     新建连接
        /// </summary>
        private void CreateConnection()
        {
            ModifyConn.ConnectionName = txtConnectionName.Text;
            //感谢 呆呆 的Bug 报告，不论txtConnectionString.Text是否存在都进行赋值，防止删除字符后，值还是保留的BUG
            ModifyConn.ConnectionString = txtConnectionString.Text;
            if (txtConnectionString.Text != String.Empty)
            {
                String strException = MongoDbHelper.FillConfigWithConnectionString(ref ModifyConn);
                if (strException != String.Empty)
                {
                    MyMessageBox.ShowMessage("Url Exception", "Url Formation，please check it", strException);
                }
            }
            else
            {
                ModifyConn.Host = txtHost.Text;
                ModifyConn.Port = numPort.Text != String.Empty ? Convert.ToInt32(numPort.Text) : 0;
                ModifyConn.UserName = txtUsername.Text;
                ModifyConn.Password = txtPassword.Text;
                ModifyConn.DataBaseName = txtDataBaseName.Text;

                //仅有用户名或密码
                if (txtUsername.Text != String.Empty && txtPassword.Text == String.Empty)
                {
                    MessageBox.Show("Please Input Password");
                    return;
                }
                if (txtUsername.Text == String.Empty && txtPassword.Text != String.Empty)
                {
                    MessageBox.Show("Please Input UserName");
                    return;
                }
                //数据库名称存在，则必须输入用户名和密码
                if (txtDataBaseName.Text != String.Empty)
                {
                    //用户名或者密码为空
                    if (txtUsername.Text == String.Empty || txtPassword.Text == String.Empty)
                    {
                        MessageBox.Show("Please Input UserName or Password");
                        return;
                    }
                }

                ModifyConn.IsUseDefaultSetting = chkUseDefault.Checked;
                if (ModifyConn.IsUseDefaultSetting) {
                    ModifyConn.wtimeoutMS = SystemManager.ConfigHelperInstance.wtimeoutMS;
                    ModifyConn.WaitQueueSize = SystemManager.ConfigHelperInstance.WaitQueueSize;
                    ModifyConn.WriteConcern = SystemManager.ConfigHelperInstance.WriteConcern;
                    ModifyConn.ReadPreference = SystemManager.ConfigHelperInstance.ReadPreference;
                } else {
                    ModifyConn.wtimeoutMS = (double)NumWTimeoutMS.Value;
                    ModifyConn.WaitQueueSize = (int)NumWaitQueueSize.Value;
                    ModifyConn.WriteConcern = cmbWriteConcern.Text;
                    ModifyConn.ReadPreference = cmbReadPreference.Text;
                }

                ModifyConn.socketTimeoutMS = (double) NumSocketTimeOut.Value;
                ModifyConn.connectTimeoutMS = (double) NumConnectTimeOut.Value;
                ModifyConn.journal = chkJournal.Checked;
                ModifyConn.fsync = chkFsync.Checked;
                ModifyConn.ReplSetName = txtReplsetName.Text;
                ModifyConn.ReplsetList = new List<string>();
                foreach (String item in lstHost.Items)
                {
                    ModifyConn.ReplsetList.Add(item);
                }
            }
        }

        /// <summary>
        ///     添加HostList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdAddHost_Click(object sender, EventArgs e)
        {
            String strHost = String.Empty;
            if (string.IsNullOrEmpty(strHost)) return;
            strHost = txtReplHost.Text;
            if (NumReplPort.Value == 0) return;
            strHost += ":" + NumReplPort.Value;
            lstHost.Items.Add(strHost);
        }

        /// <summary>
        ///     移除HostList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdRemoveHost_Click(object sender, EventArgs e)
        {
            lstHost.Items.Remove(lstHost.SelectedItem);
        }

        /// <summary>
        ///     读策略的官方文档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkReadPreference_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://docs.mongodb.org/manual/reference/connection-string/#read-preference-options");
        }

        /// <summary>
        ///     写策略的官方文档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkWriteConcern_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://docs.mongodb.org/manual/reference/connection-string/#write-concern-options");
        }

    }
}