using System;
using CBridge.Clang;

namespace CBridge
{
  internal sealed class InnerVisitor : ICXCursorVisitor
  {
    public CXChildVisitResult Visit(CXCursor cursor, CXCursor parent, IntPtr data)
    {
      var cs = ClangInvoker.getCursorSpelling(cursor);
      CXCursorKind curKind = ClangInvoker.getCursorKind(cursor);
      var cks = ClangInvoker.getCursorKindSpelling(curKind);
      Console.WriteLine("Cursor definition is {0} with kind {1}", cs, cks);

      return CXChildVisitResult.CXChildVisit_Continue;
    }
  }
}
