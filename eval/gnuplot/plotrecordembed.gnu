set datafile separator ","
set style data linespoints
set ylabel "Time (s)"
set xlabel "Record recursion level"
set xrange [000:51000]
set yrange [0:7]
#set xtics 5000
set title "Time to embed records versus record recursion level"
#set key left top
set term post eps color linewidth 2 font 'Helvetica,25'
set style line 1 lt 1 lw 1.5 pt 3 ps 1 linecolor rgb "#193774"
set style line 2 lt 1 lw 1.5 pt 6 ps 1 linecolor rgb "#F7353E"
set output 'record_embed.eps'
#set format x "%.1e"
plot "record_size_embed.csv" using 1:2:3 ls 1 with errorbars notitle