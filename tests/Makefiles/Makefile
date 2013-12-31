# The Computer Language Benchmarks Game

# ASSUME each program will build in a clean empty tmpdir
# ASSUME there's a symlink to the program source in tmpdir
# ASSUME there's a symlink to the Include directory in tmpdir
# ASSUME there are symlinks to helper files in tmpdir
# ASSUME no responsibility for removing temporary files from tmpdir

# TYPICAL actions include an initial mv to give the expected extension 

# ASSUME environment variables for compilers and interpreters are set in the header


COPTS := -O3 -fomit-frame-pointer



############################################################
# ACTIONS for specific language implementations
############################################################

########################################
# F#
########################################

%.fs: %.fsharp $(FSHARPC)
	-mv $< $@

%.fsharp_run: %.fs
	-$(FSHARPC) --target:exe --platform:x86 -O $(FSHARPOPTS) -o $@.exe $<

# %.fsharp_run: %.fs $(FSHARPC)
# 	-mv $< $*.fs
# 	-$(FSHARPC) --target:exe --platform:x86 -O $(FSHARPOPTS) -o $@.exe $<

fasta.fsharp_run:
	mv fasta.fsharp fasta.fs
	fsharpc -O fasta.fs -o fasta.fsharp_run.exe

binarytrees3.fsharp_run:
	mv binarytrees3.fsharp binarytrees3.fs
	fsharpc -O binarytrees3.fs -o binarytrees3.fsharp_run.exe
mandelbrot.fsharp-3.fsharp_run:
	mv mandelbrot.fsharp-3.fsharp mandelbrot.fs
	fsharpc -O mandelbrot.fs -o mandelbrot.fsharp-3.fsharp_run.exe
fannkuchredux.fsharp-3.fsharp_run:
	mv fannkuchredux.fsharp-3.fsharp fannkuchredux.fs
	fsharpc -O fannkuchredux.fs -o fannkuchredux.fsharp-3.fsharp_run.exe
knucleotide.fsharp_run:
	mv knucleotide.fsharp knucleotide.fs
	fsharpc -O knucleotide.fs -o knucleotide.fsharp_run.exe

regexdna.fsharp-3.fsharp_run:
	mv regexdna.fsharp-3.fsharp regexdna3.fs
	fsharpc -O regexdna3.fs -o regexdna.fsharp-3.fsharp_run.exe

regexdna.fsharp-2.fsharp_run:
	mv regexdna.fsharp-2.fsharp regexdna2.fs
	fsharpc -O regexdna2.fs -o regexdna.fsharp-2.fsharp_run.exe

regexdna.fsharp_run:
	mv regexdna.fsharp regexdna.fs
	fsharpc -O regexdna.fs -o regexdna.fsharp_run.exe

nbody.fsharp_run:
	mv nbody.fsharp nbody.fs
	fsharpc -O nbody.fs -o nbody.fsharp_run.exe

spectralnorm.fsharp-2.fsharp_run:
	mv spectralnorm.fsharp-2.fsharp spectralnorm.fs
	fsharpc -O spectralnorm.fs -o spectralnorm.fsharp-2.fsharp_run.exe

integers.fsharp_run:
	mv integers.fsharp integers.fs
	fsharpc -r:Mixture.dll -O integers.fs -o integers.fsharp_run.exe

poly.fsharp_run:
	mv poly.fsharp poly.fs
	fsharpc -r:Mixture.dll -O poly.fs -o poly.fsharp_run.exe

websites.fsharp_run:
	mv websites.fsharp websites.fs
	fsharpc -r:Mixture.dll -O websites.fs -o websites.fsharp_run.exe

int_re.fsharp_run:
	mv int_re.fsharp int_re.fs
	fsharpc -r:Mixture.dll -O int_re.fs -o int_re.fsharp_run.exe

int_arr.fsharp_run:
	mv int_arr.fsharp int_arr.fs
	fsharpc -r:Mixture.dll -O int_arr.fs -o int_arr.fsharp_run.exe

add.fsharp_run:
	mv add.fsharp add.fs
	fsharpc -r:Mixture.dll -O add.fs -o add.fsharp_run.exe

undefined.fsharp_run:
	mv undefined.fsharp undefined.fs
	fsharpc -r:Mixture.dll -O undefined.fs -o undefined.fsharp_run.exe


strings.fsharp_run:
	mv strings.fsharp strings.fs
	fsharpc -r:Mixture.dll -O strings.fs -o strings.fsharp_run.exe

floats.fsharp_run:
	mv floats.fsharp floats.fs
	fsharpc -r:Mixture.dll -O floats.fs -o floats.fsharp_run.exe

string_length_proj.fsharp_run:
	mv string_length_proj.fsharp string_length_proj.fs
	fsharpc -r:Mixture.dll -O string_length_proj.fs -o string_length_proj.fsharp_run.exe

