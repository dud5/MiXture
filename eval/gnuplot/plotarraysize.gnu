set datafile separator ","
set style data linespoints
set ylabel "Time (s)"
set xlabel "Array length"
set xrange [500:400000]
set yrange [0:85]
set title "Time to embed and project arrays versus array length"
#set key left top
set term post eps color linewidth 2
set output 'array_length.eps'
set format x "%.0e"
plot "array_size.csv" using 1:2 title ""