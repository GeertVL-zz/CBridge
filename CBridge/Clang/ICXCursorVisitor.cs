using System;

namespace CBridge.Clang
{
  internal interface ICXCursorVisitor
  {
    CXChildVisitResult Visit(CXCursor cursor, CXCursor parent, IntPtr data);
  }
}