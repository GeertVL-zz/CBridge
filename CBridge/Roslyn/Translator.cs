using System.Collections.Generic;
using System.IO;
using System.Linq;
using CBridge.Bridge;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CBridge.Roslyn
{
  internal class TreeTranslator
  {
    private readonly string _filePath;

    public TreeTranslator(string filePath)
    {
      _filePath = filePath;
    }

    public void Invoke(IList<ASTFunction> clangTree, string invokerClassName, string cDllName)
    {
      var classDeclaration = ClassDeclaration(invokerClassName)
        .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword))).WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName($"I{invokerClassName}")))));
      foreach (var node in clangTree)
      {
        var method = MethodDeclaration(PredefinedType(Token(TranslateType(node.ReturnType))), Identifier(node.Name))
          .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("DllImport"))
            .WithArgumentList(AttributeArgumentList(SeparatedList<AttributeArgumentSyntax>(
              new SyntaxNodeOrToken[]
              {
                AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal($"@\"{cDllName}\"", cDllName))),
                Token(SyntaxKind.CommaToken),
                AttributeArgument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("CallingConvention"), IdentifierName("Cdecl")))
                  .WithNameEquals(NameEquals(IdentifierName("CallingConvention")))
              }
              )))
           ))))
          .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ExternKeyword)));
        method = TranslateArguments(method, node.Arguments).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        classDeclaration = classDeclaration.AddMembers(method);
      }

      var cu = CompilationUnit()
        .WithMembers(
          SingletonList<MemberDeclarationSyntax>(classDeclaration));
      var formattedSource = Formatter.Format(cu.SyntaxTree.GetRoot(), SyntaxAnnotation.ElasticAnnotation, new AdhocWorkspace());
      File.WriteAllText(_filePath, formattedSource.ToFullString());
    }

    private SyntaxKind TranslateType(string dataType)
    {
      switch (dataType)
      {
        case "char *":
        case "char*":
          return SyntaxKind.StringKeyword;
        case "int":
          return SyntaxKind.IntKeyword;
        default:
          return SyntaxKind.VoidKeyword;
      }
    }

    private string TranslateUType(string dataType)
    {
      switch (dataType)
      {
        case "char *":
        case "char*":
          return "LPStr";
        case "int":
          return "I4";
        default:
          return "";
      }
    }

    private MethodDeclarationSyntax TranslateArguments(MethodDeclarationSyntax method, IList<ASTArgument> arguments)
    {
      var tokens = new List<SyntaxNodeOrToken>();
      foreach (var arg in arguments)
      {

        var param = Parameter(Identifier(arg.Name))
          .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("MarshalAs"))
            .WithArgumentList(AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
              MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("UnmanagedType"), IdentifierName(TranslateUType(arg.VarType)))))))))))
          .WithType(PredefinedType(Token(TranslateType(arg.VarType))));       
        tokens.Add(param);
        tokens.Add(Token(SyntaxKind.CommaToken));
      }

      tokens.RemoveAt(tokens.Count - 1);
      return method.WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(tokens.ToArray())));
    }
  }
}
