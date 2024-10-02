#pragma once

class HookHelper
{
public:
    static bool FillWithNops(void* toHook, int len);
};
