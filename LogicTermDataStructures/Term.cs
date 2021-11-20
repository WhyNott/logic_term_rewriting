namespace LogicTermDataStructures {
    public record Context(int line_number, int column_number, string file_name);

    public abstract class Term {
        public string name {get; set;}
        public Context context {get; set;}
    }

    public class Variable : Term {
        public bool is_head {get; set;}
        public int id {get; set;}

        private static int top_free_id = 0;

        public override int GetHashCode() {
            return this.id;

        }

        public Variable(string name, Context context, bool is_head) {
            this.name = name;
            this.context = context;
            this.is_head = is_head;
            this.id = Variable.top_free_id;
            
            Variable.top_free_id++;
        }

        public Variable(Context context, bool is_head) {
            this.context = context;
            this.is_head = is_head;
            this.id = Variable.top_free_id;

            this.name = "unn_" + this.id;
            
            Variable.top_free_id++;
        }

        public Variable(string name, Context context) {
            this.name = name;
            this.context = context;
            this.is_head = false;
            this.id = Variable.top_free_id;
            
            Variable.top_free_id++;
        }

        public void Deconstruct(out string name, out Context context,
                           out bool is_head, out int id) {
            name    = this.name;
            context = this.context;
            is_head = this.is_head;
            id      = this.id;
        }
        
        public void Deconstruct(out string name, out Context context, out bool is_head) {
            name    = this.name;
            context = this.context;
            is_head = this.is_head;
        }

        public void Deconstruct(out string name, out Context context) {
            name    = this.name;
            context = this.context;
        }
    }
    
    public class Sentence : Term {
        public Term[] elements {get; set;}

        public Sentence(string name, Term[] elements, Context context){
            this.name = name;
            this.context = context;
            this.elements = elements; 
        }

        public void Deconstruct(out string name, out Term[] elements, out Context context){
            name     = this.name;
            context  = this.context;
            elements = this.elements;
        }
        
    }
    
}
