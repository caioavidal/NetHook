using System.Diagnostics;

namespace NetHook;

public class RemoteProcess
{
    public RemoteProcess(int processId)
    {
        ProcessId = processId;
        Process = Process.GetProcessById(ProcessId);
    }

    public RemoteProcess(Process process) : this(process.Id)
    {
        Process = process;
    }

    public int ProcessId { get; }
    public Process Process { get; set; }
}