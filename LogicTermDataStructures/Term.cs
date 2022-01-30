using System;
namespace LogicTermDataStructures {
    public record Context(int line_number, int column_number, string file_name);

    public struct Identifier {
        public int id_num;
        public int compilation_number;

        private static Dictionary<string, Identifier> string_id_map = new Dictionary<string, Identifier>();
        private static List<int> comp_offset = new List<int>();
        private static List<string> ids = new List<string>();
        public static int highest_id = 0;
        public static int current_compilation_unit = 0;

        public string id_to_string() {
            var offset = Identifier.comp_offset[this.compilation_number];
            return Identifier.ids[this.id_num + offset];
        }

        public static Identifier string_to_id(string str) {
            try {
                var id = new Identifier(
                    Identifier.highest_id,
                    Identifier.current_compilation_unit
                );
                Identifier.string_id_map.Add(str, id);
                Identifier.highest_id++;
                return id;
            
            } catch (ArgumentException) {
                return Identifier.string_id_map[str];
            }
        }

        public Identifier(int id, int comp){
            this.id_num = id;              
            this.compilation_number = comp;  
        }
        
    }
    
    public abstract class Term {

        public Context context {get; set;}
    }

    public class Variable : Term {
        public string name {get; set;}
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

        public override string ToString(){
            return String.Format("?{2}{0}({1})", this.name, this.id, this.is_head ? "." : "");
        }
    }
    
    public class Sentence : Term {

        //name string is actually stored in a static dictionary and structure
        //only keeps Identifier struct that points to it
        public Identifier id;
        public string name {
            get => id.id_to_string();
            set {
                id = Identifier.string_to_id(value);
            };
        }
        public Term[] elements {get; set;}


        public override string ToString(){
            string str = this.name + "(";
            foreach (var element in this.elements) {
                str += element.ToString();
                str += ",";
            }
            return str + ")";
        }
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
