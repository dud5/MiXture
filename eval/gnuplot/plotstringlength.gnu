set datafile separator ","
set style data linespoints
set ylabel "Time (s)"
set xlabel "String length"
set xrange [500:21000000]
set yrange [0:20]
set title "Time to embed and project strings versus string length"
set key left top
set term post eps color linewidth 2
set style line 1 lt 1 lw 1.5 pt 3 ps 1 linecolor rgb "#193774"
set style line 2 lt 1 lw 1.5 pt 6 ps 1 linecolor rgb "#F7353E"
set output 'string_length.eps'
set format x "%.1e"
plot "string_length.csv" using 1:2:4 ls 1 with errorbars title "embed", "string_length.csv" using 1:3:5 ls 2 with errorbars title "project"