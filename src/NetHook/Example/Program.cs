// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using NetHook.Hook;
using Reloaded.Injector;

var process = Process.GetProcessesByName("client").FirstOrDefault();
if (process is null) return;

//use framework of your preference to inject the dll
var injector = new Injector(process);
injector.Inject("HookInjection.dll");

var hookSession = new HookSession(process);

hookSession.Hook(
    [0x83, 0xC4, 0x10, 0x5E, 0x8B, 0xE5, 0x5D, 0xC2, 0x10, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC], "client.exe",
    Events.OnCreatureMovement);

Console.ReadLine();

public class Events
{
    public static void OnCreatureMovement(object sender, HookEventArgs args)
    {
        Console.WriteLine(args.GetRegister(Register.Eax));
    }
}