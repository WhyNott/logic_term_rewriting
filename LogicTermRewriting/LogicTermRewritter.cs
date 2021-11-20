using System;
using System.Collections.Generic;

using LogicTermDataStructures;
using EmissionDataStructures;
using LogicTermSentence = LogicTermDataStructures.Sentence;
using EmissionSentence = EmissionDataStructures.Sentence;

using SemiUnify = EmissionDataStructures.Unify;
using SemiStructure = EmissionDataStructures.Structure;


namespace LogicTermRewriting
{
    public class Unify : LogicVerb {
        public Variable l_term {get; set;}
        public Variable r_term {get; set;}

        public Unify(Variable l_term, Variable r_term) {
            this.l_term = l_term;
            this.r_term = r_term;
        }

        public void Deconstruct(out Variable l_term, out Variable r_term) {
            l_term = this.l_term;
            r_term = this.r_term;
        }
    }

    public class Structure : LogicVerb {
        public Variable l_term {get; set;}
        public LogicTermSentence r_term {get; set;}

        public Structure(Variable l_term, LogicTermSentence r_term) {
            this.l_term = l_term;
            this.r_term = r_term;
        }

        public void Deconstruct(out Variable l_term, out LogicTermSentence r_term) {
            l_term = this.l_term;
            r_term = this.r_term;
        }
    }


    public class PredicateCallVariableExtracted : LogicVerb {
        public EmissionSentence sentence {get; set;}

        public PredicateCallVariableExtracted(EmissionSentence sentence){
            this.sentence = sentence;
        }

        public void Deconstruct(out EmissionSentence sentence){
            sentence = this.sentence;
        }

    }

    public class ClauseVariableExtracted {
        public EmissionSentence head {get; set;}
        public LogicVerb body {get; set;}
        public Context context {get; set;}
        public Variable[] variables {get; set;} 

        public ClauseVariableExtracted(EmissionSentence head, LogicVerb body, Context context, Variable[] variables) {
            this.head = head;
            this.body = body;
            this.context = context;
            this.variables = variables;
        }

        public void Deconstruct(out EmissionSentence head,  out LogicVerb body, out Context context, out Variable[] variables) {
            head =    this.head;
            body =    this.body;
            context = this.context;
            variables = this.variables;
        }
    }


    
    public class LogicTermRewriter {

        List<EmissionVerb> continuations = new List<EmissionVerb>();
        int conditions = 0;

        public Procedure rewrite(ClauseVariableExtracted input) {
            return new Procedure(
                input.head,
                input.variables,
                this.continuations.ToArray(),
                this.conditions,
                this.rewrite(input.body, true),
                input.context
            );

        }

        
        public EmissionVerb rewrite(LogicVerb input, bool last) {
            int next_cont = last ? 0 : this.continuations.Count;
            EmissionVerb output;
            switch (input) {
                case null:
                    output = new Succeed(next_cont);
                    break;
                case PredicateCallVariableExtracted p:
                    output = new Call(next_cont, p.sentence);
                    break;

                case Unify u:
                    output = new Conditional(next_cont, new SemiUnify(u.l_term, u.r_term));
                    break;

                case Structure s:
                    output = new Conditional(next_cont, new SemiStructure(s.l_term, s.r_term));
                    break;

                case Or or:
                    List<EmissionVerb> new_or = new List<EmissionVerb>();
                    foreach (LogicVerb lv in or.contents) {
                        new_or.Add(this.rewrite(lv, last));
                    }
                    output = new Disjunction(new_or.ToArray());
                    break;

                case IfElse ifel:
                    var (cond, then, lse) = ifel;
                    this.continuations.Add(new Disjunction(new EmissionVerb[] {
                                new SetCondition(this.conditions),
                                this.rewrite(then, last)
                            }));

                    var cond_and_then = this.rewrite(cond, false);
                    output = new Disjunction(new EmissionVerb[] {
                            cond_and_then,
                            new Conditional(this.continuations.Count,
                                            new CheckCondition(this.conditions))
                        });
                    this.continuations.Add(this.rewrite(lse, last));
                    this.conditions++;
                    break;

                case And a:
                    var first_element = a.contents[0];
                    for (int i = a.contents.Length - 1; i > 0; i--) {
                        var result = this.rewrite(a.contents[i], i == (a.contents.Length - 1));
                        this.continuations.Add(result);
                    }
                    output = this.rewrite(first_element, a.contents.Length == 1);
                    break;

                default:
                    output = null;
                    throw new InvalidOperationException();
            }
            return output;
        }
    }

    
    
    public class VariableExtractor {    

        private List<LogicVerb> new_body_verbs = new List<LogicVerb>();
        List<Variable> head_variables = new List<Variable>();
        
