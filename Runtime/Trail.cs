using System;
using System.Collections.Generic;

namespace Runtime {
        
    public static class Trail {
        public static int global_id_counter = 0;
        private static Stack<RuntimeTerm> program_stack = new Stack<RuntimeTerm>();
        private static Stack<int> last_choice_point = new Stack<int>();
        public static Stack<int> last_id_counter = new Stack<int>();

        public static void new_choice_point(){
            Trail.last_choice_point.Push(Trail.program_stack.Count);
            Trail.last_id_counter.Push(Trail.global_id_counter);
        }
        
        
        public static void add_postbinding(RuntimeTerm p){
            Trail.program_stack.Push(p);
        }
        public static void restore_choice_point(){
            var oldtop = Trail.last_choice_point.Peek();
            for (var top = Trail.program_stack.Count; top != oldtop; top--){
                var v = Trail.program_stack.Pop();
                v.variable_value = v;
            }
            
        }

        public static void remove_choice_point(){
            Trail.last_choice_point.Pop();
            Trail.last_id_counter.Pop();
        }
        
        
    }
}
