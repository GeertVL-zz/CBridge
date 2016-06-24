using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CToString
{
  static class Program
  {

    static void Main(string[] args)
    {
      char[] result;
      using (var reader = File.OpenText(args[0]))
      {
        result = new char[reader.BaseStream.Length];
        reader.Read(result, 0, (int)reader.BaseStream.Length);
      }

      var tokenizer = new Tokenizer();
      tokenizer.Invoke(result);

      Console.WriteLine(string.Join(",", tokenizer.Tokens));

      Console.ReadKey();
    }
  }

  public class CNode
  {
    internal CNode(string value, NodeType cNodeType)
    {
      Value = value;
      CNodeType = cNodeType;
      Children = new List<CNode>();
    }

    public string Value { get; set; }
    internal NodeType CNodeType { get; set; }
    public IList<CNode> Children { get; set; }
  }

  public class Parser
  {
    internal void Invoke(IList<CToken> tokens)
    {
      int count = 0;
      var callerToken = new CNode(tokens[count].Value, tokens[count].CNodeType);
      count++;
      count++;


    }
  }
}
