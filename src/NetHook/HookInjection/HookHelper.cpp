#include "HookHelper.h"
#include <Windows.h>

bool HookHelper::FillWithNops(void* toHook, int len)
{
    if (len < 5) return false;

    DWORD curProtection;
    VirtualProtect(toHook, len, PAGE_EXECUTE_READWRITE, &curProtection);

    memset(toHook, 0x90, len);

    DWORD temp;
    VirtualProtect(toHook, len, curProtection, &temp);

    return true;
}
