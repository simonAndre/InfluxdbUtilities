# moveDataTimeseries

Util to import to influxDb data from csv files.

3 usages :
* Explore : simple exploration of the content of the file rendered in the influx line protocole
* Convert : file conversion to the influxDb line protocole. If files are too big, the program split them in bunchs smaller than the limited influxdb import file size (<25Mb)
* Export : Export data directly to a database

to get full doc on options, use `moveDataTimeseries --help`

### examples 

* Explore the content of file
```
set EXEPATH=.\moveDataTimeseries\bin\Release\netcoreapp2.1\win10-x64
%EXEPATH%\moveDataTimeseries.exe explore -v -f C:\data\exportdata\parametres.csv -e 50 -t Parametres
%EXEPATH%\moveDataTimeseries.exe explore -v -f C:\data\exportdata\DataJeu1.csv -t data -e 100
```
* convert the file for import and split out files (influxdb can't import files more than 25Mb)
```
%EXEPATH%\moveDataTimeseries.exe convert -v -f C:\data\exportdata\parametres.csv -e 50 -t Parametres
%EXEPATH%\moveDataTimeseries.exe convert -v -f C:\data\exportdata\DataJeu1.csv -t data -e 100
```
* Export data directly to a database in batch of 50000 points, renaming the measurement to `param`
```
%EXEPATH%\moveDataTimeseries.exe export -v -f C:\data\exportdata\parametres.csv  --db azimut -b 50000 --tablename param -t Parametres
%EXEPATH%\moveDataTimeseries.exe export -v --db azimut -b 50000 -f C:\data\exportdata\DataJeu1.csv -t data -e 100
```
* another direct export
```
%EXEPATH%\moveDataTimeseries.exe export -v -f C:\data\exportdata\DataIndicBruitJeu.csv  --db azimut -b 100000 -t indicbruit
%EXEPATH%\moveDataTimeseries.exe export -v -f C:\data\exportdata\DataJeu1.csv  --db azimut -b 100000 -t Data
```
