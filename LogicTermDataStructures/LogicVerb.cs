namespace LogicTermDataStructures {
    public abstract class LogicVerb {}

    public class PredicateCall : LogicVerb {
        public Sentence sentence {get; set;}

        public PredicateCall(Sentence sentence){
            this.sentence = sentence;
        }

        public void Deconstruct(out Sentence sentence){
            sentence = this.sentence;
        }

    }

    public class And : LogicVerb {
        public LogicVerb[] contents {get; set;}

        public And(LogicVerb[] contents){
            this.contents = contents;
        }

        public void Deconstruct(out LogicVerb[] contents){
            contents = this.contents;
        }

    }
    public class Or : LogicVerb {
        public LogicVerb[] contents {get; set;}

        public Or(LogicVerb[] contents){
            this.contents = contents;
        }

        public void Deconstruct(out LogicVerb[] contents){
            contents = this.contents;
        }

    }
    
    public class IfElse : LogicVerb {
        public LogicVerb[] condition_contents {get; set;}
        public LogicVerb[] if_contents {get; set;}
        public LogicVerb[] else_contents {get; set;}

        public IfElse(LogicVerb[] condition_contents,
                      LogicVerb[] if_contents, LogicVerb[] else_contents){
            this.condition_contents = condition_contents;
            this.if_contents = if_contents;
            this.else_contents = else_contents;
        }

        public void Deconstruct(out LogicVerb[] condition_contents,
                                out LogicVerb[] if_contents,
                                out LogicVerb[] else_contents){
            condition_contents = this.condition_contents;
            if_contents        = this.if_contents;
            else_contents      = this.else_contents;
        }

    }
    
    public class Not : LogicVerb {
        public LogicVerb[] contents {get; set;}

        public Not(LogicVerb[] contents){
            this.contents = contents;
        }

        public void Deconstruct(out LogicVerb[] contents){
            contents = this.contents;
        }

    }
    public class Unify : LogicVerb {
        public Term l_term {get; set;}
        public Term r_term {get; set;}

        public Unify(Term l_term, Term r_term) {
            this.l_term = l_term;
            this.r_term = r_term;
        }

        public void Deconstruct(out Term l_term, out Term r_term) {
            l_term = this.l_term;
            r_term = this.r_term;
        }
    }

    public class Clause {
        public Sentence head {get; set;}
        public LogicVerb body {get; set;}
        public Context context {get; set;}

        public Clause(Sentence head, LogicVerb body, Context context) {
            this.head = head;
            this.body = body;
            this.context = context;
        }

        public void Deconstruct(out Sentence head,  out LogicVerb body, out Context context) {
            head =    this.head;
            body =    this.body;
            context = this.context;
        }
    }


}
