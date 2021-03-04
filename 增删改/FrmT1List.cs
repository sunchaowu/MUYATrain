using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Data;
using MUYA.API;
using MUYA.Utility;

/// <summary>
/// 命名控件只设置两层，方便后期识别和维护：MUYA.XXX,XXX代表模块名
/// </summary>
namespace MUYA.Train
{
    /// <summary>
    /// 1.页面类必须继承 DevExpress.XtraEditors.XtraForm,统一界面样式
    /// 2.界面注释最少5块，全局变量/构造函数/初始化/界面事件/其他逻辑，如果其他逻辑过多，可以拆分多块
    /// </summary>
    public partial class FrmT1List : XtraForm
    {
        #region 全局变量
        //定义当前页面的全局访问数据库对象
        Session session = AppData.session;
        //定义主列表分页控件
        Pager page;
        string setting;
        #endregion

        #region 构造函数
        public FrmT1List()
        {
            InitializeComponent();
            //初始化主列表分页控件，第一个参数绑定主列表
            page = new Pager(gridMain);
            //把分页的按钮添加到当前页面上
            barManager1.Bars.Add(page.barPage);
        }
        #endregion

        #region 初始化
        private void FrmT1List_Load(object sender, EventArgs e)
        {
            LoadLanguage();
            BindDataSource();
            BindData();
        }
        #endregion

        #region 界面事件
        /// <summary>
        /// 界面加载的时候执行，一般加载数据和初始化数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonOpen_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //判断选择了行
            if (gridViewMain.FocusedRowHandle < 0)
            {
                return;
            }
            //创建明细页面
            var frm = new FrmT1Edit(session, page.pageData.Data, gridViewMain.GetFocusedDataSourceRowIndex(), false);
            //明细页面关闭后，刷新当前页面
            frm.FormClosed += (a, se) => BindData();
            //通用打开明细方法
            MUYA.Utility.Utility.OpenDetialForm(this, frm,gridViewMain.GetFocusedDataRow(), "编辑","ID");
        }
        /// <summary>
        /// 新增按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonNew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var frm = new FrmT1Edit(session, page.pageData.Data, -1, true);
            frm.FormClosed += (a, se) => BindData();
            MUYA.Utility.Utility.OpenDetialForm(this, frm, null);
        }
        /// <summary>
        /// 删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonDelete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            MUYA.Utility.Utility.SetPageDelete(sqlParamer =>
            {
                string ID = gridViewMain.GetFocusedDataRow()["ID"].ConvertTo("");
                sqlParamer.Add("ID", ID);
                return " 'TRA_T1','ID',@ID";
            }, sqlParamer =>
            {
                var sqlDel = "Update TRA_T1 set IsDelete = 1 where ID = @ID;Update TRA_TD1 set IsDelete = 1 where MID = @ID;Update TRA_TD2 set IsDelete = 1 where MID = @ID";
                var rt = session.DoSQL(sqlDel, sqlParamer);
                if (rt > 0)
                {
                    BindData();
                }
            });
        }
        #endregion

        #region 其它逻辑
        /// <summary>
        /// 绑定数据
        /// </summary>
        private void BindData()
        {
            var sqlParameter = new Dictionary<string, object>();
            sqlParameter.Add("S1", setting.ConvertTo(""));
            string sql = "select * from TRA_T1 where S1 = @S1 and IsDelete = 0 and IsEnable = 1";
            page.BindData(sql, "ID", sqlParameter);
            page.ShowColumns(true, "UpdateUser", "UpdateDate");
        }
        /// <summary>
        /// 初始化界面控件资源，下拉列表/菜单参数配置等
        /// </summary>
        private void BindDataSource()
        {
            DataSet dsAllFD = MUYA.Utility.Utility.GetFrmAllFuncD(this.Tag);
            //设置导入导出模板
            page.SetImportTemp("T1");
            if (dsAllFD != null && dsAllFD.Tables.Count > 0)
            {
                foreach (DataRow item in dsAllFD.Tables[0].Rows)
                {
                    if (item["FuncType"].ToString() == "Setting")
                    {
                        setting = item["Value"].ConvertTo("");
                    }
                }
            }
            MUYA.Utility.Utility.BindListBox(repositoryItemImageComboBoxImg1);
            MUYA.Utility.Utility.BindListBox(repositoryItemLookUpSL1, "Status");
        }
        /// <summary>
        /// 初始化按钮和语言翻译
        /// </summary>
        private void LoadLanguage()
        {
            ImageHelper.InitBarButton(barButtonOpen, "open", session.GetMsg("open"));
            ImageHelper.InitBarButton(barButtonDelete, "delete", session.GetMsg("delete"));
            ImageHelper.InitBarButton(barButtonNew, "new", session.GetMsg("new"));
            PageLanguage.InitLanguage(this);
        }
        #endregion
    }
}
