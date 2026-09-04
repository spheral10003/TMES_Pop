using System;
using System.Collections.Generic;
using SEOJIN_MEIS.Client.Common.Models;

namespace SEOJIN_MEIS.Client.Mock
{
    /// <summary>
    /// POP 본 화면(진행 중 공정 정보 + 하단 4개 탭)의 목업 데이터.
    /// 값은 화면 설계서(목업 이미지)의 샘플 행을 그대로 옮긴 것이며, 실제 업무 데이터가 아니다.
    ///
    /// App.config의 <c>UseMockData</c>가 true일 때만 사용된다.
    /// 서버 API가 확정되면 이 파일과 <c>UseMockData</c> 설정을 함께 걷어내면 된다.
    /// </summary>
    public static class MockPopData
    {
        // ─────────────────────────────────────────────────────────────────────────
        //  공정 진행 상태 (목업 전용)
        //
        //  실제 운영에서는 상태 전이도 시간 집계도 전부 서버가 한다.
        //  목업은 서버가 없으므로, 화면 흐름(시작 → 중지 → 재시작 → 생략)을 눈으로 확인할 수 있도록
        //  공정별 상태를 프로세스 메모리에만 들고 있는다. 앱을 껐다 켜면 다시 [작업 대기]로 돌아간다.
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>진행 상태 문구 — 화면 하단 띠(<c>lblProgressState</c>)에 그대로 찍힌다.</summary>
        public const string StateWaiting = "작업 대기";
        public const string StateRunning = "공정 진행중";
        public const string StateStopped = "공정 중지";
        public const string StateSkipped = "공정 생략";

        /// <summary>공정 하나의 진행 상태와 시간 누적치.</summary>
        private class ProcessState
        {
            public string ProgressState = StateWaiting;
            public DateTime? StartedAt;
            public DateTime? EndedAt;

            /// <summary>마지막 상태 전이 시각 — 여기서 지금까지 흐른 시간을 가동/비가동에 나눠 담는다.</summary>
            public DateTime? ChangedAt;

            public TimeSpan Run;
            public TimeSpan Idle;
        }

        private static readonly Dictionary<string, ProcessState> States =
            new Dictionary<string, ProcessState>(StringComparer.OrdinalIgnoreCase);

        /// <summary>공정 하나를 가리키는 키 문자열(작업지시/시리얼/라우팅그룹/라우팅공정/페이지공정).</summary>
        private static string StateKey(PopProcessKey key) =>
            string.Join("|", key?.WorkOrderNo, key?.SerialNo, key?.RoutingGroup, key?.RoutingOperation, key?.PageOperation);

        private static ProcessState StateOf(PopProcessKey key)
        {
            var id = StateKey(key);
            ProcessState state;
            if (!States.TryGetValue(id, out state))
            {
                state = new ProcessState();
                States[id] = state;
            }

            return state;
        }

        /// <summary>
        /// 상태를 바꾸면서, 직전 상태로 머문 시간을 가동시간/비가동시간에 나눠 담는다.
        /// (진행중이었으면 가동, 중지였으면 비가동. 아직 시작 전이면 어느 쪽에도 넣지 않는다.)
        /// </summary>
        private static void Accumulate(ProcessState state, DateTime now)
        {
            if (state.ChangedAt == null)
                return;

            var elapsed = now - state.ChangedAt.Value;
            if (elapsed < TimeSpan.Zero)
                elapsed = TimeSpan.Zero;

            if (state.ProgressState == StateRunning)
                state.Run += elapsed;
            else if (state.ProgressState == StateStopped)
                state.Idle += elapsed;
        }

        /// <summary>
        /// [공정 시작] · [공정 중지] · [공정 생략]을 목업 상태에 반영하고, 바뀐 공정 정보를 돌려준다.
        /// 전이 가능 여부를 따지지는 않는다 — 그 판정은 서버 몫이고, 목업은 화면 확인용이다.
        /// </summary>
        public static PopProcessInfoDto ApplyProcessAction(PopProcessKey key, string nextState)
        {
            var state = StateOf(key);
            var now = DateTime.Now;

            Accumulate(state, now);

            if (nextState == StateRunning)
            {
                // 생략/중지 뒤에 다시 시작할 수 있다. 첫 시작에만 시작시각을 남긴다.
                if (state.StartedAt == null)
                    state.StartedAt = now;
                state.EndedAt = null;
            }
            else if (nextState == StateSkipped)
            {
                state.EndedAt = now;
            }

            state.ProgressState = nextState;
            state.ChangedAt = now;

            return GetProcessInfo(key);
        }

