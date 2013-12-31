set datafile separator ","
set style data lines
set xlabel "Time (ms)"
set ylabel "Number of instances of JSEngine.FSharpFunction"
set xrange [0:3500]
set yrange [0:140000]
set title "Heapshot analysis of Mixture"
set key left top
set term post eps color linewidth 2
set style line 1 lt 1 lw 1.5 pt 3 ps 1 linecolor rgb "#193774"
#set style line 2 lt 1 lw 1.5 pt 6 ps 1 linecolor rgb "#F7353E"
set output 'memory.eps'
#set format x "%.1e"
plot "memory.csv" using 1:2 ls 1 notitle w filledcu