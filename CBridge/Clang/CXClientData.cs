using System;

namespace CBridge.Clang
{
  public partial struct CXClientData
  {
    public CXClientData(IntPtr pointer)
    {
      this.Pointer = pointer;
    }

    public IntPtr Pointer;
  }
}