msbuild -p:Configuration=Release
mkdir SpeedrunningUtils
cd bin/Release/net48
cp Newtonsoft.Json.dll ../../../SpeedrunningUtils
cp SpeedrunningUtils.dll ../../../SpeedrunningUtils
cp TcpSharp.dll ../../../SpeedrunningUtils
cd ..
cd ..
cd ..
tar -c -f SpeedrunningUtils.zip SpeedrunningUtils
rm -r SpeedrunningUtils