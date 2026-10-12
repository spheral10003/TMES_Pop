using DevExpress.XtraEditors.Repository;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.WorkOrderEntry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class PopScreen2 : UserControl
    {
        private Setting _appSet;
        private Devices _equip;
        private WorkEntryContext _entry;
        private PopProcessKey _key;
        private PopProcessInfoDto _info;

        private RepositoryItemButtonEdit _resultInputEditor;
        private RepositoryItemTextEdit _torqueResultEditor;
        private RepositoryItemComboBox _resultStateEditor;
        private RepositoryItemTextEdit _resultValueEditor;

        public PopScreen2()
        {
            InitializeComponent();
        }

        private void PopScreen2_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// POP 진입 화면에서 선택한 작업 대상(<paramref name="entry"/>)을 받아 화면을 구성한다.
        /// 메뉴에서 바로 열린 경우처럼 대상이 없으면(null) 조회 없이 안내만 하고 빈 화면으로 남는다.
        /// </summary>
        public void Init(WorkEntryContext entry = null)
        {
            _appSet = Global.Instance.Settings;
            _equip = Global.Instance.Devices;

            _entry = entry;
            //_key = BuildKey(entry);

            //// 작업지시가 바뀌면 공정 목록도 다른 것이라, 다음 이동 때 새로 읽게 비워 둔다.
            //_processes = new List<RoutingProcessDto>();

            //ApplyLanguage();
            //ApplyTheme();
            //ConfigureActionTiles();
            ////SetActiveTab(PopTab.Material);
            //WireEquipmentEvents();
            //_viewerMessage = Language.Pop_DrawingNone;

            //StartClock();
        }




    }
}
