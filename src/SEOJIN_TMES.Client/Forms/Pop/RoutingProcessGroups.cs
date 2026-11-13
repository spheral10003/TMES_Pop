using System;
using System.Collections.Generic;
using SEOJIN_TMES.Client.Common.Models;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// 서버가 내려준 페이지 공정 목록(순번 순)을 라우팅 공정 단위로 묶는다.
    /// 상태 값은 서버가 준 그대로 쓰고, 여기서는 "어느 줄을 보여 줄지"만 고른다.
    ///
    /// 묶는 기준은 목록 순서에서 라우팅 그룹 · 라우팅 공정이 바뀌는 지점이다 —
    /// 같은 라우팅 공정이 떨어져 다시 나오면 다른 줄로 본다(순번대로 진행하는 흐름을 그대로 보여 주려는 것).
    /// </summary>
    internal static class RoutingProcessGroups
    {
        private const string StateReady = "Ready";
        private const string StateProgress = "Progress";
        private const string StateStopped = "Stopped";

        /// <summary>목록을 라우팅 공정이 바뀔 때마다 끊어 묶음 목록으로 만든다. 순서는 유지한다.</summary>
        public static List<List<RoutingProcessDto>> Split(IList<RoutingProcessDto> processes)
        {
            var groups = new List<List<RoutingProcessDto>>();
            if (processes == null)
                return groups;

            List<RoutingProcessDto> current = null;
            foreach (var process in processes)
            {
                if (process == null)
                    continue;

                if (current == null || !IsSameRouting(current[current.Count - 1], process))
                {
                    current = new List<RoutingProcessDto>();
                    groups.Add(current);
                }

                current.Add(process);
            }

            return groups;
        }

        /// <summary>
        /// &lt;작업 진입 확인&gt;에 한 줄로 보여 줄 대표 페이지 공정 — 라우팅 공정마다 하나.
        /// 우선순위: 대기(Ready) 중 가장 앞 → 작업 중(Progress) 중 가장 앞 → 중지(Stopped) 중 가장 앞 → 마지막 페이지.
        /// 다 완료된 라우팅 공정은 마지막 페이지가 된다.
        /// </summary>
        public static List<RoutingProcessDto> Representatives(IList<RoutingProcessDto> processes)
        {
            var result = new List<RoutingProcessDto>();
            foreach (var group in Split(processes))
                result.Add(PickRepresentative(group));
            return result;
        }

        /// <summary>
        /// <see cref="Representatives"/> 목록에서 <paramref name="subRoutingRef"/> 페이지가 속한 줄의 위치. 목록에 없으면 -1.
        /// &lt;작업 진입 확인&gt;이 특정 페이지의 라우팅 공정 줄을 처음부터 가리키게 할 때 쓴다.
        /// </summary>
        public static int RepresentativeIndexOf(IList<RoutingProcessDto> processes, string subRoutingRef)
        {
            var groups = Split(processes);
            for (int i = 0; i < groups.Count; i++)
            {
                foreach (var page in groups[i])
                {
                    if (string.Equals(page.SubRoutingReference, subRoutingRef, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// <paramref name="subRoutingRef"/> 페이지가 속한 라우팅 그룹의 페이지 공정 전체(그 그룹의 모든 라우팅 공정, 순서 유지).
        /// 서브 라우팅 키로 찾는다 — 목록을 새로 받아 객체가 바뀌어도 찾도록. 목록에 없으면 빈 목록.
        /// </summary>
        public static List<RoutingProcessDto> PagesInRoutingGroup(IList<RoutingProcessDto> processes, string subRoutingRef)
        {
            var result = new List<RoutingProcessDto>();
            if (processes == null)
                return result;

            RoutingProcessDto current = null;
            foreach (var page in processes)
            {
                if (page != null && string.Equals(page.SubRoutingReference, subRoutingRef, StringComparison.OrdinalIgnoreCase))
                {
                    current = page;
                    break;
                }
            }

            if (current == null)
                return result;

            foreach (var page in processes)
            {
                if (page != null && IsSameRoutingGroup(page, current))
                    result.Add(page);
            }

            return result;
        }

        /// <summary>
        /// 라우팅 그룹 비교 — 키(RoutingGroupRef)가 있으면 키로, 없으면 이름으로 본다.
        /// POP의 공정 이동([이전 공정] · [다음 공정] · 자동 진행)도 이 기준으로 그룹 경계를 넘지 않게 막는다.
        /// </summary>
        public static bool IsSameRoutingGroup(RoutingProcessDto a, RoutingProcessDto b)
        {
            if (a == null || b == null)
                return false;

            if (!string.IsNullOrEmpty(a.RoutingGroupRef) || !string.IsNullOrEmpty(b.RoutingGroupRef))
                return string.Equals(a.RoutingGroupRef, b.RoutingGroupRef, StringComparison.OrdinalIgnoreCase);

            return string.Equals(a.RoutingGroup, b.RoutingGroup, StringComparison.Ordinal);
        }

        private static RoutingProcessDto PickRepresentative(List<RoutingProcessDto> group)
        {
            return FirstInState(group, StateReady)
                   ?? FirstInState(group, StateProgress)
                   ?? FirstInState(group, StateStopped)
                   ?? group[group.Count - 1];
        }

        private static RoutingProcessDto FirstInState(List<RoutingProcessDto> group, string state)
        {
            foreach (var page in group)
            {
                // 서버 코드의 대소문자 · 공백이 달라도 같은 상태로 본다(Language.ProgressStateText와 같은 기준).
                if (string.Equals(page.ProgressState?.Trim(), state, StringComparison.OrdinalIgnoreCase))
                    return page;
            }

            return null;
        }

        private static bool IsSameRouting(RoutingProcessDto a, RoutingProcessDto b)
        {
            return string.Equals(a.RoutingGroup, b.RoutingGroup, StringComparison.Ordinal)
                   && string.Equals(a.RoutingName, b.RoutingName, StringComparison.Ordinal);
        }
    }
}
