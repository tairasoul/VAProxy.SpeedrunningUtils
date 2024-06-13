msbuild -p:Configuration=Release
cpf() {
    cp "$1" ../../../SpeedrunningUtils
}

mkdir SpeedrunningUtils
(
    cd bin/Release/net48
    cpf Newtonsoft.Json.dll
    cpf SpeedrunningUtils.dll
    cpf TcpSharp.dll
    cpf ObsWebSocket.Net.dll
    cpf MessagePack.dll
    cpf MessagePack.Annotations.dll
    cpf System.Text.Json.dll
    cpf Microsoft.Bcl.HashCode.dll
    cpf Microsoft.Bcl.AsyncInterfaces.dll
    cpf Microsoft.NET.StringTools.dll
    cpf System.Buffers.dll
    cpf System.Memory.dll
    cpf System.Collections.Immutable.dll
    cpf System.Numerics.Vectors.dll
    cpf System.Runtime.CompilerServices.Unsafe.dll
    cpf System.Text.Encodings.Web.dll
    cpf System.Text.Json.dll
    cpf System.Threading.Tasks.Extensions.dll
    cpf System.ValueTuple.dll
)
rm -rf SpeedrunningUtils.tar.xz
tar -c -f SpeedrunningUtils.tar.xz SpeedrunningUtils
rm -rf SpeedrunningUtils