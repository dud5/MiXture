set key out horiz
set key top center
set datafile separator ","
set title "Comparison between monolingual and multilanguage CPU execution times"
set auto x
set ylabel "CPU execution time (s)"
set xlabel "Benchmark name and input size"
set yrange [0:60]
set style data histogram
set style histogram cluster gap 1
set style fill solid 1.0 noborder
#set boxwidth 0.9
set xtic rotate by -45
set term post eps enhanced color solid
set output 'monovsmix.eps'
plot "monovsmix2.csv" using 2:xtic(1) ti col linecolor rgb "#193774", '' u 3 ti col linecolor rgb "#F8A03F", '' u 4 ti col linecolor rgb "#86B784", '' u 5 ti col linecolor rgb "#F7353E"
