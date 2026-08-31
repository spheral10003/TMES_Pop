using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SEOJIN_MEIS.Client.Common.Logging
{
    /// <summary>
    /// 앱 처리 단계를 시간순으로 남기는 초경량 로거.
    ///
    /// 같은 줄을 세 곳에 동시에 남긴다:
    ///  - Console : 디버그 실행 시 콘솔 창
    ///  - Trace   : Visual Studio 출력 창
    ///  - 파일    : 하루 한 개(<c>meis-client-yyyyMMdd.log</c>) — 현장 PC에서 나중에 원인을 볼 수 있어야 하므로
    ///
    /// 파일은 <b>첫 기록 때 자동으로 열리며</b> 별도 초기화 호출이 필요 없다(앱 시작 첫 줄부터 남는다).
    /// 위치는 <see cref="LogFolder"/>, 지금 쓰는 파일은 <see cref="CurrentFilePath"/>로 확인한다.
    ///
    /// 파일 관리 정책(파일을 새로 열 때 — 앱 시작 시와 자정을 넘길 때 — 한 번씩 정리한다):
    ///  - 일별 생성 : logs\meis-client-yyyyMMdd.log
    ///  - 7일 압축 : 7일이 지난 로그는 7일 단위로 묶어 logs\archive\meis-client-{시작}_{끝}.zip 으로 백업하고 원본은 지운다
    ///  - 30일 보관 : 압축본·원본 모두 30일이 지나면 지운다
    ///
    /// 로그 때문에 앱이 죽는 일은 없어야 하므로 파일 쪽 실패는 전부 삼키고
    /// (권한 없음/디스크 가득 등) 파일 기록만 끈 채 콘솔·Trace는 계속 남긴다.
    /// </summary>
    public static class AppLog
    {
        private static readonly object SyncRoot = new object();

        /// <summary>보관 기간(일). 이보다 오래된 로그는 압축본까지 지운다.</summary>
        private const int RetentionDays = 30;

        /// <summary>압축 묶음의 크기(일) 겸 압축 대상이 되는 나이(일). 7일이 지난 로그부터 7일씩 묶는다.</summary>
        private const int ArchiveBucketDays = 7;

        /// <summary>
        /// 7일 묶음의 기준점. 앱 실행 이력과 무관하게 항상 같은 경계로 묶이도록 고정 날짜를 쓴다
        /// (마지막 압축 시각을 어딘가에 저장해 두는 방식은 그 상태 파일이 사라지면 묶음이 어긋난다).
        /// </summary>
        private static readonly DateTime BucketEpoch = new DateTime(2000, 1, 1);

        private const string ArchiveFolderName = "archive";
        private const string FilePrefix = "meis-client-";
        private const string FileExtension = ".log";
        private const string ArchiveExtension = ".zip";
        private const string DatePattern = "yyyyMMdd";

        private static StreamWriter _writer;

        /// <summary>지금 열려 있는 파일의 날짜. 자정을 넘기면 다음 줄에서 새 파일로 넘어간다.</summary>
        private static DateTime _writerDate;

        /// <summary>파일에 쓸 수 없는 환경으로 판명된 경우. 이후로는 콘솔·Trace에만 남긴다.</summary>
        private static bool _fileLogDisabled;

        /// <summary>로그 폴더(첫 기록 때 결정된다). 파일 로깅을 못 하면 null.</summary>
        public static string LogFolder { get; private set; }

        /// <summary>지금 기록 중인 로그 파일의 전체 경로. 파일 로깅을 못 하면 null.</summary>
        public static string CurrentFilePath { get; private set; }

        /// <summary>앱 처리 흐름의 한 단계(로그인 시도, 화면 전환 등)를 기록한다.</summary>
        public static void Step(string message) => Write("STEP", message);

        public static void Info(string message) => Write("INFO", message);

        public static void Warn(string message) => Write("WARN", message);

        public static void Error(string message, Exception ex = null) =>
            Write("ERROR", ex == null ? message : $"{message} :: {ex}");

        /// <summary>앱을 끝낼 때 파일을 닫는다(호출하지 않아도 각 줄은 이미 디스크에 있다 — AutoFlush).</summary>
        public static void Close()
        {
            lock (SyncRoot)
                CloseWriter();
        }

        private static void Write(string level, string message)
        {
            var now = DateTime.Now;
            var body = $"[{level,-5}] {message}";

            lock (SyncRoot)
            {
                // 콘솔/출력 창은 실행 중에만 보므로 시각(시:분:초)이면 충분하고,
                // 파일은 며칠 뒤에 열어 보므로 날짜까지 남긴다.
                var line = $"[{now:HH:mm:ss.fff}] {body}";
                Console.WriteLine(line);
                Trace.WriteLine(line);

                WriteToFile(now, $"[{now:yyyy-MM-dd HH:mm:ss.fff}] {body}");
            }
        }

        /// <summary>파일 한 줄. 어떤 실패도 밖으로 던지지 않는다 — 로그가 앱을 죽이면 안 된다.</summary>
        private static void WriteToFile(DateTime now, string line)
        {
            if (_fileLogDisabled)
                return;

            try
            {
                EnsureWriter(now.Date);
                _writer.WriteLine(line);
            }
            catch (Exception ex)
            {
                // 권한 없음/디스크 가득 등. 매 줄 실패를 반복하지 않도록 파일 기록만 끄고 계속 간다.
                _fileLogDisabled = true;
                CloseWriter();
                CurrentFilePath = null;

                var notice = $"[{now:HH:mm:ss.fff}] [WARN ] [로그] 파일 기록을 중단한다(콘솔/출력 창은 계속 남긴다): {ex.Message}";
                Console.WriteLine(notice);
                Trace.WriteLine(notice);
            }
        }

        /// <summary>그날 파일이 열려 있게 만든다. 날짜가 바뀌면 새 파일로 갈아탄다.</summary>
        private static void EnsureWriter(DateTime date)
        {
            if (_writer != null && _writerDate == date)
                return;

            CloseWriter();

            if (LogFolder == null)
                LogFolder = ResolveLogFolder();

            Directory.CreateDirectory(LogFolder);

            var path = Path.Combine(LogFolder, FilePrefix + date.ToString(DatePattern, CultureInfo.InvariantCulture) + FileExtension);

            // FileShare.ReadWrite : 앱이 켜져 있는 동안에도 메모장/탐색기로 로그를 열어 볼 수 있게 한다.
            // UTF-8 BOM       : 메모장에서 한글이 깨지지 않게(이어 쓰는 경우 StreamWriter가 BOM을 다시 넣지 않는다).
            // AutoFlush       : 앱이 강제 종료돼도 마지막 줄까지 파일에 남는다.
            var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(stream, new UTF8Encoding(true));
            _writer.AutoFlush = true;

            _writerDate = date;
            CurrentFilePath = path;

            RunMaintenance(date);
        }

        /// <summary>
        /// 파일 관리 한 바퀴 — 압축 백업 → 보관 기간 정리 순서로 돈다.
        /// 파일을 새로 열 때만(앱 시작 / 자정 넘김) 실행되므로 매 줄의 비용이 되지는 않는다.
        /// 정리가 실패해도 로깅 자체는 계속해야 하므로 여기서 나는 예외는 Trace로만 남긴다
        /// (AppLog로 남기면 이 메서드를 다시 부르는 꼴이 된다).
        /// </summary>
        private static void RunMaintenance(DateTime today)
        {
            ArchiveExpiredLogs(today);
            PurgeExpiredArchives(today);
            PurgeExpiredLogs(today);
        }

        /// <summary>
        /// 7일이 지난 일별 로그를 7일 단위로 묶어 <c>archive</c> 폴더에 zip으로 백업하고 원본을 지운다.
        /// 이미 있는 zip에는 이어 넣는다(앱이 며칠 꺼져 있다가 켜져도 같은 묶음에 모인다).
        /// </summary>
        private static void ArchiveExpiredLogs(DateTime today)
        {
            try
            {
                var archiveFolder = Path.Combine(LogFolder, ArchiveFolderName);

                // 오늘 쓰고 있는 파일과 아직 7일이 안 된 파일은 그대로 둔다.
                var limit = today.AddDays(-ArchiveBucketDays);

                var targets = DatedFiles(LogFolder, FileExtension)
                    .Where(item => item.Key < limit)
                    .GroupBy(item => BucketStart(item.Key));

                foreach (var bucket in targets)
                {
                    Directory.CreateDirectory(archiveFolder);

                    var zipPath = Path.Combine(archiveFolder,
                        FilePrefix + bucket.Key.ToString(DatePattern, CultureInfo.InvariantCulture)
                                   + "_" + bucket.Key.AddDays(ArchiveBucketDays - 1).ToString(DatePattern, CultureInfo.InvariantCulture)
                                   + ArchiveExtension);

                    ArchiveBucket(zipPath, bucket.ToList());
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[로그] 오래된 로그 압축 실패: {ex.Message}");
            }
        }

        /// <summary>묶음 하나를 zip에 넣고 원본을 지운다. 한 파일이 실패해도 나머지는 계속 처리한다.</summary>
        private static void ArchiveBucket(string zipPath, List<KeyValuePair<DateTime, string>> files)
        {
            var mode = File.Exists(zipPath) ? ZipArchiveMode.Update : ZipArchiveMode.Create;

            using (var zip = ZipFile.Open(zipPath, mode))
            {
                foreach (var file in files)
                {
                    try
                    {
                        var entryName = Path.GetFileName(file.Value);

                        // 앞선 실행에서 넣다가 원본 삭제만 실패했을 수 있다 — 같은 이름을 두 번 넣지 않는다.
                        if (mode == ZipArchiveMode.Update && zip.GetEntry(entryName) != null)
                        {
                            File.Delete(file.Value);
                            continue;
                        }

                        var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                        entry.LastWriteTime = File.GetLastWriteTime(file.Value);

                        // 원본은 FileShare.ReadWrite로 연다 — 다른 곳에서 열어 보고 있어도 백업은 진행한다.
                        using (var source = new FileStream(file.Value, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var target = entry.Open())
                            source.CopyTo(target);

                        File.Delete(file.Value);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"[로그] 압축 대상 처리 실패({Path.GetFileName(file.Value)}): {ex.Message}");
                    }
                }
            }
        }

        /// <summary>보관 기간이 지난 압축본 정리(묶음의 마지막 날짜 기준).</summary>
        private static void PurgeExpiredArchives(DateTime today)
        {
            try
            {
                var archiveFolder = Path.Combine(LogFolder, ArchiveFolderName);
                if (!Directory.Exists(archiveFolder))
                    return;

                var limit = today.AddDays(-RetentionDays);

                foreach (var file in Directory.GetFiles(archiveFolder, FilePrefix + "*" + ArchiveExtension))
                {
                    // meis-client-{시작}_{끝}.zip — 묶음의 끝 날짜가 보관 기간을 넘겼을 때만 지운다.
                    var name = Path.GetFileNameWithoutExtension(file);
                    if (name == null)
                        continue;

                    var separator = name.LastIndexOf('_');
                    if (separator < 0)
                        continue;

                    DateTime bucketEnd;
                    if (!DateTime.TryParseExact(name.Substring(separator + 1), DatePattern,
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out bucketEnd))
                        continue;

                    if (bucketEnd < limit)
                        File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[로그] 오래된 압축본 정리 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 보관 기간이 지난 일별 로그 정리.
        /// 정상 흐름이라면 7일째에 이미 압축돼 없어졌을 파일들이다 —
        /// 압축이 계속 실패하는 환경에서 파일이 무한정 쌓이지 않게 하는 마지막 정리다.
        /// </summary>
        private static void PurgeExpiredLogs(DateTime today)
        {
            try
            {
                var limit = today.AddDays(-RetentionDays);

                foreach (var file in DatedFiles(LogFolder, FileExtension).Where(item => item.Key < limit))
                    File.Delete(file.Value);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[로그] 오래된 로그 파일 정리 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 폴더에서 우리가 만든 이름 형식(<c>meis-client-yyyyMMdd</c>)의 파일만 날짜와 함께 돌려준다.
        /// 형식이 다른 파일은 우리 것이 아니므로 건드리지 않는다.
        /// </summary>
        private static IEnumerable<KeyValuePair<DateTime, string>> DatedFiles(string folder, string extension)
        {
            foreach (var file in Directory.GetFiles(folder, FilePrefix + "*" + extension))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (name == null || name.Length <= FilePrefix.Length)
                    continue;

                DateTime date;
                if (!DateTime.TryParseExact(name.Substring(FilePrefix.Length), DatePattern,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    continue;

                yield return new KeyValuePair<DateTime, string>(date, file);
            }
        }

        /// <summary>날짜가 속한 7일 묶음의 첫날.</summary>
        private static DateTime BucketStart(DateTime date)
        {
            var index = (int)Math.Floor((date - BucketEpoch).TotalDays / ArchiveBucketDays);
            return BucketEpoch.AddDays(index * ArchiveBucketDays);
        }

        private static void CloseWriter()
        {
            if (_writer == null)
                return;

            try
            {
                _writer.Dispose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[로그] 파일 닫기 실패: {ex.Message}");
            }

            _writer = null;
        }

        /// <summary>
        /// 로그 폴더를 정한다.
        ///  1) 실행 파일 옆 <c>logs</c> — 현장 PC에서 지원 인력이 바로 찾을 수 있는 자리
        ///  2) <c>%LOCALAPPDATA%\MEIS\logs</c> — 1)에 쓸 수 없을 때(Program Files 배포 등)
        /// </summary>
        private static string ResolveLogFolder()
        {
            var beside = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (IsWritable(beside))
                return beside;

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MEIS",
                "logs");
        }

        /// <summary>실제로 파일을 하나 써 보고 지운다 — 폴더 존재 여부만으로는 쓰기 권한을 알 수 없다.</summary>
        private static bool IsWritable(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);

                var probe = Path.Combine(folder, ".writetest");
                File.WriteAllText(probe, string.Empty);
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
