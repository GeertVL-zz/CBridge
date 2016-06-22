using System;
using System.Collections.Generic;
using CBridge.Bridge;
using CBridge.Clang;

namespace CBridge
{
  internal sealed class FunctionVisitor : ICXCursorVisitor
  {
    private readonly IList<ASTFunction> _astTree;

    public FunctionVisitor(IList<ASTFunction> astTree)
    {
      _astTree = astTree;
    }

    public CXChildVisitResult Visit(CXCursor cursor, CXCursor parent, IntPtr data)
    {

      if (cursor.IsInSystemHeader())
      {
        return CXChildVisitResult.CXChildVisit_Continue;
      }

      var cs = ClangInvoker.getCursorSpelling(cursor);
      CXCursorKind curKind = ClangInvoker.getCursorKind(cursor);
      var cks = ClangInvoker.getCursorKindSpelling(curKind);
      Console.WriteLine("Cursor definition is {0} with kind {1}", cs, cks);
      

      if (curKind == CXCursorKind.CXCursor_FirstDecl)
      {
        return CXChildVisitResult.CXChildVisit_Recurse;
      }

      if (curKind == CXCursorKind.CXCursor_VarDecl)
      {
        var cl = ClangInvoker.getCursorLocation(cursor);
        CXFile file;
        uint line, column, offset;
        ClangInvoker.getSpellingLocation(cl, out file, out line, out column, out offset);
        Console.WriteLine("Location is in file {0}, line {1}, column {2}, offset {3}", file, line, column, offset);
        var cdn = ClangInvoker.getCursorDisplayName(cursor);
        var cd = ClangInvoker.getCursorDefinition(cursor);

        var innerVisitor = new InnerVisitor();
        ClangInvoker.visitChildren(cursor, innerVisitor.Visit, new CXClientData(IntPtr.Zero));

        return CXChildVisitResult.CXChildVisit_Recurse;
      }

      if (curKind == CXCursorKind.CXCursor_FunctionDecl)
      {
        var astFunction = new ASTFunction
        {
          Name = ClangInvoker.getCursorSpelling(cursor).ToString(),
          ReturnType = ClangInvoker.getCursorResultType(cursor).ToString()
        };

        var argumentCount = ClangInvoker.Cursor_getNumArguments(cursor);
        for (uint i = 0; i < argumentCount; i++)
        {
          var argument = ClangInvoker.Cursor_getArgument(cursor, i);
          astFunction.Arguments.Add(new ASTArgument
          {
            Name = ClangInvoker.getCursorSpelling(argument).ToString(),
            VarType = ClangInvoker.getCursorType(argument).ToString()
          });           
        }

        _astTree.Add(astFunction);
      }

      return CXChildVisitResult.CXChildVisit_Continue;
    }
  }
}