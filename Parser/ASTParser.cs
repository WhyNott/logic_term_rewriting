using System;
using System.Collections.Generic;
using System.Text;
using LogicTermDataStructures;
using TermVariable = LogicTermDataStructures.Variable;
using System.Diagnostics;
using Element = System.Object;



namespace Parser {

    public abstract class Symbol {
        public abstract void process(ASTParser parser);
    }

    //Terminals
    
    public class Terminal : Symbol {
        Token terminal;
        
        public Terminal(Token t) {
            this.terminal = t;
        }
        public override void process(ASTParser p) {
            var t = p.tokens[p.index];
            if (t.GetType() == this.terminal.GetType()){
                p.index++;
            } else {
                throw new SyntaxErrorException(
                            String.Format("Unexpected token {0} at line {1}:{2} \n (Expected {3} here instead.)",
                                          t.ToString(), t.line_count, t.column_count, this.terminal));
                
            }


        }
    }

    public class TermSymbol : Symbol {
        public override void process(ASTParser p){
            var t = p.tokens[p.index];
            if (t is Variable) {
                var v = (Variable) t;
                p.ast.Push(new TermVariable(v.content,
                                            new Context(
                                                v.line_count, v.column_count, v.filename)));
                    
            } else if (t is BeginSentence) {
                p.ast.Push(p.parse_sentence());
            } else {
                throw new SyntaxErrorException(
                    String.Format("Unexpected token {0} at line {1}:{2} \n (Expected a map, term or a list here instead.)",
                                  t.ToString(), t.line_count, t.column_count));    
            }
            p.index++;
        }
    }


    //Non-terminals
    public class ElementSymbol : Symbol {
        public override void process(ASTParser p) {
            var t = p.tokens[p.index];
            if (t is Indent) {
                var t1 = p.tokens[p.index+1];
                // is a list
                if (t1 is Dash) {
                    p.symbols.Push(new ListSymbol());
                } else if (t1 is Variable || t1 is BeginSentence) {
                    p.symbols.Push(new MapSymbol());
                } else {
                    throw new SyntaxErrorException(
                            String.Format("Unexpected token {0} at line {1}:{2} \n (Expected a map or a list here instead.)",
                                          t1.ToString(), t1.line_count, t1.column_count));
                }

            } else {
                p.symbols.Push(new TermSymbol());
                
            }
        }
    }

    public class ListSymbol : Symbol {
        public override void process(ASTParser p){
            p.ast.Push(new List<Element>());

            p.symbols.Push(new Terminal(new Indent()));
            p.symbols.Push(new Terminal(new Dash()));               
            p.symbols.Push(new ElementSymbol());
            p.symbols.Push(new ListItemSymbol());
            p.symbols.Push(new Terminal(new Dedent()));
            
        }

    }

    public class ListItemSymbol : Symbol{
        public override void process(ASTParser p){
            Element prev_item = p.ast.Pop();
            List<Element> curr_list = (List<Element>)p.ast.Peek();
            curr_list.Add(prev_item);
            if (p.tokens[p.index] is Dash) {
                p.symbols.Push(new Terminal(new Dash()));
                p.symbols.Push(new ElementSymbol());
                p.symbols.Push(new ListItemSymbol());
            } 
        }
    }

    public class MapSymbol : Symbol {
        public override void process(ASTParser p){
            p.ast.Push(new List<(Term, Element)>());
            p.symbols.Push(new Terminal(new Indent()));
            p.symbols.Push(new TermSymbol());
            p.symbols.Push(new Terminal(new Semicolon()));
            p.symbols.Push(new ElementSymbol());
            p.symbols.Push(new MapItemSymbol());
            p.symbols.Push(new Terminal(new Dedent()));
            
        }

    }

    public class MapItemSymbol : Symbol{
        public override void process(ASTParser p){
            Element prev_item = p.ast.Pop();
            Term prev_key = (Term) p.ast.Pop();
            List<(Term, Element)> curr_list = (List<(Term, Element)>)p.ast.Peek();
            curr_list.Add((prev_key, prev_item));
            if (p.tokens[p.index] is Variable || p.tokens[p.index] is BeginSentence) {
                p.symbols.Push(new TermSymbol());
                p.symbols.Push(new Terminal(new Semicolon()));
                p.symbols.Push(new ElementSymbol());
                p.symbols.Push(new MapItemSymbol());
            } 
        }
    }
    
    




    public class ASTParser {

        public Token[] tokens;
        public int index;
        public Stack<Symbol> symbols = new Stack<Symbol>();
        public Stack<Element> ast = new Stack<Element>();


        public List<(Term, Element)> parse_document() {
            this.symbols.Push(new MapItemSymbol());
            this.ast.Push(new List<(Term, Element)>());
            while (this.symbols.Count > 0) {
                Symbol s = this.symbols.Pop();
                s.process(this);
            }

            var result = this.ast.Pop();
            Debug.Assert(this.ast.Count == 0);

            return (List<(Term, Element)>)result;
        }

        public Sentence parse_sentence() {
            var opening_token = this.tokens[this.index];
            Debug.Assert(opening_token is BeginSentence);

            Context sentence_context = new Context(
                opening_token.line_count,
                opening_token.column_count,
                opening_token.filename
            );

            this.index++;

            StringBuilder sentence_name = new StringBuilder();
            List<Term> elements = new List<Term>();

            
            for (; this.index < this.tokens.Length; this.index++){
                switch (this.tokens[this.index]) {
                    case SentencePiece sp:
                        {
                            if (sentence_name.Length != 0)
                            {
                                sentence_name.Append(' ');
                            }
                            sentence_name.Append(sp.content);
                        }
                        break;
                    case SentencePieceVerbatim spv:
                        {
                            sentence_name.Append(spv.content);
                        }
                        break;

                    case Variable v:
                        {
                            if (sentence_name.Length != 0)
                            {
                                sentence_name.Append(' ');
                            }
                            sentence_name.Append("{}");
                            Context variable_context = new Context(
                                v.line_count,
                                v.column_count,
                                v.filename
                            );
                            
                            elements.Add(new TermVariable(v.content, variable_context));
                        }
                        break;

                    case BeginSentence b:
                        {
                            if (sentence_name.Length != 0)
                            {
                                sentence_name.Append(' ');
                            }
                            sentence_name.Append("{}");
                            
                            
                            elements.Add(this.parse_sentence());
                        }
                        break;
                        
                    case EndSentence es:
                        return new Sentence(
                            sentence_name.ToString(),
                            elements.ToArray(),
                            sentence_context
                        );
                        
                    default:
                        var t = this.tokens[this.index];
                        throw new SyntaxErrorException(
                            String.Format("Unexpected token {0} in sentence at line {1}:{2}",
                                          t.ToString(), t.line_count, t.column_count));
                

                }
            
            
            }

            Debug.Fail("Unreachable code reached");
            throw new InvalidOperationException();
            
        }
        



    }

}
