# set key out horiz
# set key bot center
set datafile separator ","
set title "Confidence intervals of difference of means for CPU execution times"
set ylabel "{/Symbol-Oblique m} = {/Symbol-Oblique m}_{MiXture} - {/Symbol-Oblique m}_{monolingual}: difference in CPU execution time (s)"
set xlabel "Benchmark name and input size"
set xrange [-1:6]
set yrange [-2.4:2]
set style line 1 lw 3 linecolor rgb "#193774"
set style line 2 lw 3 linecolor rgb "#F8A03F"
set xtic rotate by -45
set xzeroaxis
set term post eps enhanced color solid
set output 'diffmeans.eps'
plot "diffmean.csv" using ($0):2:4:xticlabels(1) ls 1 with errorbars ti col, "diffmean.csv" u 6:3:5 ls 2 with errorbars ti col