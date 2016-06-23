namespace CBridge.Bridge
{
  public class ASTArgument
  {
    public ASTArgument()
    {
      
    }

    public ASTArgument(string name, string varType)
    {
      Name = name;
      VarType = varType;
    }

    public string Name { get; set; }
    public string VarType { get; set; }
  }
}