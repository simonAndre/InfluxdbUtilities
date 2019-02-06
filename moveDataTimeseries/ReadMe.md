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
%EXEPATH%\moveDataTimeseries.exe explore -v2 -fC:\data\exportdata\parametres.csv -e50 -tParametres
%EXEPATH%\moveDataTimeseries.exe explore -v1 -fC:\data\exportdata\Data_2019.csv -tdata -e100
```
* convert the file for import and split out files (influxdb can't import files more than 25Mb)
```
%EXEPATH%\moveDataTimeseries.exe convert -v1 -fC:\data\exportdata\parametres.csv -e50 -tParametres
%EXEPATH%\moveDataTimeseries.exe convert -v1 -fC:\data\exportdata\DataJeu1.csv -tdata -e100
```
* Export data directly to a database in batch of 50000 points, renaming the measurement to `param`
```
%EXEPATH%\moveDataTimeseries.exe export -v1 -fC:\data\exportdata\parametres.csv  --db=azimut -b50000 --tablename=param -tParametres
%EXEPATH%\moveDataTimeseries.exe export -v1 --db=azimut -fC:\data\exportdata\DataJeu1.csv -tdata -b50000 
%EXEPATH%\moveDataTimeseries.exe export -v1 --db=azimut -fC:\data\exportdata\DataIndicBruit_2019.csv -tdataindicbruit -b50000 
```
* another direct export
```
%EXEPATH%\moveDataTimeseries.exe export -v -fC:\data\exportdata\DataIndicBruitJeu.csv  --db=azimut -b100000 -tindicbruit
%EXEPATH%\moveDataTimeseries.exe export -v -fC:\data\exportdata\DataJeu1.csv  --db=azimut -b100000 -tData
```


## development configuration with Docker

docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

puis 
``` 
docker-compose run movedatatimeseries [COMMAND] [args]
```

ex : 
```
docker-compose run movedatatimeseries explore -v2 -f'/data/parametres.csv' -e50 -tParametres
``` 
