#pragma once
#include <windows.h>

class HookPipeServer
{
    static DWORD __stdcall ListenToPipe(LPVOID lpParam);

    static OVERLAPPED overlapped;

public:
    static HANDLE Pipe;
    static DWORD WINAPI Connect(LPVOID lpParam);
    static void SendToPipe(unsigned int eax, unsigned int ecx, unsigned int edx, unsigned int ebx,
                           unsigned int esp, unsigned int ebp, unsigned int esi,
                           unsigned int edi, DWORD lowExternalPtr, DWORD highExternalPtr);
};
