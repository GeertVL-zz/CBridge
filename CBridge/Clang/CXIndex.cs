using System;

namespace CBridge.Clang
{
  public partial struct CXIndex
  {
    public CXIndex(IntPtr pointer)
    {
      this.Pointer = pointer;
    }

    public IntPtr Pointer;
  }
}
