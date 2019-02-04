rem export data from SQL server to CSV files

bcp "select AffaireID,voieid,TypeVoieID,ntsid,date,TypeCanalID,ConvertedValue from datapubli where AffaireID=21 and Annee>=2019" queryout datajeu1.csv -t, -c -S azimut-database.database.windows.net -U rouser -d azimut-servicesdb
bcp "select affaireid,d.* from DataIndicBruitJeu d join FicheMetierJeu fm on fm.id=d.FicheMetierid where AffaireID=21 and DATEPART(yy,date)=2019" queryout DataIndicBruitJeu.csv -t, -c -S azimut-database.database.windows.net -U rouser -d azimut-servicesdb
bcp "select top 100 * from [dataindicateur] where DATEPART(yy,date)=2019 and affaireid=21" queryout DataIndicJeu.csv -t, -c -S azimut-database.database.windows.net -U rouser -d azimut-servicesdb
bcp "select voieid,paramname,value,date,typedonneeenum from [v_paramvalue]" queryout parametres.csv -t, -c -S azimut-database.database.windows.net -U rouser -d azimut-servicesdb