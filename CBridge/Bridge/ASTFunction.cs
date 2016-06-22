using System.Collections.Generic;

namespace CBridge.Bridge
{
  public class ASTFunction
  {
    public ASTFunction()
    {
      Arguments = new List<ASTArgument>();
    }

    public string Name { get; set; }
    public string ReturnType { get; set; }
    public IList<ASTArgument> Arguments { get; set; }
  }
}
