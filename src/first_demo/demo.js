// this is a JF boundary
function FLump(pointer) {
    this.pointer = pointer;
}


function apply_func(f, arg) {
    if (f instanceof FLump) {
	if (arg instanceof FLump) {
	    apply_fsharp_ff(f.pointer, arg.pointer);
	}
	else {
	    apply_fsharp_fj(f.pointer, arg);
	}
    }
    else {
	f(arg);
    }
}


function Rabbit(adjective) {
    this.adjective = adjective;
    this.speak = function(line) {
	print("The ", this.adjective, " rabbit says '", line, "'");
    };
}