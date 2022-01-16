namespace Runtime {

    public class RuntimeTerm {
        private bool variable = false;
        private bool model = false;
        public RuntimeTerm value = null;
        
        //okay, all of the confusing bullshit is in that prototype mostly

        public boolean is_variable(){
            return this.variable;
        }

        public boolean is_model(){
            return this.variable;
        }

        public boolean is_bound(){
            return this.value == this;
        }
        

        
    }

    

}
