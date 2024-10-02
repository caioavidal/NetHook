#include "CallbackHook.h"

int* CallbackHook::CallbackCodePtr = nullptr;

void __declspec(naked) CallbackHook::Callback()
{
    __asm {
        push ebp
        mov ebp, esp

        push DWORD PTR [ebp+8]
        push DWORD PTR [ebp+12]

        push edi
        push esi
        push ebp
        push esp; esp not accurate yet.
        push ebx
        push edx
        push ecx
        push eax

        call HookPipeServer::SendToPipe

        pop eax; restore all registers
        pop ecx
        pop edx
        pop ebx
        add esp, 4; pop off esp
        pop ebp
        pop esi
        pop edi
        add esp, 8

        nop
        pop ebp
        ret
        }
}
