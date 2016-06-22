namespace CBridge.Clang
{
  public partial struct CXCursor
  {
    public override string ToString()
    {
      return ClangInvoker.getCursorSpelling(this).ToString();
    }
  }
}
