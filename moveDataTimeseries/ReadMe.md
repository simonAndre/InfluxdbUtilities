== moveDataTimeseries

ex : 
.\moveDataTimeseries\bin\Release\netcoreapp2.1\win10-x64\moveDataTimeseries.exe explore -v -f C:\data\exportdata\parametres.csv -e 50
.\moveDataTimeseries\bin\Release\netcoreapp2.1\win10-x64\moveDataTimeseries.exe convert -v -f C:\data\exportdata\parametres.csv -e 50
.\moveDataTimeseries\bin\Release\netcoreapp2.1\win10-x64\moveDataTimeseries.exe export -v -f C:\data\exportdata\parametres.csv  --db azimut -b 50000 --tablename param
.\moveDataTimeseries\bin\Release\netcoreapp2.1\win10-x64\moveDataTimeseries.exe export -v -f C:\data\exportdata\DataIndicBruitJeu.csv  --db azimut -b 100000 --tablename indicbruit