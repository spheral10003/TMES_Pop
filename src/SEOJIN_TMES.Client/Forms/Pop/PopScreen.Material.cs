using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class PopScreen
    {
        private List<PopInputMaterialDto> _materials = new List<PopInputMaterialDto>();

        private async Task<List<PopInputMaterialDto>> GetInputMaterials()
        {
            //if (UseMock)
            //{
            //    await Task.Yield();
            //    return MockPopData.GetInputMaterials(_key);
            //}

            return await _appSet.ApiClients.PopApiClient.GetInputMaterialsAsync(_key)
                   ?? new List<PopInputMaterialDto>();
        }


        private bool MaterialsChk()
        {
            for (int i = 0; i < _materials.Count; i++)
            {
                var item = _materials[i];
                if (item.IsSerialInputCompleted)
                    continue;

                int rowHandle = gridViewMaterial.GetRowHandle(i);   // DataSource 인덱스 → rowHandle
                gridViewMaterial.FocusedRowHandle = rowHandle;
                gridViewMaterial.FocusedColumn = gridViewMaterial.Columns["SerialInputStateText"];

                var e = new RowCellClickEventArgs(
                    new DXMouseEventArgs(MouseButtons.Left, 1, 0, 0, 0),
                    rowHandle,
                    gridViewMaterial.Columns["SerialInputStateText"]
                    );

                GridViewMaterial_RowCellClick(gridViewMaterial, e); // RowCellClick 이벤트 핸들러 호출
                
            }

            // 모든 자재의 시리얼 입력이 끝났을 때만 BOM 탭으로 이동
            if (_materials.Count > 0 && _materials.All(m => m.IsSerialInputCompleted))
                tabSpec.SelectedTabPage = pageBom;

            return true;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  탭 1 — 투입자재 시리얼번호 입력
        // ─────────────────────────────────────────────────────────────────────────

        ///// <summary>"시리얼번호 입력상태" 칸을 클릭하면 &lt;투입자재 정보 입력&gt; 팝업을 연다.</summary>
        //private async void GridViewMaterial_RowCellClick(object sender, RowCellClickEventArgs e)
        //{
        //    if (e.Column == null || e.Column.FieldName != "SerialInputStateText")
        //        return;

        //    var material = gridViewMaterial.GetRow(e.RowHandle) as PopInputMaterialDto;
        //    if (material == null)
        //        return;

        //    if (!material.SerialCheckRequired)
        //    {
        //        AppMessageBox.Info(this, Language.Pop_SerialNotRequired, Language.PopSerial_Title);
        //        return;
        //    }

        //    List<string> serials;
        //    using (var form = new MaterialSerialForm())
        //    {
        //        form.Initialize(material);
        //        form.CenterOn(this);
        //        if (form.ShowDialog(this) != DialogResult.OK)
        //            return;

        //        serials = form.SerialNumbers;
        //    }

        //    material.SerialNumbers = serials;
        //    gridViewMaterial.RefreshData();

        //    await SaveMaterialSerialsAsync(material);
        //}

        private async Task SaveMaterialSerialsAsync(PopInputMaterialDto material)
        {
            try
            {
                //if (!UseMock)
                //{
                //var request = new PopMaterialSerialSaveRequest
                //{
                //    Key = _key,
                //    CorpCode = _key.CorpCode,
                //    WorkOrderNo = _key.WorkOrderNo,
                //    SerialNo = _key.SerialNo,
                //    RoutingGroup = _key.RoutingGroup,
                //    RoutingOperation = _key.RoutingOperation,
                //    PageOperation = _key.PageOperation,
                //    SubRoutingRef = _key.SubRoutingRef,
                //    RoutingGroupRef = _key.RoutingGroupRef,
                //    MaterialCode = material.MaterialCode,
                //    SerialNumbers = material.SerialNumbers
                //};

                var request = new PopMaterialSerialSaveRequest
                {
                    inputtedItemCd = material.MaterialCode,
                    inputtedSerialList = material.SerialNumbers
                };

                BeginResultSave();
                try
                {
                    await _appSet.ApiClients.PopApiClient.SaveMaterialSerialsAsync(request, _key, material.MaterialCode);
                }
                finally
                {
                    EndResultSave();
                }
                //}

                if (IsDisposed || Disposing) return;
                AppMessageBox.Success(this, Language.PopSerial_Saved, Language.PopSerial_Title);
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;
                AppMessageBox.Warn(this, ex.Message, Language.PopSerial_Title);
                return;
            }

            await TryAutoProgressAsync();
        }





    }
}