        /// <summary>
        /// &lt;진행 중 공정 정보&gt; 목업.
        /// 화면에서 넘어온 키(작업지시/시리얼/라우팅/페이지 공정)를 그대로 되돌려 주고,
        /// 상태·시간은 <see cref="ApplyProcessAction"/>으로 쌓아 온 값에서 만든다.
        /// </summary>
        public static PopProcessInfoDto GetProcessInfo(PopProcessKey key)
        {
            var state = StateOf(key);
            var now = DateTime.Now;

            // 마지막 전이 이후 흐른 시간까지 더해서 보여 준다(화면을 열어 둔 채로도 시간이 흐르도록).
            var run = state.Run;
            var idle = state.Idle;
            if (state.ChangedAt != null)
            {
                var elapsed = now - state.ChangedAt.Value;
                if (elapsed > TimeSpan.Zero)
                {
                    if (state.ProgressState == StateRunning) run += elapsed;
                    else if (state.ProgressState == StateStopped) idle += elapsed;
                }
            }

            var info = new PopProcessInfoDto
            {
                WorkOrderNo = key?.WorkOrderNo,
                ItemCode = "ITEM_01",
                SerialNo = key?.SerialNo,
                RoutingGroup = key?.RoutingGroup,
                RoutingOperation = key?.RoutingOperation,
                PageOperation = key?.PageOperation,
                PageStartTime = state.StartedAt,
                PageEndTime = state.EndedAt,
                RunTime = run,
                IdleTime = idle,
                WorkTime = run + idle,
                ProgressState = state.ProgressState,

                // 서버가 내려줄 도면 경로 자리. 목업에서도 공유 폴더 루트만 설정하면
                // 실제 SMB 읽기 경로를 그대로 태워 볼 수 있도록 샘플 파일명을 넣어 둔다.
                DrawingFilePath = "ITEM_01_OP_10_0010.pdf"
            };

            return info;
        }

        /// <summary>[투입자재] 탭 목업 — 6품목. 앞 2건만 시리얼번호 검사 대상이다.</summary>
        public static List<PopInputMaterialDto> GetInputMaterials(PopProcessKey key)
        {
            return new List<PopInputMaterialDto>
            {
                Material("M-00001-01010", "PN-010101010-11", 1, 1, true, "SN-00001-01010"),
                Material("M-00001-02223", "PN-010101010-22", 3, 2, true),
                Material("M-00001-03312", "PN-010101010-33", 10, 3, false),
                Material("M-00001-04591", "PN-010101010-45", 12, 4, false),
                Material("M-00001-05881", "PN-010101010-58", 2, 5, false),
                Material("M-00001-06092", "PN-010101010-60", 1, 6, false)
            };
        }

        /// <summary>
        /// [BOM 스팩] 탭 목업 — 기준정보의 토크 작업 스펙 6건.
        /// 첫 행만 이미 측정된 상태(4.3)이고 나머지는 미측정이라, 자동 수신/수동 입력 두 경로를 모두 확인할 수 있다.
        /// </summary>
        public static List<PopBomSpecDto> GetBomSpecs(PopProcessKey key)
        {
            return new List<PopBomSpecDto>
            {
                BomSpec("M-00001-01010", "1", "토크작업01", 1.0m, 3.0m, 4.3m),
                BomSpec("M-00001-02223", "2", "토크작업02", 2.0m, 3.0m, null),
                BomSpec("M-00001-02223", "3", "토크작업02", 2.0m, 3.0m, null),
                BomSpec("M-00001-02223", "4", "토크작업02", 2.0m, 3.0m, null),
                BomSpec("M-00001-03312", "5", "토크작업03", 3.0m, 4.0m, null),
                BomSpec("M-00001-03312", "6", "토크작업03", 3.0m, 4.0m, null)
            };
        }

