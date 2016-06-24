using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToString
{
  public class Tokenizer
  {
    internal IList<CToken> Tokens { get; set; }

    public void Invoke(char[] code)
    {
      Tokens = new List<CToken>();
      var currentToken = new StringBuilder();
      // todo remove whitespace and comments


      foreach (char c in code)
      {
        if (char.IsWhiteSpace(c))
        {
          continue;
        }
        switch (c)
        {
          case Punctuations.OPEN_PARENS:
            Tokens.Add(new CToken(currentToken.ToString(), NodeType.Caller));
            Tokens.Add(new CToken(Punctuations.OPEN_PARENS.ToString(), NodeType.Punctuation));
            currentToken = new StringBuilder();
            break;
          case Punctuations.CLOSE_PARENS:
            Tokens.Add(new CToken(currentToken.ToString(), NodeType.Variable));
            Tokens.Add(new CToken(Punctuations.CLOSE_PARENS.ToString(), NodeType.Punctuation));
            currentToken = new StringBuilder();
            break;
          case Punctuations.COMMA:
            if (currentToken.Length != 0)
            {
              Tokens.Add(new CToken(currentToken.ToString(), NodeType.Variable));
            }
            Tokens.Add(new CToken(Punctuations.COMMA.ToString(), NodeType.Punctuation));
            currentToken = new StringBuilder();
            break;
          case Punctuations.DOUBLE_QUOTE:
            if (currentToken.Length == 0)
            {
              Tokens.Add(new CToken(Punctuations.DOUBLE_QUOTE.ToString(), NodeType.Punctuation));
            }
            else
            {
              Tokens.Add(new CToken(currentToken.ToString(), NodeType.String));
              Tokens.Add(new CToken(Punctuations.DOUBLE_QUOTE.ToString(), NodeType.Punctuation));
              currentToken = new StringBuilder();
            }
            break;
          case Punctuations.SEMI_COL:
            {
              Tokens.Add(new CToken(Punctuations.SEMI_COL.ToString(), NodeType.Punctuation));
              break;
            }
        }
        if (char.IsLetterOrDigit(c) || (Tokens.Last() != null && Tokens.Last().Value == "\"" && c != '"'))
        {
          currentToken.Append(c);
        }
      }
    }
  }
}
