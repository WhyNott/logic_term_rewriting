using System;

namespace Runtime {
        
    public static class Trail {
        public static int global_id_counter = 0;
        private static Stack<RuntimeTerm> program_stack = new Stack();
        private static Stack<int> last_choice_point = new Stack();
        public static Stack<int> last_id_counter = new Stack();

        public static void new_choice_point(){
            Trail.last_choice_point.Add(Trail.stack.length);
            Trail.last_id_counter.Add(Trail.global_id_counter);
        }
        public static void add_postbinding(RuntimeTerm p){
            Trail.program_stack.Push(p);
        }
        public static void restore_choice_point(){
            var oldtop = Trail.last_choice_point.Peek();
            for (var top = Trail.program_stack.Count; top != oldtop; top--){
                var v = Trail.program_stack.Pop();
                v.value = v;
            }
            
        }

        public static void remove_choice_point(){
            Trail.last_choice_point.Pop();
            Trail.last_id_counter.Pop();
        }
        
        
    }
}
