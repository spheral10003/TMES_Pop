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

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class PopScreen
    {
        private List<PopInspectionSpecDto> _inspections = new List<PopInspectionSpecDto>();

        public void InspectionInit()
        {
            ConfigureInspectionGrid();

            LoadInspectionAsync();
        }

        private async Task LoadInspectionAsync()
        {
            _inspections = await LoadGridAsync(gridInspection, FetchInspectionsAsync, _inspections);
        }

        private async Task<List<PopInspectionSpecDto>> FetchInspectionsAsync()
        {
            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetInspectionSpecs(_key);
            }

            return await _appSet.ApiClients.PopApiClient.GetInspectionSpecsAsync(_key)
                   ?? new List<PopInspectionSpecDto>();
        }



        /// <summary>정성 검사 줄에서는 결과값 칸을 열지 않는다(결과상태만 고른다).</summary>
        private void GridViewInspection_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (gridViewInspection.FocusedColumn?.FieldName != "ResultValue")
                return;

            var spec = gridViewInspection.GetFocusedRow() as PopInspectionSpecDto;
            if (spec != null && !spec.IsQuantitative)
                e.Cancel = true;
        }
    }
}
