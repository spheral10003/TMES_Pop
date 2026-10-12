using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Equipment.Core;
using SEOJIN_TMES.Client.Equipment.Nutrunner;
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
        private List<PopEquipmentDto> _equipments = new List<PopEquipmentDto>();

        public void EquipmentInit()
        {
            ConfigureEquipmentGrid();
            WireEquipmentEvents();
            LoadEquipAsync();

        }

        private async Task LoadEquipAsync()
        {
            try
            {
                _equipments = await FetchEquipmentsAsync();
                gridEquipment.DataSource = _equipments;

                RefreshEquipmentStates();
            }
            catch (ApiException ex)
            {
                AppMessageBox.Warn(this, ex.Message, Language.Pop_Title);
                return;
            }
        }

        private async Task<List<PopEquipmentDto>> FetchEquipmentsAsync()
        {
            var workstation = _appSet?.CurrentUser?.Workstation;

            if (UseMock)
            {
                await Task.Yield();
                return MockPopData.GetWorkstationEquipments(workstation);
            }

            return await _appSet.ApiClients.PopApiClient.GetWorkstationEquipmentsAsync(workstation)
                   ?? new List<PopEquipmentDto>();
        }


        private void ConfigureEquipmentGrid()
        {
            gridEquipment.DataSource = new List<PopEquipmentDto>();
            GridColumns.Apply(gridViewEquipment, Language.Col_No,
                GridColumns.Text("EquipmentCode", Language.Col_EquipmentCode, 160),
                GridColumns.Text("EquipmentName", Language.Col_EquipmentName, 150),
                GridColumns.Left("EquipmentRemark", Language.Col_EquipmentRemark, 260),
                GridColumns.Text("EquipmentType", Language.Col_EquipmentType, 130),
                GridColumns.Text("CommunicationState", Language.Col_CommunicationState, 130),
                GridColumns.Text("EquipmentState", Language.Col_EquipmentState, 130),
                GridColumns.Text("RangeLower", Language.Col_RangeLower, 150),
                GridColumns.Text("RangeUpper", Language.Col_RangeUpper, 150));

            gridViewEquipment.OptionsView.ColumnAutoWidth = true;
            NumericFormat(gridViewEquipment, "RangeLower", "RangeUpper");
        }


        /// <summary>연결 실패/사용불가 상태는 붉은 글씨로 눈에 띄게 한다(상태 표시일 뿐 판정은 아니다).</summary>
        private void GridViewEquipment_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName != "CommunicationState" && e.Column.FieldName != "EquipmentState")
                return;

            var equipment = gridViewEquipment.GetRow(e.RowHandle) as PopEquipmentDto;
            if (equipment == null)
                return;

            var connected = equipment.CommunicationState == Language.Pop_CommConnected;
            e.Appearance.ForeColor = connected ? AppTheme.Emerald : AppTheme.Rose;
        }


        // ─────────────────────────────────────────────────────────────────────────
        //  탭 4 — 연결 장비 / 설비 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        private void WireEquipmentEvents()
        {
            _equip.Manager.ConnectionChanged += OnConnectionChanged;
            _equip.Manager.AlarmOccurred += OnErrorOccurred;
            _equip.Manager.RegisterDataReceived += OnRegisterDataReceived;
            _equip.Manager.FasteningReceived += OnFasteningReceived;

        }

        private void OnFasteningReceived(object sender, NutrunnerFasteningEventArgs e)
        {
            InvokeIfRequired(() =>
            {
                FasteningData data = e.Data;

                AppLog.Warn("────────────────────────");
                AppLog.Warn($"[ID {e.Id}] Fastening");
                AppLog.Warn($"[ToolId] : {data.ToolId}");
                AppLog.Warn($"[FastenTime] : {data.FastenTime}");
                AppLog.Warn($"[Preset] : {data.Preset}");
                AppLog.Warn($"[TargetTorque] : {data.TargetTorqueValue:0.00}");
                AppLog.Warn($"[Torque] : {data.TorqueValue:0.00}");
                AppLog.Warn($"[Speed] : {data.Speed}");
                AppLog.Warn($"[Angle1] : {data.Angle1}");
                AppLog.Warn($"[Angle2] : {data.Angle2}");
                AppLog.Warn($"[Angle3] : {data.Angle3}");
                AppLog.Warn($"[RemainScrew] : {data.RemainScrew}");
                AppLog.Warn($"[Error] : {data.Error}");
                AppLog.Warn($"[Direction] : {data.Direction}");
                AppLog.Warn($"[EventStatus] : {data.EventStatus}");
                AppLog.Warn($"[SnugAngle] : {data.SnugAngle}");
                AppLog.Warn($"[Barcode] : {data.Barcode}");


                ApplyTorqueFromEquipment(e.Id.ToString(), (decimal)data.TorqueValue, (decimal)data.Angle3);

            });
        }

        private void OnRegisterDataReceived(object sender, NutrunnerRegisterEventArgs e)
        {
            InvokeIfRequired(() =>
            {
                if (!e.Success)
                {
                    return;
                }

                ushort address = e.StartAddress;

                foreach (ushort value in e.Data)
                {
                    AppLog.Warn($"[ID {e.Id}][{address++:D4}] : {value}");
                }

            });
        }

        private void OnConnectionChanged(object sender, EquipmentConnectionEventArgs e)
        {
            InvokeIfRequired(() =>
            {
                AppLog.Warn($"[ID {e.Id}] {(e.IsConnected ? "연결 됨" : "연결 끊김")}");

                foreach (var equipment in _equipments)
                {
                    INutrunner nutrunner;
                    _equip.Manager.TryGet(e.Id, out nutrunner);

                    if (nutrunner.Info.DisplayName == equipment.EquipmentCode && e.IsConnected)
                    {
                        equipment.CommunicationState = Language.Pop_CommConnected;
                        equipment.EquipmentState = Language.Pop_EquipmentUsable;
                    }
                    else
                    {
                        equipment.CommunicationState = Language.Pop_CommFailed;
                        equipment.EquipmentState = Language.Pop_EquipmentUnusable;

                    }
                }

                gridViewEquipment.RefreshData();
            });
        }

        private void OnErrorOccurred(object sender, AlarmEventArgs e)
        {
            InvokeIfRequired(() =>
                AppLog.Warn($"[ALARM] [{e.OccurredAt} / {e.Code}] {e.Message}")
            );

        }

        private void UnwireEquipmentEvents()
        {
            _equip.Manager.ConnectionChanged -= OnConnectionChanged;
            _equip.Manager.AlarmOccurred -= OnErrorOccurred;
            _equip.Manager.RegisterDataReceived -= OnRegisterDataReceived;
            _equip.Manager.FasteningReceived -= OnFasteningReceived;
        }


        /// <summary>
        /// [연결 장비] 탭의 통신상태/장비상태를 현재 Modbus 연결 상태로 덮어쓴다.
        /// 장비코드(EquipmentCode)가 App.config의 EquipmentId와 같아야 연결 상태가 붙는다.
        /// 표준/이벤트 두 Connection 중 하나라도 연결돼 있으면 "연결 성공"으로 본다.
        /// </summary>
        private void RefreshEquipmentStates()
        {
            if (_equip?.Manager == null)
                return;

            foreach (var equipment in _equipments)
            {
                INutrunner nutrunner;
                _equip.Manager.TryGet(equipment.Id, out nutrunner);

                if (nutrunner.IsConnected && nutrunner.IsEventChannelConnected)
                {
                    equipment.CommunicationState = Language.Pop_CommConnected;
                    equipment.EquipmentState = Language.Pop_EquipmentUsable;
                }
                else
                {
                    equipment.CommunicationState = Language.Pop_CommFailed;
                    equipment.EquipmentState = Language.Pop_EquipmentUnusable;
                }
            }

            gridViewEquipment.RefreshData();

        }



    }
}
