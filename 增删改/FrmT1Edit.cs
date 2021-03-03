using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MUYA.API;
using MUYA.Utility;

namespace MUYA.Train
{
    /// <summary>
    /// 命名控件只设置两层，方便后期识别和维护：MUYA.XXX,XXX代表模块名
    /// </summary>
    public partial class FrmT1Edit : DevExpress.XtraEditors.XtraForm
    {
        //S1通过菜单传入固定值
        //维护主表S2不能重复的操作
        //S3下拉选择

        #region 全局变量
        //定义当前页面的全局访问数据库对象
        Session session = AppData.session;
        //主列表内存表，来自主列表打开前传值
        DataSet dsMain;
        //标记当前行在主列表的行索引
        int idx;
        //是否标记新增
        bool isnew;
        //当前编辑行内容
        DataRow drMain;
        //定义主列表分页控件
        Pager pageDT1;
        //定义第二个分页列表控件
        Pager pageDT2;
        #endregion

        #region 构造函数
        public FrmT1Edit(Session Session, DataSet DataMain, int RowIdx, bool IsNew)
        {
            InitializeComponent();
            session = Session;
            dsMain = DataMain;
            idx = RowIdx;
            isnew = IsNew;
            //初始化分页
            pageDT1 = new Pager(gridMainTD1);
            this.barManager1.Bars.Add(pageDT1.barPage);
            //自定义导入
            pageDT1.ImportButtonClicked += barButtonImport_ItemClick;

            //如果存在两个分页，一次只能显示一个
            pageDT2 = new Pager(gridControlTD2);
            this.barManager1.Bars.Add(pageDT2.barPage);
            //默认不显示
            pageDT2.barPage.Visible = false;
        }
        #endregion

        #region 初始化
        private void FrmSiteEdit_Load(object sender, EventArgs e)
        {
            if (isnew)
            {
                drMain = dsMain.Tables[0].NewRow();
                //如果idx>0 复制
                if (idx > 0)
                {
                    drMain.ItemArray = dsMain.Tables[0].Rows[idx].ItemArray;
                }
            }
            else
            {
                drMain = dsMain.Tables[0].Rows[idx];
            }
            BindDataSource();
            ReadData(xtraTabControl1.SelectedTabPage);
            LoadLanguage();
        }

        private void ReadData(DevExpress.XtraTab.XtraTabPage tabPage)
        {
            if (tabPage == tabTD1)
            {
                BindDataDT1();
                pageDT1.barPage.Visible = true;
                pageDT2.barPage.Visible = false;
            }
            else if (tabPage == tabTD2)
            {
                BindDataDT2();
                pageDT2.barPage.Visible = true;
                pageDT1.barPage.Visible = false;
            }
        }
        #endregion

