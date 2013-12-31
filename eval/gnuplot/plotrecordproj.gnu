A=850
B=0
C=800
D=3600
E=1
eps=0.03*E
eps2=0.005*(D-C)

set datafile separator ","
set style data linespoints
set ylabel "Time (s)"
set xlabel "Record recursion level"
set xrange [800:3600]
set yrange [0:1]
#set xtics 5000
set title "Time to project records versus record recursion level"
#set key left top
set term post eps color linewidth 2 font 'Helvetica,25'
set style line 1 lt 1 lw 1.5 pt 3 ps 1 linecolor rgb "#193774"
set style line 2 lt 1 lw 1.5 pt 6 ps 1 linecolor rgb "#F7353E"
set output 'record_proj.eps'
#set format x "%.1e"
set arrow 1 from A-eps2, 0 to A+eps2, 0 nohead lc rgb "#ffffff" front
set arrow 2 from A-eps2, E to A+eps2, E nohead lc rgb "#ffffff" front
set arrow 3 from A-eps-2*eps2, -eps to A+eps, +eps nohead front
set arrow 4 from A-eps, -eps to A+eps+2*eps2, +eps nohead front
set arrow 5 from A-eps-2*eps2, E-eps to A+eps, E+eps nohead front
set arrow 6 from A-eps, E-eps to A+eps+2*eps2, E+eps nohead front
plot "record_size_proj.csv" using 1:2:3 ls 2 with errorbars notitle