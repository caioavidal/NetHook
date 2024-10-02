namespace NetHook.Hook;

public delegate void HookCallbackDelegate(object sender, HookEventArgs args);

public class HookEventArgs(uint[] registers)
{
    public uint[] Registers { get; } = registers;

    public uint GetRegister(Register register)
    {
        return Registers[(int)register];
    }
}