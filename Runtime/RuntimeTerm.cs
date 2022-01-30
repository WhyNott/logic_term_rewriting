namespace Runtime {

    public class RuntimeTerm {
        public int id;
        private bool model = false;
        public RuntimeTerm variable_value = null;
        public String name;
        public RuntimeTerm[] arguments = [];
        public int? copy_num = null;

        public static RuntimeTerm make_empty_variable(
            String name, int? copy_num){
            var term = new RuntimeTerm();
            term.id = Trail.global_id_counter++;
            term.variable_value = term;
            term.name = name;
            term.copy_num = copy_num;
            return term;
        }

        public static RuntimeTerm make_atom(String name, bool model){
            var term = new RuntimeTerm();
            term.id = Trail.global_id_counter++;
            term.name = name;
            term.model = model;
            return term;
        }

        public static RuntimeTerm make_structured_term(
            String functor, RuntimeTerm[] arguments, bool model){
            var term = new RuntimeTerm();
            term.id = Trail.global_id_counter++;
            term.name = name;
            term.model = model;
            return term;
        }
        

        public bool is_variable(){
            return this.variable_value != null;
        }

        public bool is_model(){
            return this.variable;
        }

        public bool is_bound(){
            return this.variable_value != this;
        }

        public bool is_atom(){
            return (this.arguments == null || this.arguments.Length == 0);
        }

        public bool is_copy(){
            return this.copy_num != null;
        }

        public RuntimeTerm dereferenced(){
            if (this.is_variable() && this.is_bound()) {
                return this.variable_value.dereferenced_value();
            } else {
                return this;
            }
        }

        public RuntimeTerm dereferenced_value(){
            if (this.is_variable()){
                return this.dereferenced();
            } else {
                return this;
            }
        }

        public RuntimeTerm backup_value(){
            if (this.is_variable() && this.value.is_copy()){
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
                trail.add_postbinding(this);
            }
            
            //L1 binding
            if (sq.is_atom()){
                this.variable_value = sq;
            } 
            //L2 binding
            else if (sq.is_variable() && !sq.bound()){
                if (this.id < sq.id){ //this is older then sq
                    sq.value = this.copy();
                } else {
                    this.value = sq.copy();
                }            
            } 
            //L3 binding
            else if (sq.is_model()){
                this.value = sq.copy();
            } else {
                this.value = sq;
            }
            
        }

        public RuntimeTerm copy(){
            if (this.is_atom()){
                return this;
            } else if (this.is_variable()){
                var a = this.dereferenced();
                if (a.is_variable() && !a.bound()){
                    var b =  RuntimeTerm.make_empty_variable(a.name, a.id);
                    b.variable_value = a;
                    return b;
                } else {
                    return a;
                }
            } else {
                List<RuntimeTerm> new_args = new List();
                for (var val in this.variable_value.arguments){
                    new_args.push(val.copy());
                }
                var term = RuntimeTerm.make_structured_term(
                    this.variable_value.name, new_args.ToArray());
                term.copy_num = term.id;
                return term;
            }
            
        }

        public bool unify_with(RuntimeTerm other){
            const x = this.dereferenced_value();
            const y = other.dereferenced_value();

            if (x.is_atom() && y.is_atom()){
                return x.name == y.name;
            } else if (x.is_variable() && !x.bound()) {
                x.bind(y);
                return true;
            } else if (y.is_variable() && !y.bound()) {
                y.bind(x);
                return true;
            } else if (x.variable_value == y.variable_value){
                return true;
            } else {
                if (x.value.name != y.value.name ||
                    x.value.arguments.Length != y.value.arguments.Length){
                    return false;
                } else {
                    for (let i = 0; i < x.variable_value.arguments.Length; i++){
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
