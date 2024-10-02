#pragma once
#include "Windows.h"

class HookSession
{
public:
    static void Hook(DWORD hookAddress, int hookLength, unsigned long externalCallbackPointer);
};
