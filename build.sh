msbuild -p:Configuration=Release
mkdir SpeedrunningUtils
cd bin/Release/net48
cp Newtonsoft.Json.dll ../../../SpeedrunningUtils
cp SpeedrunningUtils.dll ../../../SpeedrunningUtils
cp TcpSharp.dll ../../../SpeedrunningUtils
cp UIWindowPageFramework.dll ../../../SpeedrunningUtils
cd ..
cd ..
cd ..
rm -rf SpeedrunningUtils.tar.xz
tar -c -f SpeedrunningUtils.tar.xz SpeedrunningUtils
rm -rf SpeedrunningUtils