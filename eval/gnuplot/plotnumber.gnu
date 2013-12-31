set datafile separator ","
set style data linespoints
set ylabel "CPU execution time (s)"
set xlabel "Number of elements"
set xrange [500:1100000]
set yrange [0:320]
set title "Time to embed and project different F# datatypes"
set key left top
set term post eps color
set style line 1 lw 3
set style line 2 lw 3
set style line 3 lw 3
set style line 4 lw 3
set style line 5 lw 3
set style line 6 lw 3
set style line 7 lw 3
set style line 8 lw 3
set style line 9 lw 3
set output 'datatypecomparison2.eps'
set format x "%.0e"
set key samplen 2
plot "number_of_values_raw.csv" using 1:7:16 ls 9 with errorbars title "id (polymorphic)", "number_of_values_raw.csv" using 1:2:11 ls 1 with errorbars title "add function", "number_of_values_raw.csv" using 1:10:19 ls 2 with errorbars title "website/object", "number_of_values_raw.csv" using 1:5:14 ls 3 with errorbars title "int_record/object", "number_of_values_raw.csv" using 1:4:13 ls 8 with errorbars title "int array", "number_of_values_raw.csv" using 1:6:15 ls 4 with errorbars title "int/Number", "number_of_values_raw.csv" using 1:9:18 ls 7 with errorbars title "unit/Undefined", "number_of_values_raw.csv" using 1:3:12 ls 5 with errorbars title "float/Number", "number_of_values_raw.csv" using 1:8:17 ls 6 with errorbars title "string/String"