namespace CToString
{
  internal class CToken
  {
    public CToken(string value, NodeType cNodeType)
    {
      Value = value;
      CNodeType = cNodeType;
    }

    public string Value { get; set; }
    public NodeType CNodeType { get; set; }
  }
}