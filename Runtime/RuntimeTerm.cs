using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
namespace Runtime {

    public class RuntimeTerm {
        public int id;
        private bool model = false;
        public RuntimeTerm variable_value = null;
        public string name;
        public RuntimeTerm[] arguments = {};
        public int? copy_num = null;

        public override string ToString(){
            StringBuilder representation = new StringBuilder();
            var term = this.dereferenced();
            if (term.is_variable()){
                representation.Append(term.name);
            } else if (term.is_atom()) {
                representation.Append("<");
                representation.Append(term.name);
                representation.Append(">"); 
            } else {
                representation.Append("<");
                int i = 0;
                foreach (char c in term.name) {
                    if (c == '}')
                        continue;

                    if (c == '{') {
                        representation.Append(
                            term.arguments[i].ToString()
                        );
                        i++;
                    } else {
                        representation.Append(c);
                    }
                   
                }
                representation.Append(">");
                
            }
            
            return representation.ToString();
        }

        public static RuntimeTerm make_empty_variable(
            string name, int? copy_num){
            var term = new RuntimeTerm();
            term.id = Trail.global_id_counter++;
            term.variable_value = term;
            term.name = name;
            term.copy_num = copy_num;
            return term;
        }

        public static RuntimeTerm make_atom(string name, bool model){
            var term = new RuntimeTerm();
            term.id = Trail.global_id_counter++;
            term.name = name;
            term.model = model;
            return term;
        }

        public static RuntimeTerm make_structured_term(
            string functor, RuntimeTerm[] arguments, bool model){
            var term = new RuntimeTerm();
            term.id = Trail.global_id_counter++;
            term.name = functor;
            term.model = model;
            term.arguments = arguments;
            return term;
        }
        

        public bool is_variable(){
            return this.variable_value != null;
        }

        public bool is_model(){
            return this.model;
        }

        public bool is_bound(){
            return this.variable_value != this;
        }

        public bool is_atom(){
            return !this.is_variable() && (this.arguments == null || this.arguments.Length == 0);
        }

        public bool is_copy(){
            return this.copy_num != null;
        }

        //note: this function could get stuck in an infinite loop if there is
        //a circular binding
        //however, since we perform unification without occurs check,
        //this is already something that would break stuff, so its probably ok
        public RuntimeTerm dereferenced(){
            RuntimeTerm term = this;
            while (term.is_variable() && term.is_bound()) {
                term = term.variable_value;
            }
            return term;
        }

        public RuntimeTerm backup_value(){
            if (this.is_variable() && this.variable_value.is_copy()){
                return this.id == this.variable_value.copy_num ? this : this.variable_value;
            } else {
                return this.variable_value;
            }
        }

        public RuntimeTerm content(){
            if (this.is_variable()){
                return this.variable_value;
            } else {
                return this;
            }

        }

        public void bind(RuntimeTerm sq){
            Debug.Assert(this.is_variable());
            Debug.Assert(!this.is_bound());
            if (this.id < Trail.last_id_counter.Peek()){
                Trail.add_postbinding(this);
            }
            sq = sq.dereferenced();
            
            //L1 binding
            if (sq.is_atom()){
                this.variable_value = sq;
            } 
            //L2 binding
            else if (sq.is_variable() && !sq.is_bound()){
                if (this.id < sq.id){ //this is older then sq
                    sq.variable_value = this.copy();
                } else {
                    this.variable_value = sq.copy();
                }            
            } 
            //L3 binding
            else if (sq.is_model()){
                this.variable_value = sq.copy();
            } else {
                this.variable_value = sq;
            }
            
        }

        public RuntimeTerm copy(){
            if (this.is_atom()){
                return this;
            } else if (this.is_variable()){
                var a = this.dereferenced();
                if (a.is_variable() && !a.is_bound()){
                    var b =  RuntimeTerm.make_empty_variable(a.name, a.id);
                    b.variable_value = a;
                    return b;
                } else {
                    return a;
                }
            } else {
                List<RuntimeTerm> new_args = new List<RuntimeTerm>();
                foreach (var val in this.variable_value.arguments){
                    new_args.Add(val.copy());
                }
                var term = RuntimeTerm.make_structured_term(
                    this.variable_value.name, new_args.ToArray(), false);
                term.copy_num = term.id;
                return term;
            }
            
        }

        public bool unify_with(RuntimeTerm other){
            var x = this.dereferenced();
            var y = other.dereferenced();
                        
            if (x.is_atom() && y.is_atom()){
                return x.name == y.name;
            } else if (x.is_variable() && !x.is_bound()) {
                x.bind(y);
                return true;
            } else if (y.is_variable() && !y.is_bound()) {
                y.bind(x);
                return true;
            } else if (x.variable_value == y.variable_value){
                return true;
            } else {
                if (x.variable_value.name != y.variable_value.name ||
                    x.variable_value.arguments.Length != y.variable_value.arguments.Length){
                    return false;
                } else {
                    for (var i = 0; i < x.variable_value.arguments.Length; i++){
                        var result = x.variable_value.arguments[i]
                            .unify_with(y.variable_value.arguments[i]);
                        if (!result){
                            return false;
                        }
                    }
                    return true;  
                } 
            }

        }
         
            
    }

    

}

