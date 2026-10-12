using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Mock;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class PopScreen
    {
        private List<PopInputMaterialDto> _materials = new List<PopInputMaterialDto>();

        public void MaterialInit()
        {
            ConfigureMaterialGrid();
            LoadMaterialAsync();
        }

        private async Task LoadMaterialAsync()
        {
            _materials = await LoadGridAsync(gridMaterial, FetchMaterialsAsync, _materials);
        }



        private async Task<List<PopInputMaterialDto>> FetchMaterialsAsync()
        {
            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetInputMaterials(_key);
            }

            return await _appSet.ApiClients.PopApiClient.GetInputMaterialsAsync(_key)
                   ?? new List<PopInputMaterialDto>();
        }



        // ─────────────────────────────────────────────────────────────────────────
        //  탭 1 — 투입자재 시리얼번호 입력
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>"시리얼번호 입력상태" 칸을 클릭하면 &lt;투입자재 정보 입력&gt; 팝업을 연다.</summary>
        private async void GridViewMaterial_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            if (e.Column == null || e.Column.FieldName != "SerialInputStateText")
                return;

            var material = gridViewMaterial.GetRow(e.RowHandle) as PopInputMaterialDto;
            if (material == null)
                return;

            if (!material.SerialCheckRequired)
            {
                AppMessageBox.Info(this, Language.Pop_SerialNotRequired, Language.PopSerial_Title);
                return;
            }

            List<string> serials;
            using (var form = new MaterialSerialForm())
            {
                form.Initialize(material);
                form.CenterOn(this);
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                serials = form.SerialNumbers;
            }

            material.SerialNumbers = serials;
            gridViewMaterial.RefreshData();

            await SaveMaterialSerialsAsync(material);
        }

        private async Task SaveMaterialSerialsAsync(PopInputMaterialDto material)
        {
            try
            {
                if (!UseMock)
                {
                    var request = new PopMaterialSerialSaveRequest
                    {
                        WorkOrderNo = _key.WorkOrderNo,
                        SerialNo = _key.SerialNo,
                        RoutingGroup = _key.RoutingGroup,
                        RoutingOperation = _key.RoutingOperation,
                        PageOperation = _key.PageOperation,
                        MaterialCode = material.MaterialCode,
                        SerialNumbers = material.SerialNumbers
                    };

                    await _appSet.ApiClients.PopApiClient.SaveMaterialSerialsAsync(request);
                }

                AppMessageBox.Success(this, Language.PopSerial_Saved, Language.PopSerial_Title);
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Language.PopSerial_Title);
            }
        }





    }
}
