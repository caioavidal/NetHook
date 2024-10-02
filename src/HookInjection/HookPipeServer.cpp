#include "HookPipeServer.h"
#include <string>
#include "Windows.h"
#include <iostream>
#include <cstring>
#include "HookSession.h"
using namespace std;

HANDLE HookPipeServer::Pipe = nullptr;

DWORD WINAPI HookPipeServer::Connect(LPVOID lpParam)
{
    CreateThread(nullptr, 0, ListenToPipe, nullptr, 0, nullptr);

    std::wstring pid = std::to_wstring(GetCurrentProcessId());

    wstring pipeName = L"\\\\.\\Pipe\\HookPipeServer-" + pid;

    while (true)
    {
        HANDLE pipe = CreateFile(pipeName.c_str(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
                                 nullptr,
                                 OPEN_EXISTING,
                                 FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED,
                                 nullptr);

        if (pipe == INVALID_HANDLE_VALUE)
        {
            Sleep(1000);
            continue;
        }

        Pipe = pipe;

        while (true)
        {
            Sleep(1000);

            if (WaitNamedPipe(pipeName.c_str(), NMPWAIT_WAIT_FOREVER) == 0)
            {
                printf("WaitNamedPipe failed. error=%d\n", GetLastError());
                break;
            }
        }
    }
}

DWORD WINAPI HookPipeServer::ListenToPipe(LPVOID lpParam)
{
    const auto buffer = new byte[13];
    while (true)
    {
        Sleep(1);

        if (Pipe == INVALID_HANDLE_VALUE || Pipe == nullptr)
        {
            continue;
        }

        DWORD bytesRead;
        const bool result = ReadFile(Pipe, buffer, 13, &bytesRead, nullptr); // repeat loop if ERROR_MORE_DATA 

        if (result == false) continue;

        const int hookLength = buffer[0];

        unsigned int hookAddress;
        memcpy(&hookAddress, &buffer[1], sizeof(unsigned int));

        unsigned long externalCallbackPointer;
        memcpy(&externalCallbackPointer, &buffer[5], 8);

        HookSession::Hook(hookAddress, hookLength, &externalCallbackPointer);
    }
}

OVERLAPPED HookPipeServer::overlapped = {};

void HookPipeServer::SendToPipe(unsigned int eax, unsigned int ecx, unsigned int edx, unsigned int ebx,
                                unsigned int esp, unsigned int ebp, unsigned int esi,
                                unsigned int edi, DWORD lowExternalPtr, DWORD highExternalPtr)
{
    if (overlapped.hEvent == nullptr)
    {
        overlapped.hEvent = CreateEvent(nullptr, TRUE, FALSE, nullptr);
    }
    unsigned int buffer[10] = {eax, ecx, edx, ebx, esp, ebp, esi, edi, lowExternalPtr, highExternalPtr};
    DWORD messageSize = sizeof(buffer);

    WriteFile(Pipe, buffer, messageSize, &messageSize, &overlapped);
}
