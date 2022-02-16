using System;

namespace Parser {
    public class Tokenizer {
        public Stack<Token> tokens = new Stack();
        private int line_count = 0;
        private int column_count = 0;
        private int index = 0;
        private String filename;
        private String file;
        private StringBuilder sentence_piece;
    

        public Tokenizer(String filename, String file){
            this.filename = filename;
            this.file = file;
        }

        private void incr(){
            index++;
            if (file[index] == '\n'){
                this.line_count++;
                this.column_count = 0;
            } else {
                this.column_count++;
            }
        }

        private void decr(){
            index--;
            //the column number will be wrong sometimes
            //but no one looks at column numbers in compiler errors anyway
            if (this.column_count == 0)
                this.line_count--;
            else
                this.column_count--;
        }

        private void emmit_sentence_piece(){
            tokens.Push(
                new SentencePiece(
                    this.filename,
                    this.line_count,
                    this.column_count,
                    this.sentence_piece.ToString()
                )
            );
            this.sentence_piece.Clear();
        }

        private bool tokenize_variable(){
            if (Char.IsLetterOrDigit(this.file[this.index+1])) {
                //previous sentence piece is over
                this.emmit_sentence_piece();

                
                for (; this.index  < this.file.Length; this.incr()){
                    if (Char.IsLetterOrDigit(this.file[this.index])){
                        this.sentence_piece.Insert(this.file[this.index]);
                    } else {
                        break;
                    }
                }
                //reaching end of file is same as reaching a non-alphanumeric char
                this.decr();
                tokens.Push(
                    new Variable(
                        this.filename,
                        this.line_count,
                        this.column_count,
                        this.sentence_piece.ToString()
                    )
                );
                this.sentence_piece.Clear();
                return true;

            
            } else {
                //regular question mark, not a variable
                return false;
            }
        }

        public Stack<Token> tokenize_file(){
            for (; this.index < this.file.Length; this.incr()){
                if (this.file[this.index] == '-'){
                    tokens.Push(
                        new Dash(
                            this.filename,
                            this.line_count,
                            this.column_count
                        )
                    );
                } else if (this.file[this.index] == ':'){
                    tokens.Push(
                        new Semicolon(
                            this.filename,
                            this.line_count,
                            this.column_count
                        )
                    );
                } else if (this.file[this.index] == '<'){
                    //@todo: shouldn't this be doing a simillar scan to the
                    //parenthesis one?
                    this.tokenize_prose_sentence();
                } else if (this.file[this.index] == '\n'){
                    //@todo: do the weird stuff with indentation here
                } else if (Char.IsWhiteSpace(this.file[this.index])){
                    continue;
                } else if (this.file[this.index] == '('){
                
                    int paren_counter = 1;
                
                    bool inner_parens = false;
                    bool bounded = false;
                    //scan until the end of line
                    for (int i = this.index+1; this.file[i] != "\n"; i++){
                        if (paren_counter == 0) {
                            bounded = true;
                            break;
                        }
                        if (this.file[i] == "(") {
                            paren_counter++;
                            inner_parens = true;
                        }

                        if (this.file[i] == ")") {
                            paren_counter--;
                        }
                    }

                    bounded = bounded || !inner_parens;

                    if (bounded)
                        this.tokenize_code_sentence();
                    else
                        this.tokenize_inline_sentence();
                } else {
                    this.tokenize_inline_sentence();
                } 
            }
            return this.tokens;
        
        
        }
        private void  tokenize_inline_sentence(){
            tokens.Push(
                new BeginSentence(
                    this.filename,
                    this.line_count,
                    this.column_count
                )
            );
            for (; this.index < this.file.Length; this.incr()){
                if (this.file[this.index] == '\n'){
                    break;
                } else if (this.file[this.index] == ':'){
                    break;
                } else if (this.file[this.index] == '?'){
                    if (!this.tokenize_variable()){
                        this.sentence_piece.Insert(this.file[this.index]);
                    } 
                } else if (this.file[this.index] == '('){
                    this.tokenize_code_sentence();
                } else if (this.file[this.index] == '<'){
                    this.tokenize_prose_sentence();
                } else if (Char.IsWhiteSpace(this.file[this.index])){
                    if (this.sentence_piece.Length != 0)
                        this.emmit_sentence_piece();
                } else {
                    this.sentence_piece.Insert(this.file[this.index]);
                }
            }
            this.decr();
            tokens.Push(
                new EndSentence(
                    this.filename,
                    this.line_count,
                    this.column_count
                )
            );
        }
        
    }   
}
