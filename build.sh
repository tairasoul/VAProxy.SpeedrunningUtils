msbuild -p:Configuration=Release
mkdir SpeedrunningUtils
cd bin/Release/net48
cp Newtonsoft.Json.dll ../../../SpeedrunningUtils
cp SpeedrunningUtils.dll ../../../SpeedrunningUtils
cp TcpSharp.dll ../../../SpeedrunningUtils
cp SettingsAPI.dll ../../../SpeedrunningUtils
cd ..
cd ..
cd ..
rm -rf SpeedrunningUtils.zip
tar -c -f SpeedrunningUtils.zip SpeedrunningUtils
rm -rf SpeedrunningUtils