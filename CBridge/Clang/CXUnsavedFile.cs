using System.Runtime.InteropServices;

namespace CBridge.Clang
{
  public partial struct CXUnsavedFile
  {
    [MarshalAs(UnmanagedType.LPStr)]
    public string @Filename;
    [MarshalAs(UnmanagedType.LPStr)]
    public string @Contents;
    public int @Length;
  }
}