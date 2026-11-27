using TraceDiag.Localization;

namespace TraceDiag;

/// <summary>All user-facing and log text used by the application.</summary>
internal static partial class Strings
{
    /// <summary>Translates canonical text into the selected interface language.</summary>
    internal static string T(string english) => Translator.Text(english);

    public static class Console
    {
        public const string Usage = "Usage: TraceDiag.Console <run|monitor|reset> [options]";
        public const string HelpRun = "  run     finite diagnostic check";
        public const string HelpMonitor = "  monitor continuous monitoring (Ctrl+C to stop)";
        public const string HelpReset = "  reset   remove saved UI state";
        public const string OptionsTitle = "Options:";
        public const string OptionLineFormat = "  {0}";
        public const string OptionHost = "--host <ip|domain>";
        public const string OptionPacketSize = "--packet-size <n>";
        public const string OptionAttempts = "--attempts <n>";
        public const string OptionLossDelay = "--loss-delay <ms>";
        public const string OptionDynamic = "--dynamic";
        public const string OptionDontFragment = "--df";
        public const string OptionFormat = "--format <plain|csv|json>";
        public const string OptionLanguage = "  --language <en|cn|fr|de|ru|jp> - interface language";
        public const string OptionReset = "--reset";
        public const string OptionHelp = "--help";
        public const string HelpShort = "-?";
        public const string HelpHost = "  --host <ip|domain> - remote host IP or domain";
        public const string HelpPacketSize = "  --packet-size <n> - maximum packet size";
        public const string HelpAttempts = "  --attempts <n> - attempts per hop";
        public const string HelpLossDelay = "  --loss-delay <ms> - delay after packet loss";
        public const string HelpDynamic = "  --dynamic - scan increasing packet sizes";
        public const string HelpDontFragment = "  --df - do not fragment packets";
        public const string HelpIpv4 = "  --ipv4 - resolve and probe IPv4 only";
        public const string HelpIpv6 = "  --ipv6 - resolve and probe IPv6 only";
        public const string HelpFormat = "  --format <plain|csv|json> - output format";
        public const string OptionResetHelp = "  --reset - remove saved UI state";
        public const string HelpHelp = "  --help - show this help";
        public const string DidYouMeanFormat = "Did you mean: {0}?";
        public const string FormatPlain = "plain";
        public const string FormatCsv = "csv";
        public const string FormatJson = "json";
        public const string KindMessage = "message";
        public const string KindStats = "stats";
        public const string ColumnTtl = "ttl";
        public const string ColumnIp = "ip";
        public const string ColumnHost = "host";
        public const string ColumnAvg = "avg";
        public const string ColumnJitter = "jitter";
        public const string ColumnStdDev = "stddev";
        public const string ColumnMin = "min";
        public const string ColumnMax = "max";
        public const string ColumnLast = "last";
        public const string ColumnSent = "sent";
        public const string ColumnReceived = "recv";
        public const string ColumnStatus = "status";
        public const string ColumnDiagnosis = "diagnosis";
        public const string ColumnKind = "kind";
        public const string ColumnText = "text";
        public const string ColumnHops = "hops";
        public const string UnknownOptionFormat = "Unknown option: {0}";
        public const string MissingValueFormat = "Missing value for option: {0}";
        public const string InvalidNumberFormat = "Invalid number for option {0}: {1}";
        public const string RangeFormat = "Option {0} must be between {1} and {2}.";
        public const string HostRequired = "--host is required for run and monitor.";
        public const string InvalidCommandFormat = "Unknown command: {0}";
        public const string InvalidFormat = "--format must be plain, csv or json.";
        public const string InvalidLanguageFormat = "Unknown language: {0}";
        public const string LinuxIcmpHint = "Note: ICMP probes on Linux may require net.ipv4.ping_group_range or CAP_NET_RAW.";
        public const string ResetDone = "Saved UI state removed.";
        public const string LogStartedFormat = "Log file: {0}";
        public const string ErrorPrefix = "Error: {0}";
        public const string Cancelled = "Cancelled.";
        public const string SummaryTitle = "Summary:";
        public const string StatsHeader = "ttl  ip  host  avg  jitter  stddev  min  max  last  sent  recv  status  diagnosis";
        public const string StatsRowFormat = "{0,-3} {1,-39} {2,-20} {3,-8} {4,-8} {5,-8} {6,-8} {7,-8} {8,-8} {9,-5} {10,-5} {11,-12} {12}";
        public const string CsvStatsHeader = "kind,ttl,ip,host,avg,jitter,stddev,min,max,last,sent,recv,status,diagnosis";
    }

