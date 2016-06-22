using System;

namespace CBridge.Clang
{
  public partial struct CXTranslationUnit
  {
    public CXTranslationUnit(IntPtr pointer)
    {
      this.Pointer = pointer;
    }

    public IntPtr Pointer;
  }
}