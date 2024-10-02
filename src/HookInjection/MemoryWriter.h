#pragma once
#include "HookPipeServer.h"

class MemoryWriter
{
    int index_;
    byte* ptr_;

public:
    MemoryWriter(byte* ptr): index_(0)
    {
        this->ptr_ = ptr;
    }

    void WriteInt(int value);
    void WriteByte(char value);
    void WriteBytes(byte* data, int length); //std::memcpy(&ptr[5], originalCode, hookLength);
    void WriteAddress(DWORD value);
};
