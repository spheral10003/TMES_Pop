using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Models
{
    public class WorkOrderQuery
    {
        /// <summary>Server 정보(필수).</summary>
        public string CorpCode { get; set; }

        /// <summary>지시일자 시작(부분 일치). 화면에서 기본값은 한 달 전.</summary>
        public DateTime? OrderDateFrom { get; set; }

        /// <summary>지시일자 종료(부분 일치). 화면에서 기본값은 오늘.</summary>
        public DateTime? OrderDateTo { get; set; }

        /// <summary>작업지시번호(부분 일치).</summary>
        public string WorkOrderNo { get; set; }

        /// <summary>작업지시계획번호(부분 일치).</summary>
        public string WorkPlanNo { get; set; }

        /// <summary>생산품목 코드.</summary>
        public string ItemCode { get; set; }
        /// <summary>생산품목명.</summary>
        public string ItemName { get; set; }

        /// <summary>생산품목 시리얼번호(부분 일치).</summary>
        public string SerialNo { get; set; }

        /// <summary>작업지시 상태. 비어 있거나 "전체"면 상태로 거르지 않는다.</summary>
        public string StatusName { get; set; }

        /// <summary>워크스테이션 참조 코드.</summary>
        public string WorkStationRef { get; set; }

        /// <summary>
        /// 작업지시 상태 목록(Ready / Progress / Completed) — statusList=Ready&amp;statusList=Progress처럼 반복해서 보낸다.
        /// POP 진입 화면은 [대기] · [진행중] · [완료] 중 체크한 것만 보낸다(WorkOrderEntryScreen).
        /// </summary>
        public List<string> StatusList { get; set; }

    }
}