        /// <summary>[검사 스팩] 탭 목업 — 정량 1건 + 정성 1건. 결과는 작업자가 화면에서 입력한다.</summary>
        public static List<PopInspectionSpecDto> GetInspectionSpecs(PopProcessKey key)
        {
            return new List<PopInspectionSpecDto>
            {
                new PopInspectionSpecDto
                {
                    InspectionSpecName = "A 와이어 저항 확인",
                    InspectionRemark = "저항 값 범위 판정",
                    InspectionMethod = "정량",
                    ResultState = null,
                    ResultValue = null
                },
                new PopInspectionSpecDto
                {
                    InspectionSpecName = "그로멧 체결 위치 확인",
                    InspectionRemark = "그로멧이 제대로 부착되어 있는지 확인",
                    InspectionMethod = "정성",
                    ResultState = null,
                    ResultValue = null
                }
            };
        }

        /// <summary>
        /// [연결 장비] 탭 목업 — 워크스테이션에 물린 토크장비 2대(장비 기준정보만).
        /// 통신상태/장비상태는 화면이 실제 Modbus 연결 상태로 덮어쓰므로 여기서는 초기값만 채운다.
        /// </summary>
        public static List<PopEquipmentDto> GetWorkstationEquipments(string workstation)
        {
            return new List<PopEquipmentDto>
            {
                new PopEquipmentDto
                {
                    EquipmentCode = "EQ-001",
                    EquipmentName = "TORQUE_01",
                    EquipmentRemark = "토크공구 1번 라인",
                    EquipmentType = "토크장비",
                    CommunicationState = "연결 실패",
                    EquipmentState = "사용불가",
                    RangeLower = 1.2m,
                    RangeUpper = 4.3m
                },
                new PopEquipmentDto
                {
                    EquipmentCode = "EQ-002",
                    EquipmentName = "TORQUE_02",
                    EquipmentRemark = "토크공구 2번 라인",
                    EquipmentType = "토크장비",
                    CommunicationState = "연결 실패",
                    EquipmentState = "사용불가",
                    RangeLower = 4.8m,
                    RangeUpper = 5.5m
                }
            };
        }

        /// <summary>
        /// &lt;관리자 알람 유형 선택&gt; 팝업의 알람 유형 목업(기준정보).
        /// 대상이 필요한 유형(자재 결품 / 설비 이상)과 비고만 받는 유형을 함께 넣어 두 경로를 모두 확인할 수 있다.
        /// </summary>
        public static List<AdminAlarmTypeDto> GetAdminAlarmTypes()
        {
            return new List<AdminAlarmTypeDto>
            {
                new AdminAlarmTypeDto
                {
                    Code = "MATERIAL_SHORTAGE",
                    Name = "자재 결품",
                    TargetKind = AdminAlarmTargetKind.Material,
                    TargetCaption = "결품 자재 코드"
                },
                new AdminAlarmTypeDto
                {
                    Code = "EQUIPMENT_FAULT",
                    Name = "설비 이상",
                    TargetKind = AdminAlarmTargetKind.Equipment,
                    TargetCaption = "이상 장비 코드"
                },
                new AdminAlarmTypeDto
                {
                    Code = "QUALITY_ISSUE",
                    Name = "품질 이상",
                    TargetKind = AdminAlarmTargetKind.None
                },
                new AdminAlarmTypeDto
                {
                    Code = "ETC",
                    Name = "기타",
                    TargetKind = AdminAlarmTargetKind.None
                }
            };
        }

        private static PopInputMaterialDto Material(
            string materialCode, string itemCode, int qty, int position, bool serialCheck, params string[] serials)
        {
            var material = new PopInputMaterialDto
            {
                MaterialCode = materialCode,
                ItemCode = itemCode,
                InputQty = qty,
                InputUnit = "EA",
                InputPosition = position.ToString(),
                SerialCheckRequired = serialCheck
            };

            material.SerialNumbers.AddRange(serials);
            return material;
        }

        private static PopBomSpecDto BomSpec(
            string materialCode, string position, string specName, decimal lower, decimal upper, decimal? result)
        {
            return new PopBomSpecDto
            {
                MaterialCode = materialCode,
                InputPosition = position,
                BomSpecName = specName,
                SpecType = "토크 작업",
                TorqueLower = lower,
                TorqueUpper = upper,
                TorqueUnit = "lb",
                TorqueResult = result
            };
        }
    }
}
