namespace TraceDiag.Services.Abstractions;

/// <summary>Exposes the active log file path to hosts that display a startup hint.</summary>
public interface ILogPathProvider
{
    string LogFilePath { get; }
}
