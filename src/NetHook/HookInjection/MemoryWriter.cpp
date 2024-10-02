#include "MemoryWriter.h"
#include <iostream>
using namespace std;

byte* ptr_;
int index_;

void MemoryWriter::WriteInt(int value)
{
    *(int*)(ptr_ + index_) = value;
    index_ += sizeof(int);
}

void MemoryWriter::WriteByte(char value)
{
    *(ptr_ + index_) = value;
    index_++;
}

void MemoryWriter::WriteBytes(byte* data, int length)
{
    std::memcpy(&ptr_[index_], data, length);
    index_ += length;
}

void MemoryWriter::WriteAddress(DWORD value)
{
    *(DWORD*)(ptr_ + index_) = value - (DWORD)ptr_ - index_ - sizeof(DWORD);
    index_ += sizeof(DWORD);
}