        #region 界面事件
        /// <summary>
        /// 选项卡切换事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xtraTabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            ReadData(e.Page);
        }
        /// <summary>
        /// 保存按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            bool success = Save();
            if (success)
            {
                DialogResult dr = AppData.ShowMsg(session.GetMsg("SaveSucess") + session.GetMsg("IsExit"), session.GetMsg("Information"), MessageBoxButtons.OKCancel, MessageBoxIcon.Information, false);
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    this.Close();
                }
            }
        }
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonImport_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //自定义导入
        }
        #endregion

        #region 其它逻辑
        /// <summary>
        /// 初始化页面参数
        /// </summary>
        private void BindDataSource()
        {
            txtS1.EditValue = drMain["S1"];
            txtS2.EditValue = drMain["S2"];
            txtSL1.EditValue = drMain["SL1"];
            txtDec1.EditValue = drMain["Dec1"];
            txtI1.EditValue = drMain["I1"];
            txtDt1.EditValue = drMain["Dt1"];
            txtDt2.EditValue = drMain["Dt2"];
            txtImg1.EditValue = drMain["Img1"];
            txtB1.EditValue = drMain["B1"];
            txtS3.EditValue = drMain["S3"];
            MUYA.Utility.Utility.BindListBox(txtImg1);
            MUYA.Utility.Utility.BindListBox(txtSL1, "Status");
            MUYA.Utility.Utility.BindListBox(repositoryItemLookUpSL1, "Status");
            MUYA.Utility.Utility.BindListBox(repositoryItemImageImg1);
        }
        private void BindDataDT1()
        {
            var sqlParameter = new Dictionary<string, object>();
            sqlParameter.Add("ID", drMain["ID"].ConvertTo(""));
            string sql = "Select * from TRA_TD1 where MID=@ID and IsDelete = 0 and IsEnable = 1";
            pageDT1.BindData(sql, "ID", sqlParameter);
        }
        private void BindDataDT2()
        {
            var sqlParameter = new Dictionary<string, object>();
            sqlParameter.Add("ID", drMain["ID"].ConvertTo(""));
            var sqlWH = "select * from TRA_TD2 where MID=@ID and IsDelete = 0 and IsEnable = 1";
            pageDT2.BindData(sqlWH, "ID", sqlParameter);
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        private bool Save()
        {
            #region 验证界面和控件，完成所有未完成的执行
            this.Validate();
            this.ValidateChildren(ValidationConstraints.None);
            gridViewTD1.ValidateEditor();
            gridViewTD2.ValidateEditor();
            #endregion

            try
            {
                #region 界面验证
                //验证S2不能为空
                if (this.txtS2.Text == "" || this.txtS2.Text == null)
                {
                    AppData.ShowMsg(lblS1.Text + session.GetMsg("NotNull"), session.GetMsg("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error, false);
                    return false;
                }
                #endregion

                #region 新增判断已经删除的启用
                ///////////////////////////////////////////////////////
                DateTime dNow = session.GetServerTime();
                if (isnew) //新增判断
                {
                    //判断主表是否存在 S2不能重复
                    var sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Add("S2", txtS2.Text);
                    var exdataSet = session.GetData("select top 1 * from TRA_T1 where S2 = @S2", sqlParameter);
                    if (exdataSet != null && exdataSet.Tables.Count > 0 && exdataSet.Tables[0].Rows.Count > 0)
                    {
                        //新增，并且是原来删除的
                        if (exdataSet.Tables[0].Rows[0]["IsDelete"].ConvertTo(true))
                        {
                            drMain = exdataSet.Tables[0].Rows[0];
                            dsMain = exdataSet;
                        }
                        else
                        {
                            AppData.ShowMsg(string.Format(session.GetMsg("ExistPK"), txtS1.EditValue), session.GetMsg("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning, false);
                            return false;
                        }
                    }
                    else
                    {
                        //新增并且数据库不存在删除的
                        drMain["ID"] = Guid.NewGuid();
                        dsMain.Tables[0].Rows.Add(drMain);
                    }
                    drMain["CreateDate"] = dNow;
                    drMain["CreateUser"] = session.CurrentUser.UserID;
                }
                else //修改
                {
                    drMain["UpdateDate"] = dNow;
                    drMain["UpdateUser"] = session.CurrentUser.UserID;
                }
                #region 子表判断，如果子表除了ID，有唯一值判断，需要添加下面判断
                //子表判断，如果子表除了ID，有唯一值判断，需要添加下面判断
                DataSet td2DS = new DataSet();
                if (pageDT2.pageData.Data != null)
                {
                    //判断从表TD2新增是否存在已经删除的 S1+S2 = 主表 S3当前页面唯一
                    var sqldetParameter = new Dictionary<string, object>();
                    sqldetParameter.Add("S1", txtS1.Text);
                    sqldetParameter.Add("S2", txtS2.Text);
                    var s2s = pageDT2.pageData.Data.Tables[0].Select().Where(a => a.RowState == DataRowState.Added).Select(a => a["S3"].ConvertTo(""));
                    var sql = $"select * from TRA_TD2 where S1 = @S1 and S2 = @S2 and S3 in ({TRCommon.SplitToStr(string.Join(",", s2s.ToArray()), sqldetParameter)})";
                    td2DS = session.GetData(sql, sqldetParameter);
                    if (td2DS != null && td2DS.Tables.Count > 0 && td2DS.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow ptrow in td2DS.Tables[0].Rows)
                        {
                            //新增，并且是原来删除的
                            if (ptrow["IsDelete"].ConvertTo(false))
                            {
                                var ptdrs = pageDT2.pageData.Data.Tables[0].Select($"S3='{ptrow["S3"]}'");
                                if (ptdrs.Length > 0)
                                {
                                    TRCommon.SetDR(ptdrs[0], ptrow);
                                    ptdrs[0].Delete();
                                }
                                ptrow["S1"] = txtS1.EditValue;
                                ptrow["S2"] = txtS2.EditValue;
                                ptrow["CreateDate"] = dNow;
                                ptrow["CreateUser"] = session.CurrentUser.UserID;
                                ptrow["IsEnable"] = true;
                                ptrow["IsDelete"] = false;
                            }
                            else
                            {
                                AppData.ShowMsg(string.Format(session.GetMsg("ExistPK"), ptrow["WarehouseCode"]), session.GetMsg("Error"), MessageBoxButtons.OK, MessageBoxIcon.Warning, false);
                                return false;
                            }
                        }
                    }
                }
                #endregion
                ///////////////////////////////////////////////////////
                #endregion

                #region 主表赋值
                drMain["S1"] = txtS1.EditValue;
                drMain["S2"] = txtS2.EditValue;
                drMain["SL1"] = txtSL1.EditValue;
                drMain["Dec1"] = txtDec1.Value;
                drMain["I1"] = txtI1.Value;
                drMain["Dt1"] = txtDt1.DateTime;
                drMain["Dt2"] = txtDt2.DateTime;
                drMain["Img1"] = txtImg1.EditValue;
                drMain["B1"] = txtB1.Checked;
                drMain["S3"] = txtS3.EditValue;
                drMain["IsEnable"] = true;
                drMain["IsDelete"] = false;
                #endregion

                #region 子表赋值
                if (pageDT1.pageData.Data != null)
                {
                    foreach (DataRow item in pageDT1.pageData.Data.Tables[0].Rows)
                    {
                        if (item.RowState == DataRowState.Modified)
                        {
                            item["UpdateDate"] = dNow;
                            item["UpdateUser"] = session.CurrentUser.UserID;
                        }
                        else if (item.RowState == DataRowState.Added)
                        {
                            item["ID"] = Guid.NewGuid();
                            item["MID"] = drMain["ID"];
                            item["CreateDate"] = dNow;
                            item["CreateUser"] = session.CurrentUser.UserID;
                            item["IsEnable"] = true;
                            item["IsDelete"] = false;
                        }
                    }
                }
                if (pageDT2.pageData.Data != null)
                {
                    foreach (DataRow item in pageDT2.pageData.Data.Tables[0].Rows)
                    {
                        if (item.RowState == DataRowState.Modified)
                        {
                            item["UpdateDate"] = dNow;
                            item["UpdateUser"] = session.CurrentUser.UserID;
                        }
                        else if (item.RowState == DataRowState.Added)
                        {
                            item["ID"] = Guid.NewGuid();
                            item["MID"] = drMain["ID"];
                            item["CreateDate"] = dNow;
                            item["CreateUser"] = session.CurrentUser.UserID;
                            item["IsEnable"] = true;
                            item["IsDelete"] = false;
                        }
                    }
                }
                #endregion

                #region 更新数据库
                session.BeginUpdate();
                var rt = session.UpdateDataSet("TRA_T1", dsMain);
                if (rt < 0) { session.EndUpdate(false); return false; }
                if (pageDT1.pageData.Data != null)
                {
                    rt = session.UpdateDataSet("TRA_TD1", pageDT1.pageData.Data);
                    if (rt < 0) { session.EndUpdate(false); return false; }
                }
                if (pageDT2.pageData.Data != null)
                {
                    rt = session.UpdateDataSet("TRA_TD2", pageDT2.pageData.Data);
                    if (rt < 0) { session.EndUpdate(false); return false; }
                    rt = session.UpdateDataSet("TRA_TD2", td2DS);
                    if (rt < 0) { session.EndUpdate(false); return false; }
                }
                session.EndUpdate(true);
                #endregion

                #region 刷新界面表
                dsMain.AcceptChanges();
                pageDT1.pageData.AcceptChanges();
                pageDT2.pageData.AcceptChanges();
                if (isnew)
                {
                    txtS1.Properties.ReadOnly = true;
                    txtS2.Properties.ReadOnly = true;
                    isnew = false;
                }
                ReadData(xtraTabControl1.SelectedTabPage);
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                AppData.ShowMsg(ex.Message, session.GetMsg("Error"), MessageBoxButtons.OK, MessageBoxIcon.Stop, false);
                return false;
            }
        }
        /// <summary>
        /// 选择按钮选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void repositoryItemButtonS1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (e.Button.Kind == DevExpress.XtraEditors.Controls.ButtonPredefines.Ellipsis)
            {
                //弹窗选择品号
                string sql = "select PartCode,PartName,PartPicture,PartSpec,isnull(p2.SNP,0) SNP from MDC_Part p left join dbo.MDC_PartEx2 p2 on p.ID = p2.ID";
                FrmGetList frm = new FrmGetList(true, sql, "PartCode");
                frm.Tag = Tag;
                frm.Text = gridSL1.Caption + AppData.session.GetMsg("choose");
                frm.ShowDialog();
                DataTable dt = frm.dsMainSave.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        if (this.gridViewTD1.GetFocusedDataRow() == null)
                        {
                            this.gridViewTD1.AddNewRow();
                        }
                        DataRow[] drs = pageDT1.pageData.Data.Tables[0].Select(string.Format("S1 = '{0}'", item["PartCode"]));
                        if (drs.Length == 0)
                        {
                            this.gridViewTD1.SetFocusedRowCellValue("PartCode", item["PartCode"]);
                        }
                    }
                }
                ////调用已有的方法
                //TRCommon.ShowPart(gridSL1.Caption, this.Tag, true, item =>
                // {
                //     if (this.gridViewTD1.GetFocusedDataRow() == null)
                //     {
                //         this.gridViewTD1.AddNewRow();
                //     }
                //     DataRow[] drs = pageDT1.pageData.Data.Tables[0].Select(string.Format("S1 = '{0}'", item["PartCode"]));
                //     if (drs.Length == 0)
                //     {
                //         this.gridViewTD1.SetFocusedRowCellValue("PartCode", item["PartCode"]);
                //     }
                // });
            }
        }
        private void gridViewTD1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (gridViewTD1.FocusedRowHandle < 0)
                {
                    return;
                }
                Utility.Utility.SetPageDelete(a =>
                {
                    var ID = gridViewTD1.GetFocusedDataRow()["ID"].ConvertTo("");
                    a.Add("ID", ID);
                    return " 'TRA_DT1','ID',@ID";
                }, a =>
                {
                    pageDT1.pageData.SetCurrentRowValue("IsDelete", true);
                }, false);
            }
        }
        private void gridViewWH_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (gridViewTD2.FocusedRowHandle < 0)
                {
                    return;
                }
                Utility.Utility.SetPageDelete(a =>
                {
                    var ID = gridViewTD2.GetFocusedDataRow()["ID"].ConvertTo("");
                    a.Add("ID", ID);
                    return " 'TRA_DT2','ID',@ID";
                }, a =>
                {
                    pageDT1.pageData.SetCurrentRowValue("IsDelete", true);
                }, false);
            }
        }
        private void LoadLanguage()
        {
            //设置分页导入模板
            pageDT1.SetImportTemp("DT1");
            //设置分页导入模板
            pageDT2.SetImportTemp("DT2");
            //翻译按钮
            ImageHelper.InitBarButton(barButtonSave, "save", session.GetMsg("Save"));
            //翻译页面
            MUYA.Utility.PageLanguage.InitLanguage(this);
        }
        #endregion

    }
}