    public static class Ui
    {
        public const string RemoteHostLabel = "Remote host (IP or domain):";
        public const string PacketSizeLabelFormat = "Packet max size ({0}-{1}):";
        public const string AttemptsLabel = "Attempts (1-1000):";
        public const string LossDelayLabel = "Loss delay, ms:";
        public const string FontSizeLabel = "Font size:";
        public const string IpFamilyLabel = "IP version:";
        public const string LanguageLabel = "Language:";
        public const string DynamicPacketSizeLabel = "Dynamic packet size";
        public const string DontFragmentLabel = "Don't fragment (DF)";
        public const string DarkThemeLabel = "Dark theme";
        public const string MonitoringModeLabel = "Continuous monitoring mode";
        public const string RunButton = "Run";
        public const string StopButton = "Stop";
        public const string Stopping = "Stopping...";
        public const string Ready = "Ready";
        public const string ResolvingHost = "Resolving host...";
        public const string Running = "Running...";
        public const string Monitoring = "Monitoring...";
        public const string Completed = "Completed";
        public const string MonitoringCompleted = "Monitoring stopped";
        public const string Cancelled = "Cancelled";
        public const string ResolveError = "Resolve error";
        public const string Error = "Error";
        public const string ValidationTitle = "Validation";
        public const string RemoteHostRequired = "Remote host is required (IP or domain name).";
        public const string LogFontFamily = "Consolas";
        public const string GridFontFamily = "Segoe UI";
        public const string NoDns = "(no DNS)";
        public const string NoAnswer = "no answer";
        public const string Dash = "-";
        public const string HostLabelFormat = "{0} [{1}]";
        public const string AvgPingFormat = "F1";

        public const string ColumnKeyHostName = "HostName";
        public const string ColumnKeyIp = "Ip";
        public const string ColumnKeyAvgPing = "AvgPing";
        public const string ColumnKeyJitter = "Jitter";
        public const string ColumnKeyStdDev = "StdDev";
        public const string ColumnKeyMinPing = "MinPing";
        public const string ColumnKeyMaxPing = "MaxPing";
        public const string ColumnKeyLastPing = "LastPing";
        public const string ColumnKeySent = "Sent";
        public const string ColumnKeyReceived = "Received";
        public const string ColumnKeyStatus = "Status";
        public const string ColumnKeyDiagnosis = "Diagnosis";

        public const string ColumnHostName = "Host name";
        public const string ColumnIp = "IP";
        public const string ColumnAvgPing = "Avg ping, ms";
        public const string ColumnJitter = "Jitter, ms";
        public const string ColumnStdDev = "StdDev, ms";
        public const string ColumnMinPing = "Min ping, ms";
        public const string ColumnMaxPing = "Max ping, ms";
        public const string ColumnLastPing = "Last ping, ms";
        public const string ColumnSent = "Sent";
        public const string ColumnReceived = "Received";
        public const string ColumnStatus = "Status";
        public const string ColumnDiagnosis = "Diagnosis";

        internal static readonly string[] LanguageNames =
            ["English", "Chinese", "French", "German", "Russian", "Japanese"];
    }

    public static class Diagnosis
    {
        public const string None = "—";
        public const string IcmpDisabled = "ICMP disabled";
        public const string PacketSizeLimit = "ICMP size truncation";
        public const string AntiFlood = "ICMP anti-flood";
    }

    public static class Status
    {
        public const string Pending = "pending";
        public const string NoAnswer = "no answer";
        public const string Normal = "normal";
        public const string Target = "target";
        public const string Responds = "responds";
        public const string PacketLossFormat = "packet loss ({0:F0}%)";
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
        public const string LanguageChangedFormat = "Settings saved: Language={0}";
        public const string UiStateLoaded = "UI state loaded.";
        public const string UiStateReset = "UI state reset by launch argument.";
        public const string UiStateCorruptedReset = "UI state file was invalid and has been removed.";
        public const string UiStateSaved = "UI state saved.";
        public const string UiStateSaveFailed = "UI state could not be saved.";
        public const string RunRequestedFormat = "Run requested: host='{0}', maxPacketSize={1}, attempts={2}, dynamic={3}, lossDelayMs={4}, dontFragment={5}";
        public const string MonitorRequestedFormat = "Monitoring requested: host='{0}', packetSize={1}, lossDelayMs={2}, dontFragment={3}";
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
        public const string NoAnswer = "no answer";
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
        public const string DontFragmentEnabled = "Don't Fragment (DF): enabled";
        public const string PrecheckHeaderFormat = "DF precheck before the main test: {0} bytes";
        public const string PrecheckContextFormat = "df-precheck hop={0} addr={1} size={2}";
        public const string PrecheckSuccessFormat = "  df-precheck hop {0,2} {1}: size={2}, rtt={3}";
        public const string PrecheckFailureFormat = "  df-precheck hop {0,2} {1}: size={2}, {3}";
        public const string NoRouteHops = "Status: not response (no route hops discovered)";
        public const string PacketSizeHeaderFormat = "[Packet size {0} bytes]";
        public const string SkipHopFormat = "  hop {0,2} {1}: skipped (100% loss, ICMP banned or size restriction).";
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
        public const string DiagnosisFormat = "  hop {0,2} {1}: diagnosis — {2}";

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
