namespace CBridge.Clang
{
  public enum CXChildVisitResult : uint
  {
    @CXChildVisit_Break = 0,
    @CXChildVisit_Continue = 1,
    @CXChildVisit_Recurse = 2,
  }
}