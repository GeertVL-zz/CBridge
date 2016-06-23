using System;
using System.Collections.Generic;
using System.Linq;
using CBridge.Bridge;
using CBridge.Clang;
using CBridge.Roslyn;

namespace CBridge
{
  class Program
  {
    public class ApplicationArguments
    {
      public string InputHeader { get; set; }
      public string OutputFile { get; set; }
      public string InvokerClassName { get; set; }
      public string CDllName { get; set; }
      public string NamespaceName { get; set; }
    }

    static void Main(string[] args)
    {
      //var am = new ApplicationArguments
      //{
      //  InputHeader = @"C:\works\ewms\Main\Source\Ewms.Service.Ewms_rec\ewms_rec.h",
      //  OutputFile = @"c:\works\ewmsrec.cs",
      //  CDllName = "Egemin.Ewms.Service.Ewms_rec.dll",
      //  InvokerClassName = @"RecPInvoker",
      //  NamespaceName = "EwmsRec"
      //};

      var am = new ApplicationArguments
      {
        InputHeader = args[0],
        OutputFile = args[1],
        CDllName = args[2],
        InvokerClassName = args[3],
        NamespaceName = args[4]
      };

      var includeDirs = new List<string>();
      includeDirs.Add(@"C:\Program Files\LLVM\include\clang-c");

      var createIndex = ClangInvoker.createIndex(0, 0);
      string[] arr = { "-x", "c++" };
      arr = arr.Concat(includeDirs.Select(x => "-I" + x)).ToArray();

      try
      {
        CXTranslationUnit translationUnit;
        CXUnsavedFile unsavedFile;
        var translationUnitError = ClangInvoker.parseTranslationUnit2(createIndex, am.InputHeader, arr, 3, out unsavedFile, 0, 0, out translationUnit);
        if (translationUnitError == CXErrorCode.CXError_Success)
        {
          var tree = new List<ASTFunction>();
          var functionVisitor = new FunctionVisitor(tree);
          ClangInvoker.visitChildren(ClangInvoker.getTranslationUnitCursor(translationUnit), functionVisitor.Visit, new CXClientData(IntPtr.Zero));

          var roslynTranslator = new TreeTranslator(am.OutputFile);
          roslynTranslator.Invoke(tree, am.InvokerClassName, am.CDllName, am.NamespaceName);
          roslynTranslator.InvokeInterface(tree, am.InvokerClassName, am.NamespaceName);

        }

        ClangInvoker.disposeTranslationUnit(translationUnit);
        ClangInvoker.disposeIndex(createIndex);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      Console.WriteLine("Translationwork is done.");
    }
  }
}
