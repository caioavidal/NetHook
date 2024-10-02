#include "HookSession.h"
#include "CallbackHook.h"
#include "Helpers/MemoryWriter.h"
#include "OpCodeEnum.h"
#include "Helpers/HookHelper.h"

void HookSession::Hook(DWORD hookAddress, int hookLength, unsigned long* externalCallbackPointer)
{
    DWORD curProtection;

    auto toHook = (void*)hookAddress;
    VirtualProtect(toHook, hookLength, PAGE_EXECUTE_READWRITE, &curProtection);

    auto originalCode = new byte[hookLength];

    for (int i = 0; i < hookLength; i++)
    {
        originalCode[i] = *(unsigned int*)(hookAddress + i);
    }

    DWORD jmpBackAddy = hookAddress + hookLength;

    //set instruction to nope
    HookHelper::FillWithNops(toHook, hookLength);

    //allocate memory for jmp, call and original code
    int functionSize = hookLength + 25;
    auto ptr = static_cast<byte*>(VirtualAlloc(
        nullptr,
        functionSize,
        MEM_COMMIT | MEM_RESERVE,
        PAGE_EXECUTE_READWRITE));

    HookHelper::FillWithNops(ptr, functionSize);

    //Call callback function

    MemoryWriter writer(ptr);

    writer.WriteByte(PUSH);
    writer.WriteInt(*(int*)(externalCallbackPointer + 1)); //high long pointer

    writer.WriteByte(PUSH);
    writer.WriteInt(*externalCallbackPointer); //low long pointer

    writer.WriteByte(CALL);
    writer.WriteAddress((DWORD)CallbackHook::Callback);

    // Copy original code to the end of allocated memory area
    writer.WriteBytes(originalCode, hookLength);

    //Jump back to module code
    writer.WriteByte(JMP); //jmp
    writer.WriteAddress(jmpBackAddy);

    //jump to new code
    DWORD relativeAddress = (DWORD)ptr - (DWORD)toHook - 5;
    *static_cast<BYTE*>(toHook) = 0xE9;
    *(DWORD*)((DWORD)toHook + 1) = relativeAddress;

    DWORD temp;
    VirtualProtect(toHook, hookLength, curProtection, &temp);
}
