﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MagicMongoDBTool.Module;
using MongoDB.Driver.Builders;

namespace MagicMongoDBTool
{
    public partial class frmCreateCollection : Form
    {
        public Boolean Result = false;
        public String strSvrPathWithTag;
        public TreeNode treeNode;

        public frmCreateCollection()
        {
            InitializeComponent();
        }

        private void frmCreateCollection_Load(object sender, EventArgs e)
        {
            if (!SystemManager.IsUseDefaultLanguage)
            {
                Text = SystemManager.MStringResource.GetText(StringResource.TextType.Create_New_Collection);
                lblCollectionName.Text =
                    SystemManager.MStringResource.GetText(StringResource.TextType.Collection_Status_CollectionName);
                chkAdvance.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Advance_Option);
                cmdOK.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_OK);
                cmdCancel.Text = SystemManager.MStringResource.GetText(StringResource.TextType.Common_Cancel);
                chkIsCapped.Text =
                    SystemManager.MStringResource.GetText(StringResource.TextType.Collection_Status_IsCapped);
                lblMaxDocument.Text =
                    SystemManager.MStringResource.GetText(StringResource.TextType.Collection_Status_MaxDocuments);
                lblMaxSize.Text =
                    SystemManager.MStringResource.GetText(StringResource.TextType.Collection_Status_MaxSize);
                chkIsAutoIndexId.Text =
                    SystemManager.MStringResource.GetText(StringResource.TextType.Collection_Status_IsAutoIndexId);
            }

            //Difference between with long and decimal.....
            numMaxDocument.Maximum = decimal.MaxValue;
            numMaxSize.Maximum = decimal.MaxValue;
            chkAdvance.Checked = false;
            chkAdvance.Location = new Point(grpAdvanced.Location.X + 10, grpAdvanced.Location.Y);
            chkAdvance.BringToFront();
            grpAdvanced.Enabled = false;
        }

        /// <summary>
        ///     OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdOK_Click(object sender, EventArgs e)
        {
            //不支持中文 JIRA ticket is created : SERVER-4412
            //SERVER-4412已经在2013/03解决了
            //collection names are limited to 121 bytes after converting to UTF-8. 
            if (txtCollectionName.Text == String.Empty) return;
            try
            {
                String ErrMessage;
                SystemManager.GetCurrentDataBase().IsCollectionNameValid(txtCollectionName.Text, out ErrMessage);
                if (ErrMessage != null)
                {
                    MyMessageBox.ShowMessage("Create MongoDatabase", "Argument Exception", ErrMessage, true);
                    return;
                }
                if (chkAdvance.Checked)
                {
                    var option = new CollectionOptionsBuilder();
                    option.SetCapped(chkIsCapped.Checked);
                    option.SetMaxSize((long) numMaxSize.Value);
                    option.SetMaxDocuments((long) numMaxDocument.Value);
                    //CappedCollection Default is AutoIndexId After MongoDB 2.2.2
                    option.SetAutoIndexId(chkIsAutoIndexId.Checked);
                    Result = MongoDbHelper.CreateCollectionWithOptions(strSvrPathWithTag, treeNode,
                        txtCollectionName.Text, option);
                }
                else
                {
                    Result = MongoDbHelper.CreateCollection(strSvrPathWithTag, treeNode, txtCollectionName.Text);
                }
                Close();
            }
            catch (ArgumentException ex)
            {
                SystemManager.ExceptionDeal(ex, "Create MongoDatabase", "Argument Exception");
                Result = false;
            }
        }

        /// <summary>
        ///     Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     高级选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAdvance_CheckedChanged(object sender, EventArgs e)
        {
            grpAdvanced.Enabled = chkAdvance.Checked;
        }

        /// <summary>
        ///     CappedCollections官方说明
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lnkCappedCollections_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://docs.mongodb.org/manual/core/capped-collections/");
        }
    }
}