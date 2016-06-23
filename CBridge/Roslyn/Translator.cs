using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

    public void Invoke(IList<ASTFunction> clangTree, string invokerClassName, string cDllName, string namespaceName)
    {
      var classDeclaration = ClassDeclaration(invokerClassName)
        .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword))).WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName($"I{invokerClassName}")))));

      classDeclaration = CreatePublicMethods(classDeclaration, clangTree);
      classDeclaration = CreateInvocationMethods(classDeclaration, clangTree, cDllName);

      var cu = CompilationUnit()
        .WithUsings(SingletonList(UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"),IdentifierName("Runtime")),IdentifierName("InteropServices")))))
        .WithMembers(SingletonList<MemberDeclarationSyntax>(
        NamespaceDeclaration(QualifiedName(QualifiedName(QualifiedName(IdentifierName("Egemin"),IdentifierName("Ewms")),IdentifierName("Service")),IdentifierName(namespaceName)))
        .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration))));
      var formattedSource = Formatter.Format(cu.SyntaxTree.GetRoot(), SyntaxAnnotation.ElasticAnnotation, new AdhocWorkspace());
      File.WriteAllText(_filePath, formattedSource.ToFullString());
    }

    private ClassDeclarationSyntax CreatePublicMethods(ClassDeclarationSyntax classDeclaration, IList<ASTFunction> clangTree)
    {
      foreach (var node in clangTree)
      {
        var publicMethodName = new CultureInfo("en-US", false).TextInfo.ToTitleCase(node.Name.Replace("_", " ")).Replace(" ",  "");
        var method = MethodDeclaration(PredefinedType(Token(TranslateType(node.ReturnType))), Identifier(publicMethodName))
          .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));
        method = TranslateArguments(method, node, CreateMethodParameter, CreateMethodBody);
        classDeclaration = classDeclaration.AddMembers(method);
      }

      return classDeclaration;
    }

    private ClassDeclarationSyntax CreateInvocationMethods(ClassDeclarationSyntax classDeclaration, IList<ASTFunction> clangTree, string cDllName)
    {
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
        method = TranslateArguments(method, node, CreateMarshalParameter, null).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        classDeclaration = classDeclaration.AddMembers(method);
      }

      return classDeclaration;
    }

    private SyntaxKind TranslateType(string dataType)
    {
      switch (dataType)
      {
        case "char *":
        case "char*":
        case "string":
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

    private MethodDeclarationSyntax TranslateArguments(MethodDeclarationSyntax method, ASTFunction function, Func<ASTArgument, ParameterSyntax> createParamFunc, Func<string, IList<ASTArgument>, BlockSyntax> bodyFunc)
    {
      var tokens = new List<SyntaxNodeOrToken>();
      foreach (var arg in function.Arguments)
      {
        var param = createParamFunc(arg);
        tokens.Add(param);
        tokens.Add(Token(SyntaxKind.CommaToken));
      }

      tokens.RemoveAt(tokens.Count - 1);
      method = method.WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(tokens.ToArray())));
      if (bodyFunc != null)
      {
        method = method.WithBody(bodyFunc(function.Name, function.Arguments));
      }

      return method;
    }

    private ParameterSyntax CreateMarshalParameter(ASTArgument arg)
    {
      var param = Parameter(Identifier(arg.Name))
        .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("MarshalAs"))
          .WithArgumentList(AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
             MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("UnmanagedType"), IdentifierName(TranslateUType(arg.VarType)))))))))))
        .WithType(PredefinedType(Token(TranslateType(arg.VarType))));

      return param;
    }

    private ParameterSyntax CreateMethodParameter(ASTArgument arg)
    {
      var param = Parameter(Identifier(arg.Name))
        .WithType(PredefinedType(Token(TranslateType(arg.VarType))));

      return param;
    }

    private ArgumentSyntax CreateMethodArgument(string argumentName)
    {
      return Argument(IdentifierName(argumentName));
    }

    private BlockSyntax CreateMethodBody(string methodName, IList<ASTArgument> arguments)
    {
      var statement = InvocationExpression(IdentifierName(methodName));
      var tokens = new List<SyntaxNodeOrToken>
      {
        CreateMethodArgument("_cpphenv"),
        Token(SyntaxKind.CommaToken),
        CreateMethodArgument("_cpphdbc"),
        Token(SyntaxKind.CommaToken),
        CreateMethodArgument("_cpplastconnecttime"),
        Token(SyntaxKind.CommaToken)
      };
      foreach (var arg in arguments)
      {
        tokens.Add(CreateMethodArgument(arg.Name));
        tokens.Add(Token(SyntaxKind.CommaToken));
      }
      tokens.RemoveAt(tokens.Count - 1);

      return Block(SingletonList(ReturnStatement(statement.WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(tokens))))));
    }
  }
}
