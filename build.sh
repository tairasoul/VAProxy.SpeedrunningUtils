dotnet build -p:Configuration=RelWithDebInfo
rm ../vap-gamefolder/BepInEx/plugins/speedrunningutils.dll
cp bin/RelWithDebInfo/net48/speedrunningutils.dll ../vap-gamefolder/BepInEx/plugins/
rm ../vap-gamefolder/BepInEx/plugins/speedrunningutils.pdb
cp bin/RelWithDebInfo/net48/speedrunningutils.pdb ../vap-gamefolder/BepInEx/plugins/