namespace TraceDiag;

/// <summary>All user-facing and log text used by the application.</summary>
internal static class Strings
{
    public static class App
    {
        public const string Title = "TraceDiag - Traceroute Diagnostic";
        public const string DataFolder = "TraceDiag";
        public const string ConfigFile = "settings.json";
        public const string UiStateFile = "ui-state.json";
        public const string ResetArgument = "reset";
        public const string LogsFolder = "logs";
        public const string LogFileNameFormat = "tracediag-{0:yyyyMMdd}.log";
        public const string RotatedLogFileNameFormat = "{0}-{1}{2}";
        public const string ConsoleTimestampFormat = "HH:mm:ss";
        public const string LogTimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
        public const string LogLinePrefixFormat = "[{0}] [{1}] ";
        public const string TimestampedMessageFormat = "[{0}] {1}";
        public const string ConsoleLogFileFormat = "[{0}] Log file: {1}";
        public const string ConsoleConfigFileFormat = "[{0}] Config: {1}";
        public const string ConsoleUiStateFileFormat = "[{0}] UI state: {1}";
        public const string NetworkIconFile1 = "ncpa.cpl";
        public const string NetworkIconFile2 = "netshell.dll";
        public const string NetworkIconFile3 = "netcenter.dll";
        public const string IconResourceName = "TraceDiag.Resources.cable_24dp.ico";
    }

    public static class Ui
    {
        public const string RemoteHostLabel = "Remote host (IP or domain):";
        public const string PacketSizeLabel = "Packet max size (64-65534):";
        public const string AttemptsLabel = "Количество попыток (1-1000):";
        public const string LossDelayLabel = "Задержка при потере, мс:";
        public const string FontSizeLabel = "Размер шрифта:";
        public const string DynamicPacketSizeLabel = "Динамический размер пакета";
        public const string DarkThemeLabel = "Тёмная тема";
        public const string MonitoringModeLabel = "Режим постоянного мониторинга";
        public const string RunButton = "Run";
        public const string StopButton = "Stop";
        public const string Stopping = "Stopping...";
        public const string Ready = "Ready";
        public const string ResolvingHost = "Resolving host...";
        public const string Running = "Running...";
        public const string Monitoring = "Мониторинг...";
        public const string Completed = "Completed";
        public const string MonitoringCompleted = "Мониторинг остановлен";
        public const string Cancelled = "Cancelled";
        public const string ResolveError = "Resolve error";
        public const string Error = "Error";
        public const string ValidationTitle = "Validation";
        public const string RemoteHostRequired = "Remote host is required (IP or domain name).";
        public const string LogFontFamily = "Consolas";
        public const string GridFontFamily = "Segoe UI";
        public const string NoDns = "(no DNS)";
        public const string NoAnswer = "нет ответа";
        public const string Dash = "-";
        public const string HostLabelFormat = "{0} [{1}]";
        public const string AvgPingFormat = "F1";

        public const string ColumnKeyHostName = "HostName";
        public const string ColumnKeyIp = "Ip";
        public const string ColumnKeyAvgPing = "AvgPing";
        public const string ColumnKeyMinPing = "MinPing";
        public const string ColumnKeyMaxPing = "MaxPing";
        public const string ColumnKeyLastPing = "LastPing";
        public const string ColumnKeySent = "Sent";
        public const string ColumnKeyReceived = "Received";
        public const string ColumnKeyStatus = "Status";
        public const string ColumnKeyDiagnosis = "Diagnosis";

        public const string ColumnHostName = "Имя узла";
        public const string ColumnIp = "IP";
        public const string ColumnAvgPing = "Пинг среднее, мс";
        public const string ColumnMinPing = "Мин пинг, мс";
        public const string ColumnMaxPing = "Макс пинг, мс";
        public const string ColumnLastPing = "Последний пинг, мс";
        public const string ColumnSent = "Отправлено";
        public const string ColumnReceived = "Получено";
        public const string ColumnStatus = "Состояние";
        public const string ColumnDiagnosis = "Диагноз";
    }

