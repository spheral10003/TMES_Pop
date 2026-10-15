using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Equipment.Core;
using SEOJIN_TMES.Client.Equipment.Nutrunner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class PopScreen
    {
        private List<PopEquipmentDto> _equipments = new List<PopEquipmentDto>();
        private List<NutrunnerInfo> _nutrunnerInfos = new List<NutrunnerInfo>();
        private async void FetchEquipmentsAsync()
        {
            //var workstation = _appSet?.CurrentUser?.Workstation;

            _equipments = await _appSet.ApiClients.PopApiClient.GetWorkstationEquipmentsAsync(_key)?? new List<PopEquipmentDto>();

            foreach (var eq in _equipments)
            {
                //RefreshEquipmentStates();

                //INutrunner nutrunner;

                //if (_equip.Manager.Count > 0)
                //{
                    _equip.Manager.TryGet(eq.Id, out INutrunner nutrunner);
                    
                    if (nutrunner is null)
                    {
                        NutrunnerInfo info = new NutrunnerInfo
                        {
                            Id = eq.Id,
                            Code = eq.EquipmentCode,
                            Ip = eq.EquipmentIp,
                            Port = int.Parse(eq.EquipmentPort),
                            EventPort = int.Parse(eq.EquipmentPort) + 1,
                            Name = eq.EquipmentName,
                            ModelName = eq.EquipmentModel,
                        };

                        _equip.TryAdd(info);
                        _equip.Manager.Start(eq.Id);
                    }
                    else
                    {
                        if(!nutrunner.IsConnected || !nutrunner.IsEventChannelConnected)
                        {
                            _equip.Manager.Start(eq.Id);
                        }
                    }
                //}
                
            }

            RefreshEquipmentStates();

            //return await _appSet.ApiClients.PopApiClient.GetWorkstationEquipmentsAsync(_key)
            //       ?? new List<PopEquipmentDto>();
        }


        //private void ConfigureEquipmentGrid()
        //{
        //    gridEquipment.DataSource = new List<PopEquipmentDto>();
        //    GridColumns.Apply(gridViewEquipment, Language.Col_No,
        //        GridColumns.Text("EquipmentCode", Language.Col_EquipmentCode, 160),
        //        GridColumns.Text("EquipmentName", Language.Col_EquipmentName, 150),
        //        GridColumns.Left("EquipmentRemark", Language.Col_EquipmentRemark, 260),
        //        GridColumns.Text("EquipmentType", Language.Col_EquipmentType, 130),
        //        GridColumns.Text("CommunicationState", Language.Col_CommunicationState, 130),
        //        GridColumns.Text("EquipmentState", Language.Col_EquipmentState, 130),
        //        GridColumns.Text("RangeLower", Language.Col_RangeLower, 150),
        //        GridColumns.Text("RangeUpper", Language.Col_RangeUpper, 150));

        //    gridViewEquipment.OptionsView.ColumnAutoWidth = true;
        //    NumericFormat(gridViewEquipment, "RangeLower", "RangeUpper");

        //    gridViewEquipment.Appearance.HorzLine.BackColor = Color.FromArgb(230, 233, 241);
        //    gridViewEquipment.Appearance.HorzLine.Options.UseBackColor = true;
        //    gridViewEquipment.Appearance.VertLine.BackColor = Color.FromArgb(230, 233, 241);
        //    gridViewEquipment.Appearance.VertLine.Options.UseBackColor = true;
        //}


        ///// <summary>연결 실패/사용불가 상태는 붉은 글씨로 눈에 띄게 한다(상태 표시일 뿐 판정은 아니다).</summary>
        //private void GridViewEquipment_RowCellStyle(object sender, RowCellStyleEventArgs e)
        //{
        //    if (e.Column.FieldName != "CommunicationState" && e.Column.FieldName != "EquipmentState")
        //        return;

        //    var equipment = gridViewEquipment.GetRow(e.RowHandle) as PopEquipmentDto;
        //    if (equipment == null)
        //        return;

        //    var connected = equipment.CommunicationState == Language.Pop_CommConnected;
        //    e.Appearance.ForeColor = connected ? AppTheme.Emerald : AppTheme.Rose;
        //}


        // ─────────────────────────────────────────────────────────────────────────
        //  탭 4 — 연결 장비 / 설비 이벤트
        // ─────────────────────────────────────────────────────────────────────────

        private void WireEquipmentEvents()
        {
            int index = 0;
            foreach (var eq in _equipments)
            {
                NutrunnerInfo info = new NutrunnerInfo
                {
                    Id = eq.Id = index++,
                    Code = eq.EquipmentCode,
                    Ip = string.IsNullOrEmpty(eq.EquipmentIp) ? "0.0.0.0" : eq.EquipmentIp, 
                    Port = int.TryParse(eq.EquipmentPort, out var port) ? port : 5000,
                    EventPort = int.TryParse(eq.EquipmentPort, out var eventortp) ? eventortp + 1 : 5000 + 1,
                    Name = eq.EquipmentName,
                    ModelName = eq.EquipmentModel,
                };

                _equip.TryAdd(info);
            }

            _equip.Manager.ConnectionChanged += OnConnectionChanged;
            _equip.Manager.AlarmOccurred += OnErrorOccurred;
            _equip.Manager.RegisterDataReceived += OnRegisterDataReceived;
            _equip.Manager.FasteningReceived += OnFasteningReceived;
            _equip.Manager.StartAll();

            RefreshEquipmentStates();

        }

        private void OnFasteningReceived(object sender, NutrunnerFasteningEventArgs e)
        {
            InvokeIfRequired(() =>
            {
                FasteningData data = e.Data;

                AppLog.Info("────────────────────────");
                AppLog.Info($"[ID {e.Code}] Fastening");
                AppLog.Info($"[ToolId] : {data.ToolId}");
                AppLog.Info($"[FastenTime] : {data.FastenTime}");
                AppLog.Info($"[Preset] : {data.Preset}");
                AppLog.Info($"[TargetTorque] : {data.TargetTorqueValue:0.00}");
                AppLog.Info($"[Torque] : {data.TorqueValue:0.00}");
                AppLog.Info($"[Speed] : {data.Speed}");
                AppLog.Info($"[Angle1] : {data.Angle1}");
                AppLog.Info($"[Angle2] : {data.Angle2}");
                AppLog.Info($"[Angle3] : {data.Angle3}");
                AppLog.Info($"[RemainScrew] : {data.RemainScrew}");
                AppLog.Info($"[Error] : {data.Error}");
                AppLog.Info($"[Direction] : {data.Direction}");
                AppLog.Info($"[EventStatus] : {data.EventStatus}");
                AppLog.Info($"[SnugAngle] : {data.SnugAngle}");
                AppLog.Info($"[Barcode] : {data.Barcode}");

                
                ApplyTorqueFromEquipment(e.Code, (decimal)data.TorqueValue, (decimal)data.Angle3);

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

                RefreshEquipmentStates();

                foreach (ushort value in e.Data)
                {
                    AppLog.Warn($"[ID {e.Code}][{address++:D4}] : {value}");
                }

            });
        }

        private void OnConnectionChanged(object sender, EquipmentConnectionEventArgs e)
        {
            InvokeIfRequired(() =>
            {
                AppLog.Info($"[ID {e.Code}] {(e.IsConnected ? "연결 됨" : "연결 끊김")}");

                RefreshEquipmentStates();
                //foreach (var equipment in _equipments)
                //{
                //    INutrunner nutrunner;
                //    _equip.Manager.TryGet(e.Id, out nutrunner);

                //    if (nutrunner.IsConnected)
                //    {
                //        equipment.CommunicationState = Language.Pop_CommConnected;
                //        equipment.EquipmentState = Language.Pop_EquipmentUsable;
                //    }
                //    else
                //    {
                //        equipment.CommunicationState = Language.Pop_CommFailed;
                //        equipment.EquipmentState = Language.Pop_EquipmentUnusable;

                //    }
                //}

                //gridViewEquipment.RefreshData();
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
            _equip.Manager.StopAll();
            _equip.Manager.Dispose();
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
                _equip.Manager.TryGet(equipment.EquipmentCode, out nutrunner);
                if (nutrunner == null) continue;

                if (nutrunner.IsConnected && nutrunner.IsEventEnabled)
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

        /// <summary>
        /// 목표 토크(<paramref name="torqueTarget"/>)를 쓸 너트러너를 하나 골라 토크를 설정하고 잠금을 푼다.
        /// 나머지 연결된 너트러너는 잠가서, 작업자가 맞지 않는 공구로 체결하지 못하게 한다.
        ///
        ///  - 대상: 네트워크(Request 채널)에 연결된 장비만 — 끊긴 장비에는 어떤 값도 전달되지 않는다.
        ///  - 선택: 서버 기준정보의 가동범위(RangeLower ~ RangeUpper, 경계 포함) 안에 목표 토크가 드는 첫 장비
        ///          → Torque = 목표 토크, DriverLock = 0(해제)
        ///  - 그 외(범위 밖 · 범위가 비어 있음 · 두 번째 이후 범위 안 장비) → DriverLock = 3(잠금)
        /// </summary>
        /// <returns>선택된 장비코드. 범위에 맞는 연결 장비가 없으면 null(이때 연결된 장비는 전부 잠긴다).</returns>
        private string WriteTorqueToEquipment(decimal torqueTarget)
        {
            if (_equip?.Manager == null)
                return null;

            string selectedCode = null;

            foreach (var equipment in _equipments)
            {
                var code = equipment.EquipmentCode;

                // 연결 안 된 장비는 잠금도 보낼 수 없으므로 건너뛴다.
                if (!_equip.Manager.TryGet(code, out INutrunner nutrunner) || !nutrunner.IsConnected)
                    continue;

                if (selectedCode == null && IsInTorqueRange(equipment, torqueTarget))
                {
                    // 토크를 먼저 쓰고 잠금을 푼다 — 해제 직후 이전 토크로 체결되는 틈을 없앤다.
                    if (_equip.Manager.SetTorque(code, torqueTarget)
                        && _equip.Manager.SetDriverLock(code, false))
                    {
                        selectedCode = code;
                        AppLog.Info($"[POP] 토크 설정 · 잠금 해제 - {code} : {torqueTarget} " +
                                    $"(범위 {equipment.RangeLower} ~ {equipment.RangeUpper})");
                        continue;
                    }

                    // 쓰기 실패(레지스터 범위 초과 등) — 이 장비는 잠그고 다음 장비를 찾는다.
                    AppLog.Warn($"[POP] 토크 설정 실패 - {code} : {torqueTarget}");
                }

                _equip.Manager.SetDriverLock(code, true);
                AppLog.Info($"[POP] 너트러너 잠금 - {code} (범위 {equipment.RangeLower} ~ {equipment.RangeUpper}, 목표 {torqueTarget})");
            }

            if (selectedCode == null)
                AppLog.Warn($"[POP] 토크 설정 실패 - 목표 {torqueTarget} 를 받을 수 있는 연결된 장비가 없음");

            return selectedCode;
        }

        /// <summary>
        /// 서버가 내려준 장비 가동범위 안에 목표 토크가 드는지(경계 포함).
        /// 범위가 비어 있으면 어떤 토크를 감당하는지 알 수 없으므로 범위 밖으로 본다.
        /// </summary>
        private static bool IsInTorqueRange(PopEquipmentDto equipment, decimal torqueTarget)
        {
            if (equipment.RangeLower == null || equipment.RangeUpper == null)
                return false;

            return torqueTarget >= equipment.RangeLower.Value
                && torqueTarget <= equipment.RangeUpper.Value;
        }
    }
}