        //all the variables in the clause except the head ones
        HashSet<Variable> clause_variables = new HashSet<Variable>();

        
        public ClauseVariableExtracted extract_variables(Clause clause){

            for (int i = 0; i < clause.head.elements.Length; i++)
            {
                var element = clause.head.elements[i];
                switch (element)
                {
                    case LogicTermSentence _:
                        var new_variable = new Variable(
                            i.ToString(),
                            element.context,
                            true
                        );
                        this.new_body_verbs.Add(new Structure(new_variable,
                                                   (LogicTermSentence)element));
                        this.head_variables.Add(new_variable);
                        break;
                    case Variable variable:
                        variable.is_head = true;
                        this.head_variables.Add(variable);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

            }


            
            var new_body = this.extract_variables(clause.body);
            
            Variable[] new_variables = new Variable[this.clause_variables.Count];
            this.clause_variables.CopyTo(new_variables);
            return new ClauseVariableExtracted(
                new EmissionSentence(
                    clause.head.name,
                    this.head_variables.ToArray(),
                    clause.head.context
                ),
                new_body,
                clause.context,
                new_variables
            );
            
        }

        private void add_clause_variable(Variable v){
            if (!v.is_head) {
                this.clause_variables.Add(v);
            }
        }
        
        public LogicVerb extract_variables(LogicVerb verb) {
            switch (verb) {
                
                case null:
                    break;
                case PredicateCall p:
                    if (p.sentence.name == "{} = {}") {
                        var a = p.sentence.elements[0];
                        var b = p.sentence.elements[1];

                        if (a is Variable && b is Variable){
                            this.new_body_verbs.Add(new Unify(
                                                        (Variable)a, (Variable)b
                                                    ));
                            this.add_clause_variable((Variable)a);
                            this.add_clause_variable((Variable)b);
                        } else if (a is LogicTermSentence &&
                                   b is LogicTermSentence) {
                            var newvar_1 = new Variable(a.context, false);
                            var newvar_2 = new Variable(b.context, false);

                            this.new_body_verbs.Add(new Structure(
                                                        newvar_1, (LogicTermSentence)a
                                                    ));
                            this.new_body_verbs.Add(new Structure(
                                                        newvar_2, (LogicTermSentence)b
                                                    ));
                            this.new_body_verbs.Add(new Unify(
                                                        newvar_1, newvar_2
                                                    ));
                            this.add_clause_variable(newvar_1);
                            this.add_clause_variable(newvar_2);
                        } else {
                            var variable = a is Variable ? (Variable) a : (Variable) b;
                            var sentence = a is LogicTermSentence ? (LogicTermSentence) a :  (LogicTermSentence) b;
                            this.new_body_verbs.Add(new Structure(
                                                        variable, sentence
                                                    ));
                            this.add_clause_variable(variable);
                        }   
                    } else {
                        List<Variable> new_arguments = new List<Variable>();
                        foreach (Term element in p.sentence.elements) {
                            switch (element) {
                                case LogicTermSentence s:
                                    var newvar = new Variable(s.context, false);
                                    new_arguments.Add(newvar);
                                    this.add_clause_variable(newvar);
                                    this.new_body_verbs.Add(new Structure(newvar, s));
                                    break;
                                case Variable v:
                                    new_arguments.Add(v);
                                    this.add_clause_variable(v);
                                    break;
                            }
                        }
                        this.new_body_verbs.Add(new PredicateCallVariableExtracted(
                                                    new EmissionSentence(
                                                        p.sentence.name,
                                                        new_arguments.ToArray(),
                                                        p.sentence.context
                                                    )
                                                ));
                    }
                    break;
                
                case And a:
                    {
                        VariableExtractor ve = new VariableExtractor();
                        List<LogicVerb> lvs = new List<LogicVerb>();
                        
                        foreach (LogicVerb lv in a.contents){
                            var new_lv = ve.extract_variables(lv);
                            if (new_lv is And) {
                                lvs.AddRange(((And)new_lv).contents);
                            } else {
                                lvs.Add(new_lv);
                            }
                        }
                        this.new_body_verbs.Add(new And(ve.new_body_verbs.ToArray()));
                        this.clause_variables.UnionWith(ve.clause_variables);
                    }
                    break;
                    
               
                case Or or:
                    {
                        List<LogicVerb> lvs = new List<LogicVerb>();
                        VariableExtractor ve = new VariableExtractor();
                        foreach (LogicVerb lv in or.contents) {
                            
                            lvs.Add(ve.extract_variables(lv));
                            
                        }
                        this.new_body_verbs.Add(new Or(lvs.ToArray()));
                        this.clause_variables.UnionWith(ve.clause_variables);
                    }
                    break;

                case IfElse ifel:
                    {
                        var (cond, then, lse) = ifel;
                        VariableExtractor ve = new VariableExtractor();
                        this.new_body_verbs.Add(
                            new IfElse(
                                ve.extract_variables(cond),
                                ve.extract_variables(then),
                                ve.extract_variables(lse)
                            )
                        );
                        this.clause_variables.UnionWith(ve.clause_variables);
                    }
                    break;

                case Not not:
                    {
                        VariableExtractor ve = new VariableExtractor();
                        this.new_body_verbs.Add(new Not(ve.extract_variables(not.contents)));
                        this.clause_variables.UnionWith(ve.clause_variables);
                    }
                    break;
                    

            }


            var retval = this.new_body_verbs.Count switch {
                0 => null,
                1 => this.new_body_verbs[0],
                _ => new And(this.new_body_verbs.ToArray()),
            };
            this.new_body_verbs.Clear();
            return retval;

        }

        
    }



    
}
