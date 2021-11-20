using System;
using LogicTermSentence = LogicTermDataStructures.Sentence;
using EmissionSentence = EmissionDataStructures.Sentence;

using LogicVerb = LogicTermDataStructures.LogicVerb;
using Context = LogicTermDataStructures.Context;
using Variable = LogicTermDataStructures.Variable;
using Clause = LogicTermDataStructures.Clause;

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

        public ClauseVariableExtracted(Sentence head, LogicVerb body, Context context, Variable[] variables) {
            this.head = head;
            this.body = body;
            this.context = context;
            this.variables = variables;
        }

        public void Deconstruct(out Sentence head,  out LogicVerb body, out Context context, out Variable[] variables) {
            head =    this.head;
            body =    this.body;
            context = this.context;
            variables = this.variables;
        }
    }


    
    public class LogicTermRewriter {
        public EmissionSentence rewrite(LogicTermSentence input) {
            throw new NotImplementedException("Not implemented.");
        }
    }
    
    public class VariableExtractor {    

        List<LogicVerb> new_body_verbs = new List<LogicVerb>;
        List<Variable> head_variables = new List<Variable>;
        
        //all the variables in the clause except the head ones
        HashSet<Variable> clause_variables = new HashSet<Variable>;

        
        public ClauseVariableExtracted extract_variables(Clause clause){
            
            for (int i; i < clause.head.elements.Length, i++) {
                var element = clause.head.elements[i];
                switch(element) {
                    case LogicTermSentence _:
                        var new_variable = new Variable(
                            i.toString(),
                            element.context,
                            true
                        );
                        this.new_body_verbs.Add(new Structure(new_variable,
                                                   (LogicTermSentence) element));
                        this.head_variables.Add(new_variable);
                        break;
                    case Variable variable:
                        variable.head = true;
                        this.head_variables.Add(variable);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

            }

            
            if (clause.body != null) {
                this.extract_variables(clause.body);
            }
            
            LogicVerb new_body = this.new_body_verbs.Length switch {
                0 => null,
                1 => this.new_body_verbs[0],
                _ => new And(this.new_body_verbs.ToArray());
            };
            return new ClauseVariableExtracted(
                new EmissionSentence(
                    clause.head.name,
                    this.head_variables,
                    clause.head.context
                ),
                new_body,
                clause.context,
                this.clause_variables
            );
            
        }

        private void add_clause_variable(Variable v){
            if (!v.is_head) {
                this.clause_variables.Add(v);
            }
        }
        
        public void extract_variables(LogicVerb verb) {
            switch (verb) {
                //Ok, so what are the kinds of values I can expect to experience at this stage?
                
                case PredicateCall p:
                    if (p.sentence.name == "{} = {}") {
                        var a = p.sentence.elements[0];
                        var b = p.sentence.elements[1];

                        if (typeof(a) == Variable && typeof(b) == Variable){
                            this.new_body_verbs.Add(new Unify(
                                                        (Variable)a, (Variable)b
                                                    ));
                            this.add_clause_variable((Variable)a);
                            this.add_clause_variable((Variable)b);
                        } else if (typeof(a) == LogicTermSentence &&
                                   typeof(b) == LogicTermSentence) {
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
                            var variable = typeof(a) == Variable ? (Variable) a : (Variable) b;
                            var sentence = typeof(a) == LogicTermSentence ? (LogicTermSentence) a :  (LogicTermSentence) b;
                            this.new_body_verbs.Add(new Structure(
                                                        variable, sentence
                                                    ));
                            this.add_clause_variable(variable);
                        }   
                    } else {
                        //TODO: all this stuff
                    }
                    break;
                //And
                //Or
                //IfElse
                //Not
                //


            }

        }
    }

    
}
