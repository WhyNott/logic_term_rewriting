using System.ComponentModel;
using System;
using Variable = LogicTermDataStructures.Variable;
using Context = LogicTermDataStructures.Context;
using LogicTermSentence = LogicTermDataStructures.Sentence;
using System.Collections;

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

        public override string ToString(){
            string str = this.name + "(";
            foreach (var element in this.elements) {
                str += element.ToString();
                str += ",";
            }
            return str + ")";
        }
        
    }

    public abstract class Semidet {

        public override string ToString(){
            var str = this.GetType().Name;
            str += "(";
            foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this)){
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Type value_type = value.GetType();
                str += value.ToString();
                
                str += ",";
            }
            str += ")";
            return str;
            

        }

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

        public override string ToString(){
            var str = this.GetType().Name;
            str += "(";
            foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this)){
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                Type value_type = value.GetType();
                
                if (value_type.IsArray) {
                    IEnumerable enumerable = value as IEnumerable;
                    foreach (var subob in enumerable) {
                        str += subob.ToString();
                        
                    }
                } else
                {
                    str += value.ToString();
                }
                str += ",";
            }
            str += ")";
            return str;
            

        }
        
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


    public class Disjunction : EmissionVerb {
        public EmissionVerb[] arguments {get; set;}

        public Disjunction(EmissionVerb[] arguments){
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
        public BoolNum conditions           {get; set;}
        public EmissionVerb body            {get; set;}
        public Context context              {get; set;}

        public Procedure(Sentence       head,
                         Variable[]     variables,
                         EmissionVerb[] continuations,
                         BoolNum        conditions,
                         EmissionVerb   body,
                         Context        context){
            this.head           = head;          
            this.variables      = variables;     
            this.continuations  = continuations;
            this.conditions = conditions;
            this.body           = body;
            this.context = context;

        }

        public override string ToString()
        {
            
            var str = this.head.ToString() + "{\n";
            str += "variables: [";
            foreach (var v in this.variables)
            {
                str += v.ToString();
            }
            str += "];\n";

            str += "continuations: [";
            foreach (var v in this.continuations)
            {
                str += v.ToString();
            }
            str += "];\n";

            str += "number of conditions: " + this.conditions + ";\n";

            return str + (this.body is null ? "" :  this.body + ";\n") +"}";
    }

        
    }
}