    public static class Diagnosis
    {
        public const string None = "—";
        public const string IcmpDisabled = "ICMP отключён";
        public const string PacketSizeLimit = "Обрезка размера ICMP";
        public const string AntiFlood = "Антифлуд ICMP";
    }

    public static class Status
    {
        public const string Pending = "ожидание";
        public const string NoAnswer = "не отвечает";
        public const string Normal = "норма";
        public const string Target = "цель";
        public const string Responds = "отвечает";
        public const string PacketLossFormat = "потеря пакетов ({0:F0}%)";
    }

    public static class LogLevel
    {
        public const string Info = "INFO";
        public const string Warn = "WARN";
        public const string Error = "ERROR";
    }

    public static class Log
    {
        public const string ApplicationStarted = "Application started.";
        public const string SettingsSavedFormat = "Settings saved: OutputFontSize={0:0.0}";
        public const string ThemeChangedFormat = "Settings saved: Theme={0}";
        public const string UiStateLoaded = "UI state loaded.";
        public const string UiStateReset = "UI state reset by launch argument.";
        public const string UiStateCorruptedReset = "UI state file was invalid and has been removed.";
        public const string UiStateSaved = "UI state saved.";
        public const string UiStateSaveFailed = "UI state could not be saved.";
        public const string RunRequestedFormat = "Run requested: host='{0}', maxPacketSize={1}, attempts={2}, dynamic={3}, lossDelayMs={4}";
        public const string MonitorRequestedFormat = "Monitoring requested: host='{0}', packetSize={1}, lossDelayMs={2}";
        public const string RunCompleted = "Run completed.";
        public const string MonitorCompleted = "Monitoring completed.";
        public const string LogRotated = "Log file rotated.";
        public const string RunCancelled = "Run cancelled by user.";
        public const string DnsNetworkError = "DNS/Network error during run.";
        public const string UnhandledError = "Unhandled error during run.";
        public const string FailedToResolveFormat = "Failed to resolve host: {0}";
        public const string ConsoleResolvedFormat = "[{0}] Resolved: {1} -> {2}";
        public const string ConsoleOperationCancelled = "Operation cancelled.";
        public const string ConsoleDnsNetworkErrorFormat = "[{0}] DNS/Network error: {1}";
        public const string ConsoleErrorFormat = "[{0}] Error: {1}: {2}";
        public const string ConsoleFailedResolveFormat = "[{0}] {1}";
    }

    public static class Diag
    {
        public const int SeparatorLength = 72;
        public const char SeparatorChar = '-';
        public const string PacketSizeListSeparator = ", ";
        public const string Dash = "-";
        public const string NoAnswer = "нет ответа";
        public const string NoDns = "no-dns";
        public const string MsFormat = "{0:F1} ms";
        public const string HopFormat = "{0} ({1})";
        public const string NameSuffixFormat = " [{0}]";

        public const string RunStarted = "Diagnostic run started.";
        public const string LogFileFormat = "Log file: {0}";
        public const string TargetFormat = "Target: {0} ({1})";
        public const string MaxPacketSizeFormat = "Max packet size: {0} bytes";
        public const string AttemptsFormat = "Attempts: {0}";
        public const string LossDelayFormat = "ICMP loss delay: {0} ms";
        public const string DynamicEnabled = "Dynamic packet size: enabled (from 64 bytes)";
        public const string DynamicDisabled = "Dynamic packet size: disabled (fixed max size)";
        public const string PacketSizesFormat = "Packet sizes (x2): {0}";
        public const string PacketSizeFixedFormat = "Packet size (fixed): {0} bytes";
        public const string NoRouteHops = "Status: not response (no route hops discovered)";
        public const string PacketSizeHeaderFormat = "[Packet size {0} bytes]";
        public const string SkipHopFormat = "  hop {0,2} {1}: пропуск (100% потерь, ICMP запрещён или ограничение размера).";
        public const string RunFinished = "Diagnostic run finished.";
        public const string RunCancelled = "Diagnostic run cancelled.";
        public const string MonitorStarted = "Continuous monitoring started.";
        public const string MonitorStopped = "Continuous monitoring stopped.";
        public const string MonitorCancelled = "Continuous monitoring cancelled.";
        public const string MonitorContextFormat = "monitor ttl={0} addr={1}";
        public const string MonitorCycleFormat = "  monitor hop {0,2} {1}: size={2}, rtt={3}, sent={4}, recv={5}";
        public const string MonitorExcludedFormat = "  hop {0,2} {1}: excluded from monitoring ({2}).";
        public const string RunFailed = "Diagnostic run failed.";
        public const string FatalErrorFormat = "Fatal error: {0}: {1}";

