rem export data from SQL server to CSV files

bcp "select AffaireID,voieid,TypeVoieID,ntsid,date,TypeCanalID,ConvertedValue from datapubli where Annee>=2019" queryout data_2019.csv -t, -c -S azimut-database.database.windows.net -U rouser -d azimut-servicesdb
bcp "select fm.AffaireID, date,value,code,FicheMetierid,EmplacementID,typeindicateurid from DataIndicBruitJeu d join FicheMetierJeu fm on fm.id=d.FicheMetierid where DATEPART(yy,date)=2019" queryout DataIndicBruit_2019.csv -t, -c -S azimut-database.database.windows.net -U rouser -d azimut-servicesdb
bcp "select top 100 * from [dataindicateur] where DATEPART(yy,date)=2019 and affaireid=21" queryout DataIndicJeu.csv -t, -c -S azimut-database.database.windows.net -U rouser -d azimut-servicesdb
bcp "select voieid,paramname,value,date,typedonneeenum from [v_paramvalue]" queryout parametres.csv -t, -c -S azimut-database.database.windows.net -U rouser -d azimut-servicesdb