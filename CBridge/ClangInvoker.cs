using System;
using System.Runtime.InteropServices;
using CBridge.Clang;

namespace CBridge
{
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  public delegate CXChildVisitResult CXCursorVisitor(CXCursor @cursor, CXCursor @parent, IntPtr @client_data);

  public static class ClangInvoker
  {
    private const string libraryPath = "libclang.dll";

    [DllImport(libraryPath, EntryPoint = "clang_createIndex", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXIndex createIndex(int @excludeDeclarationsFromPCH, int @displayDiagnostics);


    [DllImport(libraryPath, EntryPoint = "clang_parseTranslationUnit2", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXErrorCode parseTranslationUnit2(CXIndex @CIdx, [MarshalAs(UnmanagedType.LPStr)] string @source_filename, string[] @command_line_args, int @num_command_line_args, out CXUnsavedFile @unsaved_files, uint @num_unsaved_files, uint @options, out CXTranslationUnit @out_TU);

    [DllImport(libraryPath, EntryPoint = "clang_getCursorSpelling", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXString getCursorSpelling(CXCursor @param0);

    [DllImport(libraryPath, EntryPoint = "clang_disposeString", CallingConvention = CallingConvention.Cdecl)]
    public static extern void disposeString(CXString @string);

    [DllImport(libraryPath, EntryPoint = "clang_Location_isInSystemHeader", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Location_isInSystemHeader(CXSourceLocation @location);

    [DllImport(libraryPath, EntryPoint = "clang_getCursorLocation", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXSourceLocation getCursorLocation(CXCursor @param0);

    [DllImport(libraryPath, EntryPoint = "clang_getCursorKind", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXCursorKind getCursorKind(CXCursor @param0);

    [DllImport(libraryPath, EntryPoint = "clang_getSpellingLocation", CallingConvention = CallingConvention.Cdecl)]
    public static extern void getSpellingLocation(CXSourceLocation @location, out CXFile @file, out uint @line, out uint @column, out uint @offset);

    [DllImport(libraryPath, EntryPoint = "clang_getTranslationUnitCursor", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXCursor getTranslationUnitCursor(CXTranslationUnit @param0);

    [DllImport(libraryPath, EntryPoint = "clang_visitChildren", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint visitChildren(CXCursor @parent, CXCursorVisitor @visitor, CXClientData @client_data);

    [DllImport(libraryPath, EntryPoint = "clang_disposeTranslationUnit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void disposeTranslationUnit(CXTranslationUnit @param0);

    [DllImport(libraryPath, EntryPoint = "clang_disposeIndex", CallingConvention = CallingConvention.Cdecl)]
    public static extern void disposeIndex(CXIndex @index);

    [DllImport(libraryPath, EntryPoint = "clang_getCursorKindSpelling", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXString getCursorKindSpelling(CXCursorKind @Kind);

    [DllImport(libraryPath, EntryPoint = "clang_Cursor_getNumArguments", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Cursor_getNumArguments(CXCursor @C);

    [DllImport(libraryPath, EntryPoint = "clang_Cursor_getArgument", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXCursor Cursor_getArgument(CXCursor @C, uint @i);

    [DllImport(libraryPath, EntryPoint = "clang_getCursorResultType", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXType getCursorResultType(CXCursor @C);

    [DllImport(libraryPath, EntryPoint = "clang_getTypeSpelling", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXString getTypeSpelling(CXType @CT);

    [DllImport(libraryPath, EntryPoint = "clang_getCursorType", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXType getCursorType(CXCursor @C);
    [DllImport(libraryPath, EntryPoint = "clang_getCursorDisplayName", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXString getCursorDisplayName(CXCursor @param0);
    [DllImport(libraryPath, EntryPoint = "clang_getCursorDefinition", CallingConvention = CallingConvention.Cdecl)]
    public static extern CXCursor getCursorDefinition(CXCursor @param0);
  }
}
