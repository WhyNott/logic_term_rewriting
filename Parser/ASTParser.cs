using System;
using System.Collections.Generic;
using System.Linq;
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

            p.symbols.Push(new Terminal(new Dedent()));
            p.symbols.Push(new ListItemSymbol());
            p.symbols.Push(new ElementSymbol());
            p.symbols.Push(new Terminal(new Dash()));               
            p.symbols.Push(new Terminal(new Indent()));
            
            
            
            
            
        }

    }

    public class ListItemSymbol : Symbol{
        public override void process(ASTParser p){
            Element prev_item = p.ast.Pop();
            List<Element> curr_list = (List<Element>)p.ast.Peek();
            curr_list.Add(prev_item);
            if (p.tokens[p.index] is Dash) {
                p.symbols.Push(new ListItemSymbol());
                p.symbols.Push(new ElementSymbol());
                p.symbols.Push(new Terminal(new Dash()));
                
                
            } 
        }
    }

    public class MapSymbol : Symbol {
        public override void process(ASTParser p){
            p.ast.Push(new List<(Term, Element)>());

            p.symbols.Push(new Terminal(new Dedent()));
            p.symbols.Push(new MapItemSymbol());
            p.symbols.Push(new ElementSymbol());
            p.symbols.Push(new Terminal(new Semicolon()));
            p.symbols.Push(new TermSymbol());
            p.symbols.Push(new Terminal(new Indent()));
            
            
            
            
            
            
        }

    }

    public class MapItemSymbol : Symbol{
        public override void process(ASTParser p){
            Element prev_item = p.ast.Pop();
            Term prev_key = (Term) p.ast.Pop();
            List<(Term, Element)> curr_list = (List<(Term, Element)>)p.ast.Peek();
            curr_list.Add((prev_key, prev_item));
            if (p.tokens[p.index] is Variable || p.tokens[p.index] is BeginSentence) {

                p.symbols.Push(new MapItemSymbol());
                p.symbols.Push(new ElementSymbol());
                p.symbols.Push(new Terminal(new Semicolon()));
                p.symbols.Push(new TermSymbol());
            } 
        }
    }
    
    




    public class ASTParser {

        public Token[] tokens;
        public int index;
        public Stack<Symbol> symbols = new Stack<Symbol>();
        public Stack<Element> ast = new Stack<Element>();


        public ASTParser(Token[] tokens){
            this.tokens = tokens;
            this.index = 0;
        }

        public void parse_document() {
            this.symbols.Push(new MapSymbol());
            
            while (this.symbols.Count > 0) {
                Symbol s = this.symbols.Pop();
                s.process(this);
            }

            
            Debug.Assert(this.ast.Count == 1);

        }

        public List<Clause> ast_to_clauses(){
            var predicate_section = (List<(Term, Element)>) this.ast.Pop();

            (var predicate, var clauses) = predicate_section[0];

            var ret_clauses = new List<Clause>();

            foreach (Element clause in (List<Element>) clauses) {
                switch (clause) {
                    case TermVariable v:
                        throw new SyntaxErrorException(
                            String.Format("Unexpected variable {0} inside a 'Predicates' section at line {1}:{2} \n Hint: a predicate clause should be here instead",
                                          v.name,
                                          v.context.line_number,
                                          v.context.column_number));
                    case Sentence s:
                        ret_clauses.Add(new Clause(s, null, s.context));
                        break;
                        
                    case List<(Term, Element)> map:
                        (var head, var body) = map[0];
                        if (head is TermVariable) {
                            throw new SyntaxErrorException(
                            String.Format("Unexpected variable {0} inside a 'Predicates' section at line {1}:{2} \n Hint: a predicate clause should be here instead",
                                          ((TermVariable)head).name,
                                          head.context.line_number,
                                          head.context.column_number));
                        }


                        ret_clauses.Add(new Clause((Sentence) head,
                                                   this.element_to_logic_verb(body),
                                                   head.context));
                        break;
                }
            }

            return ret_clauses;

        }

        
        private LogicVerb element_to_logic_verb(Element e){
            switch (e) {
                case Term t:
                    switch (t) {
                        case TermVariable v:
                            throw new SyntaxErrorException(
                                String.Format("Unexpected variable {0} inside a logic verb at line {1}:{2} \n Hint: a predicate call should be here instead",
                                              v.name,
                                              v.context.line_number,
                                              v.context.column_number));
                            
                        case Sentence s:
                            return new PredicateCall(s);
                            
                    }
                    break;
                    
                case List<(Term, Element)> map:
                    (var term, var element) = map[0];
                    if (term is TermVariable)
                        throw new SyntaxErrorException(
                                String.Format("Unexpected variable {0} inside a logic verb at line {1}:{2} \n Hint: a logic verb such as 'or', 'and' or 'if'/'else' should be here instead",
                                              ((TermVariable)term).name,
                                              term.context.line_number,
                                              term.context.column_number));
                    else {
                        switch (((Sentence)term).name) {
                            case "and":
                                return this.element_to_logic_verb(element);
                                
                                
                            case "or":
                                if (element is List<Element>) {
                                    var disjlist = new List<LogicVerb>();
                                    foreach (Element el in (List<Element>) element) {
                                        disjlist.Add(this.element_to_logic_verb(el));
                                    }
                                    return new Or(disjlist.ToArray());
                                } else {
                                    //a disjunction of a single item is equivalent to a single item
                                    return this.element_to_logic_verb(element);
                                }

                            case "if":
                                var condition = this.element_to_logic_verb(element);
                                (var then, var if_element) = map[1];
                                Debug.Assert(((Sentence) then).name == "then");
                                var if_content = this.element_to_logic_verb(if_element);
                                LogicVerb else_content;
                                if (map.Count > 2) {
                                    (var @else, var else_element) = map[2];
                                    Debug.Assert(((Sentence) @else).name == "else");
                                    else_content = this.element_to_logic_verb(else_element); 
                                } else {
                                    else_content = null;
                                }

                                return new IfElse(condition, if_content, else_content);

                            case "not":
                                return new Not(this.element_to_logic_verb(element));

                                
                            default:
                                throw new SyntaxErrorException(
                                    String.Format("Unsupported logic verb {0} at line {1}:{2} \n Hint: A logic verb can be one of 'or', 'and', 'not' or 'if'/'else'",
                                                  ((Sentence)term).name,
                                                  term.context.line_number,
                                                  term.context.column_number));

                        }

                    }
                    

                    //we assume a list without an explicitly specified logic verb 
                    //is a logical conjunction (an 'and' statement)
                case List<Element> list:
                    var retlist = new List<LogicVerb>();
                    foreach (Element el in list) {
                        retlist.Add(this.element_to_logic_verb(el));
                    }
                    return new And(retlist.ToArray());

                    
                default:
                    break;

            }
            
            //any other type should not be possible
            Debug.Fail("Unreachable code reached");
            throw new InvalidOperationException();
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
        
        public static Dictionary<string, Clause> join_clauses_of_same_predicate(Clause[] clause_list){
            //aggregate the clauses by predicate
            Dictionary<string, List<Clause>> clauses =
                clause_list.Aggregate(new Dictionary<string, List<Clause>>(), (map, clause) => {
                    if (!map.TryAdd(clause.head.name,
                                    new List<Clause>(
                                        new Clause[]{clause}
                                    ))
                    ) {
                        map[clause.head.name].Add(clause);
                    }
                    return map;
                });
            

            //map of values to be returned
            Dictionary<string, Clause> ret_map = new Dictionary<string, Clause>();

            //we iterate on every individual predicate
            foreach (KeyValuePair<string, List<Clause>> entry
                     in clauses) {
                //if there is only one clause of the predicate, there is nothing to join
                if (entry.Value.Count == 1) {
                    ret_map.Add(entry.Key, entry.Value[0]);
                    continue;
                }

                Sentence head = entry.Value[0].head;
                var predicate_clauses = new List<LogicVerb>();

                //if the head of the predicate has no arguments,
                //we just need to gather all the clause bodies
                if (head.elements.Length == 0) {
                    foreach (var clause in entry.Value){
                        if (clause.body is Or){
                            predicate_clauses.AddRange(
                                ((Or) clause.body).contents
                            );
                        } else if (clause.body != null) {
                            predicate_clauses.Add(clause.body);
                        }
                    }

                    ret_map.Add(entry.Key,
                                new Clause(
                                    head,
                                    new Or(predicate_clauses.ToArray()),
                                    head.context
                                )
                    );

                    continue;
                }

                //if predicate head has arguments, we need to
                //generate n fresh variables to replace them
                
                var head_variables = new List<TermVariable>();
                
                for (int i =0; i < head.elements.Length; i++){
                    head_variables.Add(
                        new TermVariable(head.context, true));
                }
                
                //iterate on all the clauses of a predicate
                foreach (var clause in entry.Value){
                    var new_clause_body = new List<LogicVerb>();
                    //for each clause, unify all arguments with
                    //the fresh variables in the predicate head
                    for (int i = 0; i < head.elements.Length; i++){
                        new_clause_body.Add(
                            new PredicateCall(
                                new Sentence(
                                    "{} = {}",
                                    new Term[] {
                                        head_variables[i],
                                        clause.head.elements[i]
                                    },
                                    clause.head.context
                                )
                            )
                        );
                        
                    }

                    if (clause.body is And){
                        new_clause_body.AddRange(
                            ((And) clause.body).contents
                        );
                    } else if (clause.body != null) {
                        new_clause_body.Add(clause.body);
                    }

                    predicate_clauses.Add(
                        new And(
                            new_clause_body.ToArray()
                        )
                    );

                }
                
                ret_map.Add(entry.Key,
                            new Clause(
                                new Sentence(
                                    head.name,
                                    head_variables.ToArray(),
                                    head.context
                                ),
                                new Or(
                                    predicate_clauses.ToArray()
                                ),
                                head.context
                            )
                );
                
                
            }

            return ret_map;

        }
        

    }

}