        public const string TracingRoute = "Tracing route (TTL increment)...";
        public const string DiscoveryRoundFormat = "Discovery round {0}";
        public const string TraceContextFormat = "trace ttl={0} attempt={1}/{2}";
        public const string TraceRoundContextFormat = "trace ttl={0} round={1}";
        public const string NoResponseHopFormat = "  {0,2}: * (no response)";
        public const string TargetMarker = " (target)";
        public const string HopDiscoveredFormat = "  {0,2}: {1}{2}";
        public const string TraceHopLimit = "Trace did not reach target within hop limit.";
        public const string TraceRoundLimit = "Trace did not reach target within discovery round limit.";

        public const string ResolvingDns = "Resolving DNS names for hops...";
        public const string DnsResolveFailedFormat = "DNS resolve failed for {0}";
        public const string DnsErrorFormat = "  hop {0,2} {1} -> DNS error: {2}";
        public const string SlowDnsFormat = "Slow DNS resolve for {0}: {1} ms";
        public const string NoPtrName = "(no PTR/DNS name)";
        public const string HopDnsFormat = "  hop {0,2} {1} -> {2}";

        public const string ProbingFormat = "  hop {0,2} {1}{2}: probing size={3}, attempts={4}...";
        public const string ProbeContextFormat = "hop={0} addr={1} size={2} attempt={3}/{4}";
        public const string ProbeErrorFormat = "Probe error at hop {0} ({1}), size={2}, attempt={3}";
        public const string ProbeErrorConsoleFormat = "    ! probe error at attempt {0}/{1}: {2}";
        public const string ProgressFormat = "    ... progress {0}/{1}, last={2} ms, avg={3:F1} ms, min={4} ms, max={5} ms, recv={6}, lost={7}";
        public const string StopWarningFirstFormat = "hop {0} ({1}) size={2}: 100% loss after {3} pings. Possible ICMP echo ban or packet size restriction; further checks for this node are skipped.";
        public const string StopWarningAllFormat = "hop {0} ({1}) size={2}: 100% loss ({3}/{3}). Possible ICMP echo ban or packet size restriction; further checks for this node are skipped.";
        public const string ConsoleWarningFormat = "  ! {0}";
        public const string HopSummaryFormat = "  hop {0,2} {1}{2}: size={3}, sent={4}, recv={5}, lost={6} ({7:F0}%), avg={8}";
        public const string DiagnosisFormat = "  hop {0,2} {1}: диагноз — {2}";

        public const string RemoteNotResponse = "Status: remote host not response";
        public const string RemoteResponds = "Status: remote host responds";
        public const string NoLossDetected = "Status: no packet loss detected on discovered hops.";
        public const string Summary = "Summary:";
        public const string SummaryRowFormat = "  hop {0,2} {1} - sent={2}, recv={3}, lost={4} ({5:F1}%)";

        public const string WatchdogFormat = "Watchdog timeout/hang detected ({0}); treating probe as timed out.";
        public const string ProbeSendFailedFormat = "Probe send failed ({0})";
        public const string ProbeSendFailedConsoleFormat = "    ! Probe send failed ({0}): {1}";
        public const string LogEnabledHint = "enabled";
    }
}
