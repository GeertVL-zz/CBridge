using System;
using System.Collections.Generic;
using System.Linq;
using CBridge.Bridge;
using CBridge.Clang;
using CBridge.Roslyn;
using Fclp;
using Fclp.Internals.Extensions;

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
      var p = new FluentCommandLineParser<ApplicationArguments>();
      p.Setup(arg => arg.InputHeader).As('i').Required().WithDescription("Please fill in a headerfile to bridge");
      p.Setup(arg => arg.OutputFile).As('o').Required().WithDescription("Please fill in an output file.");
      p.Setup(arg => arg.CDllName).As('d', "cdllname").WithDescription("Please fill in the dll name to invoke too.");
      p.Setup(arg => arg.InvokerClassName).As('c').Required().WithDescription("Please fill in the classname of the invoker.");
      p.Setup(arg => arg.NamespaceName).As('n').Required().WithDescription("Please fill in the last part of the namespace");

      var result = p.Parse(args);

      if (!result.HasErrors)
      {
        var createIndex = ClangInvoker.createIndex(0, 0);
        string[] arr = {"-x", "c++"};

        CXTranslationUnit translationUnit;
        CXUnsavedFile unsavedFile;
        var translationUnitError = ClangInvoker.parseTranslationUnit2(createIndex, p.Object.InputHeader, arr, 3, out unsavedFile, 0, 0, out translationUnit);

        if (translationUnitError == CXErrorCode.CXError_Success)
        {
          var tree = new List<ASTFunction>();
          var functionVisitor = new FunctionVisitor(tree);
          ClangInvoker.visitChildren(ClangInvoker.getTranslationUnitCursor(translationUnit), functionVisitor.Visit, new CXClientData(IntPtr.Zero));

          var roslynTranslator = new TreeTranslator(p.Object.OutputFile);
          roslynTranslator.Invoke(tree, p.Object.InvokerClassName, p.Object.CDllName, p.Object.NamespaceName);

        }

        ClangInvoker.disposeTranslationUnit(translationUnit);
        ClangInvoker.disposeIndex(createIndex);

        Console.WriteLine("Translationwork is done.");
        Console.ReadKey();
      }
      else
      {
        result.Errors.ForEach(i => Console.WriteLine("Option {0} is missing. {1}", i.Option.ShortName, i.Option.Description));
      }

    }
  }
}
