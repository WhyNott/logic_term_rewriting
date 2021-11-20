using Variable = LogicTermDataStructures.Variable;
using Context = LogicTermDataStructures.Context;
using LogicTermSentence = LogicTermDataStructures.Sentence;

using ContNum = System.Int32;
using BoolNum = System.Int32;

namespace EmissionDataStructures {
    public class Sentence{
        public string name {get; set;}
        public Context context {get; set;}
        public Variable[] elements {get; set;}

        public Sentence(string name, Variable[] elements, Context context){
            this.name = name;
            this.context = context;
            this.elements = elements; 
        }
        
    }

    public abstract class Semidet {

    }

    public class Unify : Semidet {
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

    public class Structure : Semidet {
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

    public class CheckCondition : Semidet {
        public BoolNum condition {get; set;}

        public CheckCondition(BoolNum condition){
            this.condition = condition;
        }

        public void Deconstruct(out BoolNum condition){
            condition = this.condition;
        }
    }

    
    
    public abstract class EmissionVerb {
    }

    public class Fail : EmissionVerb {}

    public class Succeed : EmissionVerb {
        public ContNum next {get; set;}

        public Succeed(ContNum next){
            this.next = next;
        }

        public void Deconstruct(out ContNum next){
            next = this.next;
        }
    }


    public class Or : EmissionVerb {
        public EmissionVerb[] arguments {get; set;}

        public Or(EmissionVerb[] arguments){
            this.arguments = arguments;
        }

        public void Deconstruct(out EmissionVerb[] arguments){
            arguments = this.arguments;
        }
    }

    public class SetCondition : EmissionVerb {
        public BoolNum condition {get; set;}

        public SetCondition(BoolNum condition){
            this.condition = condition;
        }

        public void Deconstruct(out BoolNum condition){
            condition = this.condition;
        }
    }


    public class Call : EmissionVerb {
        public Sentence sentence {get; set;}
        public ContNum next {get; set;}

        public Call(ContNum next, Sentence sentence){
            this.sentence = sentence;
            this.next     = next;
        }

        public void Deconstruct(out ContNum next, out Sentence sentence){
            sentence = this.sentence;
            next     = this.next;
        }
    }
    
    
    public class Conditional : EmissionVerb {
        public Semidet semidet {get; set;}
        public ContNum next {get; set;}

        public Conditional(ContNum next, Semidet semidet){
            this.semidet = semidet;
            this.next    = next;
        }

        public void Deconstruct(out ContNum next, out Semidet semidet){
            semidet = this.semidet;
            next    = this.next;
        }
    }


    public class Procedure {
        public Sentence head                {get; set;}
        public Variable[] variables         {get; set;}
        public EmissionVerb[] continuations {get; set;}
        public EmissionVerb body            {get; set;}
        public Context context              {get; set;}

        public Procedure(Sentence       head,
                         Variable[]     variables,
                         EmissionVerb[] continuations,
                         EmissionVerb   body,
                         Context        context){
            this.head           = head;          
            this.variables      = variables;     
            this.continuations  = continuations; 
            this.body           = body;
            this.context = context;

        }
    }
}
