using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Extensions;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Forms.Common;
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
    public partial class StopIssueForm : Form
    {
        // 콤보 순서 = 이 배열 순서. SelectedIndex로 enum 값(PD01~)을 되찾을 때 같은 배열을 쓴다.
        private readonly PopProcessStopType[] _stopTypes = (PopProcessStopType[])Enum.GetValues(typeof(PopProcessStopType));
        /// <summary>[확인] 시 cmbCode에서 고른 인덱스. 취소면 -1.</summary>
        public int SelectedIndex { get; private set; } = -1;

        /// <summary>[확인] 시 고른 중지 사유 — ToString()이 서버로 보낼 코드(PD01~PD09)다.</summary>
        public PopProcessStopType StopType { get; private set; }

        /// <summary>[확인] 시 txtRemark 내용.</summary>
        public string Remark { get; private set; }

        public StopIssueForm()
        {
            InitializeComponent();
        }

        private void StopIssueForm_Load(object sender, EventArgs e)
        {
            cmbCode.Items.Clear();
            foreach (var type in _stopTypes)
                cmbCode.Items.Add(type.GetDescription());

            if (cmbCode.Items.Count > 0)
                cmbCode.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var index = cmbCode.SelectedIndex;
            if (index < 0 || index >= _stopTypes.Length)
            {
                // 사유 없이 중지되면 비가동 집계가 안 되므로 창을 유지한다.
                //AppMessageBox.Warn(this, Language.StopIssue_SelectCode, Language.StopIssue_Title);
                return;
            }

            SelectedIndex = index;
            StopType = _stopTypes[index];
            Remark = txtRemark.Text?.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
