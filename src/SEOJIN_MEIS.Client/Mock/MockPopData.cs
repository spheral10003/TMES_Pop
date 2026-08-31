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
        /// <summary>
        /// &lt;진행 중 공정 정보&gt; 목업.
        /// 화면에서 넘어온 키(작업지시/시리얼/라우팅/페이지 공정)를 그대로 되돌려 주고,
        /// 시간 값만 "4분 전에 시작해 계속 가동 중"인 상태로 만들어 준다.
        /// </summary>
        public static PopProcessInfoDto GetProcessInfo(PopProcessKey key)
        {
            var startedAt = DateTime.Now.AddMinutes(-4);

            var info = new PopProcessInfoDto
            {
                WorkOrderNo = key?.WorkOrderNo,
                ItemCode = "ITEM_01",
                SerialNo = key?.SerialNo,
                RoutingGroup = key?.RoutingGroup,
                RoutingOperation = key?.RoutingOperation,
                PageOperation = key?.PageOperation,
                PageStartTime = startedAt,
                PageEndTime = null,
                RunTime = TimeSpan.FromMinutes(4),
                IdleTime = TimeSpan.Zero,
                WorkTime = TimeSpan.FromMinutes(4),
                ProgressState = "공정 진행중",

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
