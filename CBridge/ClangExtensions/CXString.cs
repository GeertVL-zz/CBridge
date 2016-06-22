using System.Runtime.InteropServices;

namespace CBridge.Clang
{
  public partial struct CXString
  {
    public override string ToString()
    {
      string retval = Marshal.PtrToStringAnsi(this.data);
      ClangInvoker.disposeString(this);
      return retval;
    }
  }
}