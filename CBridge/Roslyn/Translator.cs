using System;
using System.Collections.Generic;
using System.Globalization;
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

    public void Invoke(IList<ASTFunction> clangTree, string invokerClassName, string cDllName, string namespaceName)
    {
      var classDeclaration = ClassDeclaration(invokerClassName)
        .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword))).WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName($"I{invokerClassName}")))));

      classDeclaration = classDeclaration.AddMembers(CreatePrivateField("cpphenv", "int")).AddMembers(CreatePrivateField("cpphdbc", "int")).AddMembers(CreatePrivateField("cpplastconnecttime", "string"));
      classDeclaration = classDeclaration.AddMembers(CreateDelegation(invokerClassName));
      classDeclaration = classDeclaration.AddMembers(CreateConstructor(invokerClassName));
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

    public void InvokeInterface(IList<ASTFunction> clangTree, string invokerClassName, string namespaceName)
    {
      var interfaceDeclaration = InterfaceDeclaration("I" + invokerClassName).WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)));
      interfaceDeclaration = CreateInterfaceMethods(interfaceDeclaration, clangTree);

      var cu = CompilationUnit()
        .WithUsings(SingletonList(UsingDirective(QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("Runtime")), IdentifierName("InteropServices")))))
        .WithMembers(SingletonList<MemberDeclarationSyntax>(NamespaceDeclaration(QualifiedName(QualifiedName(QualifiedName(IdentifierName("Egemin"), IdentifierName("Ewms")), IdentifierName("Service")), IdentifierName(namespaceName)))
          .WithMembers(SingletonList<MemberDeclarationSyntax>(interfaceDeclaration))));

      var formattedSource = Formatter.Format(cu.SyntaxTree.GetRoot(), SyntaxAnnotation.ElasticAnnotation, new AdhocWorkspace());
      var info = new FileInfo(_filePath);
      if (info.DirectoryName != null)
      {
        var path = Path.Combine(info.DirectoryName, "I" + info.Name);
        File.WriteAllText(path, formattedSource.ToFullString());
      }
    }

    private ConstructorDeclarationSyntax CreateConstructor(string name)
    {
      return ConstructorDeclaration(Identifier(name)).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
        .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(
          new SyntaxNodeOrToken[]{ Parameter(Identifier("cpphenv")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))), Token(SyntaxKind.CommaToken),
                                   Parameter(Identifier("cpphdbc")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))), Token(SyntaxKind.CommaToken),
                                   Parameter(Identifier("cpplastconnecttime")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))})))
        .WithBody(Block(
          ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,IdentifierName("_cpphenv"),IdentifierName("cpphenv"))),
          ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,IdentifierName("_cpphdbc"),IdentifierName("cpphdbc"))),
          ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,IdentifierName("_cpplastconnecttime"),IdentifierName("cpplastconnecttime")))));
    }

    private InterfaceDeclarationSyntax CreateInterfaceMethods(InterfaceDeclarationSyntax interfaceDeclaration, IList<ASTFunction> clangTree)
    {
      foreach (var node in clangTree)
      {
        var methodName = new CultureInfo("en-US", false).TextInfo.ToTitleCase(node.Name.Replace("_", " ")).Replace(" ", "");
        var method = MethodDeclaration(PredefinedType(Token(TranslateType(node.ReturnType))), Identifier(methodName));
        method = TranslateArguments(method, node, CreateMethodParameter, null).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        interfaceDeclaration = interfaceDeclaration.AddMembers(method);
      }

      return interfaceDeclaration;
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

    private MethodDeclarationSyntax TranslateArguments(MethodDeclarationSyntax method, ASTFunction function, Func<ASTArgument, ParameterSyntax> createParamFunc, Func<ASTFunction, BlockSyntax> bodyFunc)
    {
      var tokens = new List<SyntaxNodeOrToken>();
      foreach (var arg in function.Arguments)
      {
        var param = createParamFunc(arg);
        if (param != null)
        {
          tokens.Add(param);
          tokens.Add(Token(SyntaxKind.CommaToken));
        }
      }

      if (tokens.Count > 0)
        tokens.RemoveAt(tokens.Count - 1);
      method = method.WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(tokens.ToArray())));
      if (bodyFunc != null)
      {
        method = method.WithBody(bodyFunc(function));
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
      if (IsDatabaseVars(arg.Name)) return null;

      var param = Parameter(Identifier(arg.Name))
        .WithType(PredefinedType(Token(TranslateType(arg.VarType))));

      return param;
    }

    private ArgumentSyntax CreateMethodArgument(string argumentName)
    {
      return Argument(IdentifierName(argumentName));
    }

    private BlockSyntax CreateMethodBody(ASTFunction function)
    {
      var statement = InvocationExpression(IdentifierName(function.Name));
      var tokens = new List<SyntaxNodeOrToken>();
      if (function.Arguments.Any(i => IsDatabaseVars(i.Name)))
      {
        tokens.AddRange(new List<SyntaxNodeOrToken>
        {
          CreateMethodArgument("_cpphenv"),
          Token(SyntaxKind.CommaToken),
          CreateMethodArgument("_cpphdbc"),
          Token(SyntaxKind.CommaToken),
          CreateMethodArgument("_cpplastconnecttime"),
          Token(SyntaxKind.CommaToken)
        });
      }
      foreach (var arg in function.Arguments)
      {
        if (!IsDatabaseVars(arg.Name))
        {
          tokens.Add(CreateMethodArgument(arg.Name));
          tokens.Add(Token(SyntaxKind.CommaToken));
        }
      }
      tokens.RemoveAt(tokens.Count - 1);

      if (function.ReturnType == "void")
      {
        return Block(SingletonList(ExpressionStatement(statement.WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(tokens))))));
      }

      return Block(SingletonList(ReturnStatement(statement.WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(tokens))))));
    }

    private FieldDeclarationSyntax CreatePrivateField(string name, string type)
    {
      var field = FieldDeclaration(VariableDeclaration(PredefinedType(Token(TranslateType(type)))).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("_" + name)))))
        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

      return field;
    }

    private DelegateDeclarationSyntax CreateDelegation(string type)
    {
      var delegation = DelegateDeclaration(IdentifierName("I" + type), Identifier("Factory")).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
        .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(new SyntaxNodeOrToken[]{
          Parameter(Identifier("cpphenv")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))), Token(SyntaxKind.CommaToken), Parameter(Identifier("cpphdbc"))
            .WithType(PredefinedType(Token(SyntaxKind.IntKeyword))),Token(SyntaxKind.CommaToken),Parameter(Identifier("cpplastconnecttime")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword)))})));

      return delegation;
    }

    private bool IsDatabaseVars(string varName)
    {
      return (varName.Contains("henv")) || (varName.Contains("hdbc") || (varName.Contains("last_connect_time")));
    }
  }
}
