using System;

namespace CBridge.Clang
{
  public partial struct CXFile
  {
    public CXFile(IntPtr pointer)
    {
      this.Pointer = pointer;
    }

    public IntPtr Pointer;
  }
}