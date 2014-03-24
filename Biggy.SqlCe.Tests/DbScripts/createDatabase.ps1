$sqlCeCmd = "c:\Apps\sqlce\SqlCeCmd40.exe"
$dbFolder = "../App_Data"


if (Test-Path $dbFolder) { 
	rm -Recurse -Force "../App_Data"
}
mkdir $dbFolder > $nil


$connStr = "Data Source=$dbFolder/chinook.sdf;Persist Security Info=False;"
#echo $connStr

&$sqlCeCmd -d $connStr -e create
&$sqlCeCmd -d $connStr -i Chinook_SqlServer_AutoIncrementPKs.sql
