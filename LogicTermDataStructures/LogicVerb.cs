using System.ComponentModel;
using System;
using System.Collections;
namespace LogicTermDataStructures {
    public abstract class LogicVerb {
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
            //    Console.WriteLine(contents.Length);
            //      Console.WriteLine(System.Environment.StackTrace);
        }

        public void Deconstruct(out LogicVerb[] contents){
            contents = this.contents;
        }

       // public override string ToString(){
       //     var str = this.GetType().Name;
       //     str += "(";
       //     foreach (var item in this.contents) {
       //         str += item.ToString();
       //     }
       //     str += ")";
       //     return str;
       // }

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
        public LogicVerb condition_contents {get; set;}
        public LogicVerb if_contents {get; set;}
        public LogicVerb else_contents {get; set;}

        public IfElse(LogicVerb condition_contents,
                      LogicVerb if_contents, LogicVerb else_contents){
            this.condition_contents = condition_contents;
            this.if_contents = if_contents;
            this.else_contents = else_contents;
        }

        public void Deconstruct(out LogicVerb condition_contents,
                                out LogicVerb if_contents,
                                out LogicVerb else_contents){
            condition_contents = this.condition_contents;
            if_contents        = this.if_contents;
            else_contents      = this.else_contents;
        }

    }
    
    public class Not : LogicVerb {
        public LogicVerb contents {get; set;}

        public Not(LogicVerb contents){
            this.contents = contents;
        }

        public void Deconstruct(out LogicVerb contents){
            contents = this.contents;
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

        public override string ToString(){
            return this.head.ToString() + (this.body is null ? "" : ":-" + this.body);
        }

        public void Deconstruct(out Sentence head,  out LogicVerb body, out Context context) {
            head =    this.head;
            body =    this.body;
            context = this.context;
        }
    }


}