string_length_embed.fsharp_run:
	mv string_length_embed.fsharp string_length_embed.fs
	fsharpc -r:Mixture.dll -O string_length_embed.fs -o string_length_embed.fsharp_run.exe


record_size_proj.fsharp_run:
	mv record_size_proj.fsharp record_size_proj.fs
	fsharpc -r:Mixture.dll -O record_size_proj.fs -o record_size_proj.fsharp_run.exe

record_size_embed.fsharp_run:
	mv record_size_embed.fsharp record_size_embed.fs
	fsharpc -r:Mixture.dll -O record_size_embed.fs -o record_size_embed.fsharp_run.exe


array_size.fsharp_run:
	mv array_size.fsharp array_size.fs
	fsharpc -r:Mixture.dll -O array_size.fs -o array_size.fsharp_run.exe

tuple_size.fsharp_run:
	mv tuple_size.fsharp tuple_size.fs
	fsharpc -r:Mixture.dll -O tuple_size.fs -o tuple_size.fsharp_run.exe

binarytreesjsinfs.fsharp_run:
	mv binarytreesjsinfs.fsharp binarytreesjsinfs.fs
	fsharpc -r:Mixture.dll -O binarytreesjsinfs.fs -o binarytreesjsinfs.fsharp_run.exe

binarytreesfsinjs.fsharp_run:
	mv binarytreesfsinjs.fsharp binarytreesfsinjs.fs
	fsharpc -r:Mixture.dll -O binarytreesfsinjs.fs -o binarytreesfsinjs.fsharp_run.exe

knucleotidejsinfs.fsharp_run:
	mv knucleotidejsinfs.fsharp knucleotidejsinfs.fs
	fsharpc -r:Mixture.dll -O knucleotidejsinfs.fs -o knucleotidejsinfs.fsharp_run.exe

regexdnajsinfs.fsharp_run:
	mv regexdnajsinfs.fsharp regexdnajsinfs.fs
	fsharpc -r:Mixture.dll -O regexdnajsinfs.fs -o regexdnajsinfs.fsharp_run.exe


nbodyjsinfs.fsharp_run:
	mv nbodyjsinfs.fsharp nbodyjsinfs.fs
	fsharpc -r:Mixture.dll -O nbodyjsinfs.fs -o nbodyjsinfs.fsharp_run.exe

nbodyfsinjs.fsharp_run:
	mv nbodyfsinjs.fsharp nbodyfsinjs.fs
	fsharpc -r:Mixture.dll -O nbodyfsinjs.fs -o nbodyfsinjs.fsharp_run.exe

mandelbrotjsinfs.fsharp_run:
	mv mandelbrotjsinfs.fsharp mandelbrotjsinfs.fs
	fsharpc -r:Mixture.dll -O mandelbrotjsinfs.fs -o mandelbrotjsinfs.fsharp_run.exe

mandelbrotfsinjs.fsharp_run:
	mv mandelbrotfsinjs.fsharp mandelbrotfsinjs.fs
	fsharpc -r:Mixture.dll -O mandelbrotfsinjs.fs -o mandelbrotfsinjs.fsharp_run.exe

fastajsinfs.fsharp_run:
	mv fastajsinfs.fsharp fastajsinfs.fs
	fsharpc -r:Mixture.dll -O fastajsinfs.fs -o fastajsinfs.fsharp_run.exe

fastafsinjs.fsharp_run:
	mv fastafsinjs.fsharp fastafsinjs.fs
	fsharpc -r:Mixture.dll -O fastafsinjs.fs -o fastafsinjs.fsharp_run.exe

spectralnormjsinfs.fsharp_run:
	mv spectralnormjsinfs.fsharp spectralnormjsinfs.fs
	fsharpc -r:Mixture.dll -O spectralnormjsinfs.fs -o spectralnormjsinfs.fsharp_run.exe

spectralnormfsinjs.fsharp_run:
	mv spectralnormfsinjs.fsharp spectralnormfsinjs.fs
	fsharpc -r:Mixture.dll -O spectralnormfsinjs.fs -o spectralnormfsinjs.fsharp_run.exe

fannkuchreduxjsinfs.fsharp_run:
	mv fannkuchreduxjsinfs.fsharp fannkuchreduxjsinfs.fs
	fsharpc -r:Mixture.dll -O fannkuchreduxjsinfs.fs -o fannkuchreduxjsinfs.fsharp_run.exe

fannkuchreduxfsinjs.fsharp_run:
	mv fannkuchreduxfsinjs.fsharp fannkuchreduxfsinjs.fs
	fsharpc -r:Mixture.dll -O fannkuchreduxfsinjs.fs -o fannkuchreduxfsinjs.fsharp_run.exe

