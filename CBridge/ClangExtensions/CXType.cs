namespace CBridge.Clang
{
  public partial struct CXType
  {
    public override string ToString()
    {
      return ClangInvoker.getTypeSpelling(this).ToString();
    }
  }
}