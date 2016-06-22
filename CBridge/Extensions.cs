using CBridge.Clang;

namespace CBridge
{
  internal static class Extensions
  {
    public static bool IsInSystemHeader(this CXCursor cursor)
    {
      return ClangInvoker.Location_isInSystemHeader(ClangInvoker.getCursorLocation(cursor)) != 0;
    }
  }
}
